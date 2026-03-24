using ImageMagick;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

namespace LiveWallpaper.ML
{
    public class DepthEstimator
    {
        private InferenceSession _session;
        private string _inputName;
        private int _width, _height;

        public void LoadModel(string modelPath)
        {
            if (!File.Exists(modelPath)) return;
            _session = new InferenceSession(modelPath);
            _inputName = _session.InputMetadata.Keys.First();
            _width = _session.InputMetadata[_inputName].Dimensions[2];
            _height = _session.InputMetadata[_inputName].Dimensions[3];
        }

        public void GenerateDepthMap(string inputImagePath, string outputDirectory)
        {
            using var inputImage = new MagickImage(inputImagePath);
            var originalWidth = inputImage.Width;
            var originalHeight = inputImage.Height;

            inputImage.Resize(new MagickGeometry((uint)_width, (uint)_height) { IgnoreAspectRatio = true });

            var tensor = new DenseTensor<float>(new[] { 1, 3, _height, _width });
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    using var pixels = inputImage.GetPixels();
                    var color = pixels.GetPixel(x, y)?.ToColor() ?? new MagickColor(0,0,0);
                    tensor[0, 0, y, x] = color.R / 255f;
                    tensor[0, 1, y, x] = color.G / 255f;
                    tensor[0, 2, y, x] = color.B / 255f;
                }
            }

            var inputs = new List<NamedOnnxValue>() { NamedOnnxValue.CreateFromTensor<float>(_inputName, tensor) };
            
            using var results = _session.Run(inputs);
            var outputModel = results.First().AsEnumerable<float>().ToArray();
            var normalisedOutput = NormaliseOutput(outputModel);

            using var depthImage = new MagickImage(MagickColors.Black, (uint)_width, (uint)_height);
            using var depthPixels = depthImage.GetPixels();
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    ushort val = (ushort)(normalisedOutput[y * _width + x] * 65535f);
                    depthPixels.SetPixel(x, y, new ushort[] { val, val, val }); 
                }
            }
            
            depthImage.Resize(new MagickGeometry((uint)originalWidth, (uint)originalHeight) { IgnoreAspectRatio = true });
            
            Directory.CreateDirectory(outputDirectory);
            File.Copy(inputImagePath, Path.Combine(outputDirectory, "image.jpg"), true);
            depthImage.Write(Path.Combine(outputDirectory, "depth.jpg"));
        }

        private static float[] NormaliseOutput(float[] data)
        {
            var max = data.Max();
            var min = data.Min();
            var range = max - min;
            return data.Select(d => (d - min) / range).Select(n => (1f - n) * 0f + n * 1f).ToArray();
        }
    }
}
