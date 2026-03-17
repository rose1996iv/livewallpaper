#define MyAppName "WallpaperEngine"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "WallpaperEngine"
#ifndef Runtime
  #define Runtime "win-x64"
#endif
#define MyPublishDir AddBackslash(SourcePath) + "..\dist\publish\" + Runtime

[Setup]
AppId={{0E6A4FE6-AC39-4F20-971C-31F7F6AFA9B8}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\WallpaperEngine.UI.exe
Compression=lzma
SolidCompression=yes
WizardStyle=modern
OutputDir=..\dist\installer
OutputBaseFilename=WallpaperEngine-Setup
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\WallpaperEngine"; Filename: "{app}\WallpaperEngine.UI.exe"
Name: "{autodesktop}\WallpaperEngine"; Filename: "{app}\WallpaperEngine.UI.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked

[Run]
Filename: "{app}\WallpaperEngine.UI.exe"; Description: "Launch WallpaperEngine"; Flags: nowait postinstall skipifsilent
