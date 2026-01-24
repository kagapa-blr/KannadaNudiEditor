; -------------------------------------------------
; Kannada Nudi Baraha – Inno Setup Script (CI Ready)
; -------------------------------------------------

#define MyAppName "KannadaNudiBaraha"
#define MyAppPublisher "KAGAPA"
#define MyAppURL "https://kagapa.com/"
#define MyAppExeName "KannadaNudiEditor.exe"

; =================================================
; VERSION HANDLING
; - GitHub Actions: v1.2.3  →  1.2.3
; - Local build fallback:   1.0.0-dev
; =================================================
#ifdef GITHUB_REF_NAME
  #define MyAppVersion Copy(GetEnv("GITHUB_REF_NAME"), 2)
#else
  #define MyAppVersion "1.0.0-dev"
#endif

[Setup]
; -------------------------------
; REQUIRED FIX: Correct AppId syntax
; -------------------------------
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

; Output folder (CI-friendly)
OutputDir=Output
OutputBaseFilename=KannadaNudiEditor_{#MyAppVersion}

; Relative paths for CI
SetupIconFile=..\Assets\nudi.ico
LicenseFile=license.txt
InfoBeforeFile=readme_before.txt
InfoAfterFile=readme_after.txt

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}";
GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

; -------------------------------------------------
; FILES (dotnet publish output)
; -------------------------------------------------
[Files]
Source: "..\publish\*"; DestDir: "{app}";
Flags: ignoreversion recursesubdirs createallsubdirs

; -------------------------------------------------
; ICONS
; -------------------------------------------------
[Icons]
Name: "{autoprograms}\{#MyAppName}";
Filename: "{app}\{#MyAppExeName}"

Name: "{autodesktop}\{#MyAppName}";
Filename: "{app}\{#MyAppExeName}";
Tasks: desktopicon

; -------------------------------------------------
; RUN AFTER INSTALL
; -------------------------------------------------
[Run]
Filename: "{app}\{#MyAppExeName}";
Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}";
Flags: nowait postinstall skipifsilent

; -------------------------------------------------
; FILE ASSOCIATIONS (Open With)
; -------------------------------------------------
[Registry]
Root: HKCR; Subkey: "Applications\{#MyAppExeName}"; Flags: uninsdeletekey

Root: HKCR; Subkey: "Applications\{#MyAppExeName}\shell\open\command";
ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

Root: HKCR; Subkey: ".txt\OpenWithProgids";
ValueType: string; ValueName: "{#MyAppExeName}";
ValueData: ""; Flags: uninsdeletevalue

Root: HKCR; Subkey: ".rtf\OpenWithProgids";
ValueType: string; ValueName: "{#MyAppExeName}";
ValueData: ""; Flags: uninsdeletevalue

Root: HKCR; Subkey: ".docx\OpenWithProgids";
ValueType: string; ValueName: "{#MyAppExeName}";
ValueData: ""; Flags: uninsdeletevalue

Root: HKCR; Subkey: ".html\OpenWithProgids";
ValueType: string; ValueName: "{#MyAppExeName}";
ValueData: ""; Flags: uninsdeletevalue

Root: HKCR; Subkey: ".htm\OpenWithProgids";
ValueType: string; ValueName: "{#MyAppExeName}";
ValueData: ""; Flags: uninsdeletevalue

; -------------------------------------------------
; .NET 8 DESKTOP RUNTIME CHECK
; -------------------------------------------------
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
