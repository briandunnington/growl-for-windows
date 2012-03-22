[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=1
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=
TargetName=D:\_PROJECTS\growl-for-windows\Growl\Windows Deployment\Release\GrowlInstaller.exe
FriendlyName=Growl for Windows
AppLaunched=cmd /c GrowlSetup.bat
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="setup.exe"
FILE1="Growl_v2.0.msi"
FILE2="GrowlSetup.bat"
[SourceFiles]
SourceFiles0=D:\_PROJECTS\growl-for-windows\Growl\Windows Deployment\Release\
SourceFiles1=D:\_PROJECTS\growl-for-windows\Growl\
[SourceFiles0]
%FILE0%=
%FILE1%=
[SourceFiles1]
%FILE2%=
