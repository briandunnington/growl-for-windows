PhonyBalloony
------------------------------

PhonyBalloony is a process that runs in the background and intercepts Windows
system balloon messages and re-routes them through Growl for Windows.

Since PhonyBalloony intercepts system-level messages and has no UI, the best
way to use PhonyBalloony is to have it automatically start when you log on to
Windows.

1. Unzip the following files into a folder:
	PhonyBalloony.exe
	Growl.Connector.dll
	Growl.CoreLibrary.dll
	SystemBalloonIntercepter.dll

2. Drag PhonyBalloony.exe to your 'Startup' folder
	Start Button > Programs > Startup

If you want to start PhonyBalloony for your current session manually, just
double-click the PhonyBalloony.exe file. There is no UI or tray icon, but the
PhonyBalloony process will show up in Task Manager.

PhonyBalloony automatically shuts itself down when you log off or shutdown
your computer. If Growl is not running or is closed while PhonyBalloony is
running, it will revert to using the system balloons.

NOTE: PhonyBalloony has not been tested on 64-bit systems and may not work
properly.