; -------------------------------------------------
; Kannada Nudi Baraha – Inno Setup Script (CI Ready)
; -------------------------------------------------

#define MyAppName "KannadaNudiBaraha"
#define MyAppPublisher "KAGAPA"
#define MyAppURL "https://kagapa.com/"
#define MyAppExeName "KannadaNudiEditor.exe"

; Version handling: GitHub Actions tag or fallback
#ifdef GITHUB_REF_NAME
  #define MyAppVersion Copy(GetEnv("GITHUB_REF_NAME"), 2)
#else
  #define MyAppVersion "1.0.0-dev"
#endif

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

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCR; Subkey: "Applications\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Applications\{#MyAppExeName}\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKCR; Subkey: ".txt\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".rtf\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".docx\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".html\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".htm\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppExeName}"; ValueData: ""; Flags: uninsdeletevalue

[Code]
function HasDotNet8Desktop(): Boolean;
var
  FindRec: TFindRec;
  BasePath: string;
begin
  Result := False;
  BasePath := ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App');
  if not DirExists(BasePath) then Exit;

  if FindFirst(BasePath + '\8.0.*', FindRec) then
  begin
    try
      repeat
        if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
        begin
          Result := True;
          Exit;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := HasDotNet8Desktop();
  if not Result then
    MsgBox(
      'Microsoft .NET 8 Desktop Runtime (x64) is required.' + #13#10#13#10 +
      'https://dotnet.microsoft.com/download/dotnet/8.0',
      mbError, MB_OK);
end;
