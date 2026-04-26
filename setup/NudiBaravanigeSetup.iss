; -------------------------------------------------
; Kannada Nudi Baraha – Inno Setup Script
; -------------------------------------------------

; -----------------------------
; Application Metadata
; -----------------------------
#define MyAppName "KannadaNudiBaraha"
#define MyAppPublisher "KAGAPA"
#define MyAppURL "https://kagapa.com/"
#define MyAppExeName "KannadaNudiEditor.exe"
#define MyAppVersion "1.0.0"  ; Will be replaced dynamically by CI

; -----------------------------
; Setup Configuration
; -----------------------------
[Setup]
AppId={{E0BD2D2E-D1E1-4AF0-99D7-8663ACCFB0B4}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

DisableProgramGroupPage=yes
WizardStyle=modern
SolidCompression=yes
Compression=lzma2

OutputDir=Output
OutputBaseFilename=KannadaNudiEditor_{#MyAppVersion}

SetupIconFile=..\Assets\nudi.ico
LicenseFile=license.txt
InfoBeforeFile=readme_before.txt
InfoAfterFile=readme_after.txt

; -----------------------------
; Language
; -----------------------------
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

; -----------------------------
; Optional Tasks
; -----------------------------
[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

; -----------------------------
; Files to Include
; -----------------------------
[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; -----------------------------
; Icons
; -----------------------------
[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

; -----------------------------
; Run After Installation
; -----------------------------
[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

; -----------------------------
; Registry Entries (file associations)
; -----------------------------
[Registry]
; Register the app itself
Root: HKCR; Subkey: "Applications\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Applications\{#MyAppExeName}\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

; Existing OpenWithProgids
Root: HKCR; Subkey: ".txt\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".rtf\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".docx\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".html\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".htm\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue

; -----------------------------
; Register .nudi file type
; -----------------------------
Root: HKCR; Subkey: ".nudi"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".nudi\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: "{#MyAppName}\DefaultIcon"; ValueType: string; ValueData: "{app}\{#MyAppExeName},0"; Flags: uninsdeletekey
Root: HKCR; Subkey: "{#MyAppName}\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey
