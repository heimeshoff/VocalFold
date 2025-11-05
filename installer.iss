; VocalFold Installer Script for Inno Setup
; https://jrsoftware.org/isinfo.php

#define MyAppName "VocalFold"
#define MyAppVersion GetStringParam("AppVersion", "1.1.0")
#define MyAppPublisher "VocalFold"
#define MyAppURL "https://github.com/heimeshoff/VocalFold"
#define MyAppExeName "VocalFold.exe"
#define MyAppDescription "Voice to Text Application with AI-powered transcription"
#define SourceDir GetStringParam("SourceDir", "deploy\VocalFold")
#define OutputDir GetStringParam("OutputDir", "deploy")

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{8F9A7B6C-5D4E-3A2B-1C9F-8E7D6C5B4A3F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir={#OutputDir}
OutputBaseFilename=VocalFold-Setup-{#MyAppVersion}
SetupIconFile=logo.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppDescription}
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenuicon"; Description: "Create Start Menu shortcut"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startmenuicon
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
  AppRunning: Boolean;
  RetryCount: Integer;
begin
  Result := True;
  RetryCount := 0;

  // Check if the application is already running
  repeat
    AppRunning := CheckForMutexes('Global\VocalFoldMutex');

    if not AppRunning then
      Break;

    if RetryCount = 0 then
    begin
      // First time detection - ask user to close the app
      if MsgBox('VocalFold is currently running. Please close it before continuing with the installation.' + #13#10 + #13#10 +
                'Click OK after closing VocalFold, or click Cancel to abort the installation.',
                mbError, MB_OKCANCEL) = IDCANCEL then
      begin
        Result := False;
        Exit;
      end;
    end
    else
    begin
      // User clicked OK but app is still running
      if MsgBox('VocalFold is still running. Please close it now.' + #13#10 + #13#10 +
                'Click OK to check again, or click Cancel to abort the installation.',
                mbError, MB_OKCANCEL) = IDCANCEL then
      begin
        Result := False;
        Exit;
      end;
    end;

    Inc(RetryCount);
    Sleep(500); // Wait a bit before checking again

  until RetryCount > 10; // Give up after 10 retries

  if AppRunning then
  begin
    MsgBox('Unable to continue because VocalFold is still running. Please close it manually and restart the installer.',
           mbError, MB_OK);
    Result := False;
  end;
end;

function InitializeUninstall(): Boolean;
var
  ResultCode: Integer;
  AppRunning: Boolean;
  RetryCount: Integer;
begin
  Result := True;
  RetryCount := 0;

  // Check if the application is running during uninstall
  repeat
    AppRunning := CheckForMutexes('Global\VocalFoldMutex');

    if not AppRunning then
      Break;

    if RetryCount = 0 then
    begin
      if MsgBox('VocalFold is currently running. Please close it before continuing with the uninstallation.' + #13#10 + #13#10 +
                'Click OK after closing VocalFold, or click Cancel to abort.',
                mbError, MB_OKCANCEL) = IDCANCEL then
      begin
        Result := False;
        Exit;
      end;
    end
    else
    begin
      if MsgBox('VocalFold is still running. Please close it now.' + #13#10 + #13#10 +
                'Click OK to check again, or click Cancel to abort.',
                mbError, MB_OKCANCEL) = IDCANCEL then
      begin
        Result := False;
        Exit;
      end;
    end;

    Inc(RetryCount);
    Sleep(500);

  until RetryCount > 10;

  if AppRunning then
  begin
    MsgBox('Unable to continue because VocalFold is still running. Please close it manually and restart the uninstaller.',
           mbError, MB_OK);
    Result := False;
  end;
end;
