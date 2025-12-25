; Kannada Nudi Editor / Inno Setup Script
; Works with GitHub Actions build pipeline

#define MyAppName "KannadaNudiBaraha"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "KAGAPA"
#define MyAppURL "https://kagapa.com/"
#define MyAppExeName "KannadaNudiEditor.exe"

; SourceDir is provided by GitHub Actions `/DSourceDir=...`
#define SourceDir GetEnv("SourceDir")

[Setup]
AppId={{E0BD2D2E-D1E1-4AF0-99D7-8663ACCFB0B4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputBaseFilename=KannadaNudiEditorSetup
OutputDir=installer

SetupIconFile=Assets\nudi.ico
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Copy all published files to install folder
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
