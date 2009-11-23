#ifndef gen_myplugin_h
//---------------------------------------------------------------------------
#define gen_myplugin_h
#include "stdafx.h"
#include <windows.h>
 
 
// plugin version (don't touch this)
#define GPPHDR_VER 0x10
 
// plugin name/title (change this to something you like)
#define PLUGIN_NAME "Growl notifications"
 
 
// main structure with plugin information, version, name...
typedef struct {
  int version;                   // version of the plugin structure
  char *description;             // name/title of the plugin 
  int (*init)();                 // function which will be executed on init event
  void (*config)();              // function which will be executed on config event
  void (*quit)();                // function which will be executed on quit event
  HWND hwndParent;               // hwnd of the Winamp client main window (stored by Winamp when dll is loaded)
  HINSTANCE hDllInstance;        // hinstance of this plugin DLL. (stored by Winamp when dll is loaded) 
} winampGeneralPurposePlugin;

 
#endif