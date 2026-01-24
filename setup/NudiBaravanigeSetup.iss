; -------------------------------------------------
; Kannada Nudi Baraha – Inno Setup Script (CI Ready)
; -------------------------------------------------

#define MyAppName "KannadaNudiBaraha"
#define MyAppPublisher "KAGAPA"
#define MyAppURL "https://kagapa.com/"
#define MyAppExeName "KannadaNudiEditor.exe"

; Version handled dynamically by GitHub Actions
#define MyAppVersion "1.0.0"

; CI-friendly source folder (relative to repository)
#define SourceDir "setup"

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
; Use relative path for CI
Source: "{#SourceDir}\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

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
; -------------------------------------------------
; Simple console logging for CI
; -------------------------------------------------
procedure Log(Message: string);
var
  ResultCode: Integer;
begin
  Exec('cmd.exe', '/C echo ::notice::' + Message, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

; -------------------------------------------------
; Log all files in publish folder at runtime
; -------------------------------------------------
procedure LogPublishFiles();
var
  FindRec: TFindRec;
  PublishPath: string;
begin
  PublishPath := ExpandConstant('{#SourceDir}\publish');
  Log('Checking publish folder: ' + PublishPath);

  if not DirExists(PublishPath) then
  begin
    Log('Publish folder does not exist!');
    Exit;
  end;

  if FindFirst(PublishPath + '\*', faAnyFile, FindRec) then
  begin
    repeat
      if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) = 0 then
        Log('Publish folder contains: ' + FindRec.Name);
    until not FindNext(FindRec);
    FindClose(FindRec);
  end
  else
    Log('Publish folder is empty!');
end;

; -------------------------------------------------
; Check for .NET 8 Desktop Runtime
; -------------------------------------------------
function HasDotNet8Desktop(): Boolean;
var
  FindRec: TFindRec;
  BasePath: string;
begin
  BasePath := ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App');
  Log('Checking .NET runtime in: ' + BasePath);

  Result := False;
  if not DirExists(BasePath) then
  begin
    Log('Base path not found.');
    Exit;
  end;

  if FindFirst(BasePath + '\8.0.*', FindRec) then
  begin
    try
      repeat
        if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
        begin
          Log('Found .NET 8 folder: ' + FindRec.Name);
          Result := True;
          Exit;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end
  else
    Log('No 8.0.* folders found.');
end;

; -------------------------------------------------
; Setup initialization
; -------------------------------------------------
function InitializeSetup(): Boolean;
begin
  Log('Initializing setup...');
  LogPublishFiles();
  Result := HasDotNet8Desktop();
  if not Result then
    Log('Microsoft .NET 8 Desktop Runtime not found! Setup will exit.');
end;

; -------------------------------------------------
; Optional: log installation steps
; -------------------------------------------------
procedure CurStepChanged(CurStep: TSetupStep);
begin
  case CurStep of
    ssInstall:
      Log('Starting file installation...');
    ssPostInstall:
      Log('Post-installation steps...');
  end;
end;
