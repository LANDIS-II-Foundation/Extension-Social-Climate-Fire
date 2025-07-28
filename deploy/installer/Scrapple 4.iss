; LANDIS-II Extension infomation
#define CoreRelease "LANDIS-II-V8"
#define ExtensionName "Social Climate Fire"
#define AppVersion "4.0.1"
#define AppPublisher "LANDIS-II Foundation"
#define AppURL "http://www.landis-ii.org/"

; Build directory
#define BuildDir "..\..\src\bin\Debug\netstandard2.0"

; LANDIS-II installation directories
#define ExtDir "C:\Program Files\LANDIS-II-v8\extensions"
#define AppDir "C:\Program Files\LANDIS-II-v8"
#define LandisPlugInDir "C:\Program Files\LANDIS-II-v8\plug-ins-installer-files"
#define ExtensionsCmd AppDir + "\commands\landis-ii-extensions.cmd"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{00B1C477-6DE8-41C7-85E4-4FF75010A348}
AppName={#CoreRelease} {#ExtensionName}
AppVersion={#AppVersion}
; Name in "Programs and Features"
AppVerName={#CoreRelease} {#ExtensionName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={commonpf}\{#ExtensionName}
DisableDirPage=yes
DefaultGroupName={#ExtensionName}
DisableProgramGroupPage=yes
LicenseFile=LANDIS-II_Binary_license.rtf
OutputDir={#SourcePath}
OutputBaseFilename={#CoreRelease} {#ExtensionName} {#AppVersion}-setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"


[Files]
; This .dll IS the extension (ie, the extension's assembly)
; NB: Do not put an additional version number in the file name of this .dll
; (The name of this .dll is defined in the extension's \src\*.csproj file)
Source: {#BuildDir}\Landis.Extension.SocialClimateFire-v4.dll; DestDir: {#ExtDir}; Flags: ignoreversion
Source: {#BuildDir}\Landis.Extension.SocialClimateFire-v4.pdb; DestDir: {#ExtDir}; Flags: ignoreversion

; Requisite auxiliary libraries.  These libraries may be used by other extensions and thus are never uninstalled.
Source: {#BuildDir}\Landis.Library.Parameters-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall 
Source: {#BuildDir}\Landis.Library.Climate-v5.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall 
Source: {#BuildDir}\Ether.WeightedSelector.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
; Source: {#BuildDir}\Ether.WeightedSelector.pdb; DestDir: {#ExtDir}; Flags: uninsneveruninstall
; Source: {#BuildDir}\Landis.Library.Metadata-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall 
; Source: {#BuildDir}\Landis.Library.UniversalCohorts-v1.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall 


; LANDIS-II identifies the extension with the info in this .txt file
; NB. New releases must modify the name of this file and the info in it
#define InfoTxt "SCRAPPLE 4.txt"
Source: {#InfoTxt}; DestDir: {#LandisPlugInDir}
; NOTE: Don't use "Flags: ignoreversion" on any shared system files


[Run]
Filename: {#ExtensionsCmd}; Parameters: "remove ""SCRAPPLE"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#ExtensionsCmd}; Parameters: "add ""{#InfoTxt}"" "; WorkingDir: {#LandisPlugInDir} 


[UninstallRun]
; Remove "Age-Only Succession" from "extensions.xml" file.
Filename: {#ExtensionsCmd}; Parameters: "remove ""SCRAPPLE"" "; WorkingDir: {#LandisPlugInDir}


