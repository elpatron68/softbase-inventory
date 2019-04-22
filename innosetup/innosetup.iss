; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "SoftBase Inventory"
#define MyAppVersion "1.0"
#define MyAppPublisher "Markus Busche"
#define MyAppURL "https://github.com/elpatron68"
#define MyAppExeName "SoftBase.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{98ACCB3C-DAFE-4D9D-BE69-39EEE42CFC55}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=..\SoftBase\bin\Release\LICENSE.txt
OutputDir=.\out
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\SoftBase\bin\Release\*.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SoftBase\bin\Release\x64\*"; DestDir: "{app}\x64\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\SoftBase\bin\Release\x86\*"; DestDir: "{app}\x86\"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\SoftBase Inventory"; Filename: "{app}\SoftBase.exe"; IconFilename: "..\SoftBase\img\shipment_upload_cBW_icon.ico"
Name: "{commondesktop}\SoftBase Inventory"; Filename: "{app}\SoftBase.exe"; IconFilename: "..\SoftBase\img\shipment_upload_cBW_icon.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
