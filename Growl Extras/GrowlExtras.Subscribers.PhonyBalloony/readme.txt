GrowlExtras.Subscribers.PhonyBalloony is what gets loaded by Growl. It is equivalent to the launcher app. Unlike most subscribers, it
does not actually do any notifying.

GrowlTray is the actual process that gets launched. It has specific 32- and 64-bit versions. It hooks the system tray notifications
and does the actual Growl notifying.

You must build both a Release32 and Release64 version and make sure all of the .dlls end up in the plugin folder.