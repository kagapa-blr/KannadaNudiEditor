; -------------------------------------------------
; Kannada Nudi Baraha – Inno Setup Script
; -------------------------------------------------

#define MyAppName "KannadaNudiBaraha"
#define MyAppPublisher "KAGAPA"
#define MyAppURL "https://kagapa.com/"
#define MyAppExeName "KannadaNudiEditor.exe"
#define MyAppVersion "1.0.0"

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
; -----------------------------------------------------
; Function: IsDotNet8Installed
; Purpose: Check if Microsoft .NET 8 Desktop Runtime
;          is installed on the machine.
; Returns: True if .NET 8 found, False otherwise.
; -----------------------------------------------------
function IsDotNet8Installed(): Boolean;
var
  FindRec: TFindRec;
  BasePath: string;
begin
  Result := False;

  ; Default installation folder for .NET Desktop runtimes
  BasePath := ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App');

  ; Check if the folder exists
  if DirExists(BasePath) then
  begin
    ; Look for a folder starting with 8.0
    if FindFirst(BasePath + '\8.0.*', faDirectory, FindRec) then
    begin
      try
        repeat
          ; Only check directories
          if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
          begin
            Result := True; ; .NET 8 found
            Exit;
          end;
        until not FindNext(FindRec);
      finally
        FindClose(FindRec);
      end;
    end;
  end;
end;

; -----------------------------------------------------
; Function: InitializeSetup
; Purpose: Runs before installation starts.
;          Checks prerequisites like .NET 8.
; Returns: True if installer should continue, False to abort.
; -----------------------------------------------------
function InitializeSetup(): Boolean;
begin
  ; Check for .NET 8 Desktop Runtime
  if not IsDotNet8Installed() then
  begin
    ; Show error message and abort installation
    MsgBox(
      'Microsoft .NET 8 Desktop Runtime is required to install this application.'#13#10 +
      'Please install it from https://dotnet.microsoft.com/en-us/download/dotnet/8.0',
      mbError, MB_OK
    );
    Result := False; ; Stop installation
  end
  else
    Result := True; ; Continue installation
end;
