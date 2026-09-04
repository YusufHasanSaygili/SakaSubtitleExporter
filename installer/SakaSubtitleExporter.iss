#define MyAppName "Saka Subtitle Exporter"
#define MyAppVersion "2.1.4"
#define MyAppPublisher "YusufHasanSaygili"
#define MyAppExeName "SakaSubtitleExporter.exe"

#ifndef SourceExe
  #define SourceExe "..\src\SakaSubtitleExporter\bin\Release\net48\SakaSubtitleExporter.exe"
#endif

#ifndef OutputDir
  #define OutputDir "..\artifacts\setup"
#endif

#ifndef IconFile
  #define IconFile "..\assets\saka.ico"
#endif

[Setup]
AppId={{8B899402-F413-472E-BCCA-6BEC104A7F3E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName=A:\Anime\Saka Subtitle Exporter
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=SakaSubtitleExporterSetup
SetupIconFile={#IconFile}
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
ChangesAssociations=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardStyle=modern
ArchitecturesAllowed=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#SourceExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceExe}.config"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#IconFile}"; DestDir: "{app}"; DestName: "saka.ico"; Flags: ignoreversion

[Registry]
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mkv\shell\SakaExportSubtitles"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Export Subtitles With Saka"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mkv\shell\SakaExportSubtitles"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\saka.ico,0"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mkv\shell\SakaExportSubtitles"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.mkv\shell\SakaExportSubtitles\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" --ui ""%1"""

[Icons]
Name: "{userprograms}\Saka Subtitle Exporter"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
