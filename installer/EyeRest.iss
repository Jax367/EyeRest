#ifndef AppVersion
  #define AppVersion "1.5.0"
#endif
#ifndef BuildDirectory
  #define BuildDirectory "..\build\release"
#endif

[Setup]
AppId={{BDB60885-05C2-4B1F-83B6-8178F4D46324}
AppName=目间 EyeRest
AppVersion={#AppVersion}
AppPublisher=Lar
AppPublisherURL=https://github.com/Jax367/EyeRest
AppSupportURL=https://github.com/Jax367/EyeRest/issues
AppUpdatesURL=https://github.com/Jax367/EyeRest/releases
DefaultDirName={localappdata}\Programs\EyeRest
DefaultGroupName=目间 EyeRest
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
OutputDir={#BuildDirectory}
OutputBaseFilename=EyeRest-{#AppVersion}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\EyeRest.exe
CloseApplications=yes
RestartApplications=no
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#BuildDirectory}\EyeRest.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDirectory}\EyeRest.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDirectory}\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\目间 EyeRest"; Filename: "{app}\EyeRest.exe"
Name: "{autodesktop}\目间 EyeRest"; Filename: "{app}\EyeRest.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\EyeRest.exe"; Description: "{cm:LaunchProgram,目间 EyeRest}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release);
  if Result then
    Result := Release >= 528040;
  if not Result then
    MsgBox('EyeRest requires .NET Framework 4.8 or newer. Please install it before continuing.', mbError, MB_OK);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  Command: String;
begin
  if CurUninstallStep = usUninstall then
    if RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'EyeRest', Command) then
      if CompareText(Command, '"' + ExpandConstant('{app}\EyeRest.exe') + '" --startup') = 0 then
        RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'EyeRest');
end;
