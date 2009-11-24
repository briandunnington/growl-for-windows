// gen_growl.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <gdiplus.h>
#include <wa_ipc.h>
#include <ipc_pe.h>
#include <bfc\platform\types.h>
#include <api/service/waServiceFactory.h>
#include <api/memmgr/api_memmgr.h>
#include <../Agave/AlbumArt/api_albumart.h>
#include <../Agave/AlbumArt/svc_albumArtProvider.h>
#include "gntp-send.h"
#include <strsafe.h>
#include "gen_growl.h"
#include "resource.h"

/*
 
Winamp generic plugin template code.
This code should be just the basics needed to get a plugin up and running.
You can then expand the code to build your own plugin.
 
Updated details compiled June 2009 by culix, based on the excellent code examples
and advice of forum members Kaboon, kichik, baafie, burek021, and bananskib.
Thanks for the help everyone!
 
*/


// these are callback functions/events which will be called by Winamp
int  init();
int  init_winamp_api_vars();
void config();
void quit();

WNDPROC lpWndProcOld = 0;
LRESULT CALLBACK WndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);
INT_PTR CALLBACK ConfigureDialogProg(HWND hDialog, UINT message, WPARAM wParam, LPARAM lParam);
 
 
// this structure contains plugin information, version, name...
// GPPHDR_VER is the version of the winampGeneralPurposePlugin (GPP) structure
winampGeneralPurposePlugin plugin = {
  GPPHDR_VER,  // version of the plugin, defined in "gen_myplugin.h"
  PLUGIN_NAME, // name/title of the plugin, defined in "gen_myplugin.h"
  init,        // function name which will be executed on init event
  config,      // function name which will be executed on config event
  quit,        // function name which will be executed on quit event
  0,           // handle to Winamp main window, loaded by winamp when this dll is loaded
  0            // hinstance to this dll, loaded by winamp when this dll is loaded
};

ULONG_PTR gdiplusToken;
CLSID bitmap_guid;

char *INI_FILE;
const int buffer_length = 256;

char CurrentPath[_MAX_PATH];

static api_memmgr* memory_manager;
static api_service* service_manager;
static api_albumart* album_art_manager;

// This entire function copy-pasted from MSDN. Original page can be found at
// http://msdn.microsoft.com/en-us/library/ms533843%28VS.85%29.aspx.
// Thank you, Microsoft!
int GetEncoderClsid(const WCHAR* format, CLSID* pClsid)
{
   UINT  num = 0;          // number of image encoders
   UINT  size = 0;         // size of the image encoder array in bytes

   Gdiplus::ImageCodecInfo* pImageCodecInfo = NULL;

   Gdiplus::GetImageEncodersSize(&num, &size);
   if(size == 0)
      return -1;  // Failure

   pImageCodecInfo = (Gdiplus::ImageCodecInfo*)(malloc(size));
   if(pImageCodecInfo == NULL)
      return -1;  // Failure

   Gdiplus::GetImageEncoders(num, size, pImageCodecInfo);

   for(UINT j = 0; j < num; ++j)
   {
      if( wcscmp(pImageCodecInfo[j].MimeType, format) == 0 )
      {
         *pClsid = pImageCodecInfo[j].Clsid;
         free(pImageCodecInfo);
         return j;  // Success
      }    
   }

   free(pImageCodecInfo);
   return -1;  // Failure
}
 
// event functions follow
 
int init() {
    init_winamp_api_vars();

    if (IsWindowUnicode(plugin.hwndParent))
		lpWndProcOld = (WNDPROC)SetWindowLongW(plugin.hwndParent,GWL_WNDPROC,(LONG)WndProc);
	else
		lpWndProcOld = (WNDPROC)SetWindowLongA(plugin.hwndParent,GWL_WNDPROC,(LONG)WndProc);

    INI_FILE = (char*)SendMessage(plugin.hwndParent,WM_WA_IPC,0,IPC_GETINIFILE);

    // the following is required to initialize GDI+
    Gdiplus::GdiplusStartupInput gdiplusStartupInput;
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);

    GetEncoderClsid(L"image/bmp", &bitmap_guid);

	_getcwd(CurrentPath, _MAX_PATH);
	strcat (CurrentPath, "\\..\\Plugins\\winamp.png");

    return 0;
}

int init_winamp_api_vars()
{
    // The following code was taken from the gen_classicart example Winamp plugin. See COPYRIGHT.txt
    service_manager = (api_service*)SendMessage(plugin.hwndParent, WM_WA_IPC, 0, IPC_GET_API_SERVICE);
	if(service_manager == (api_service*)1 || service_manager == NULL) return 1;

	//INI_FILE = (char*)SendMessage(plugin.hwndParent,WM_WA_IPC,0,IPC_GETINIFILE);

	waServiceFactory *sf = service_manager->service_getServiceByGuid(memMgrApiServiceGuid);
	memory_manager = reinterpret_cast<api_memmgr *>(sf->getInterface());

    sf = service_manager->service_getServiceByGuid(albumArtGUID);
    album_art_manager = reinterpret_cast<api_albumart *>(sf->getInterface());

    return 0;
}

void config() {
    DialogBox(plugin.hDllInstance, MAKEINTRESOURCE(IDD_CONFIGURE),
              plugin.hwndParent, ConfigureDialogProg);

}

INT_PTR CALLBACK ConfigureDialogProg(HWND hDialog, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch(message)
    {
    case WM_CLOSE:
        ShowWindow(hDialog, SW_HIDE);
        break;
    case WM_COMMAND:
        switch(LOWORD(wParam))
        {
        case ID_CLOSE:
            ShowWindow(hDialog, SW_HIDE);
            break;
        }
        break;
    }
    return 0;
}
 
void quit() {
    Gdiplus::GdiplusShutdown(gdiplusToken);
}

char* convertToChar(wchar_t* utf16)
{
	int utf8_length;

	utf8_length = WideCharToMultiByte(
	  CP_UTF8,           // Convert to UTF-8
	  0,                 // No special character conversions required 
						 // (UTF-16 and UTF-8 support the same characters)
	  utf16,             // UTF-16 string to convert
	  -1,                // utf16 is NULL terminated (if not, use length)
	  NULL,              // Determining correct output buffer size
	  0,                 // Determining correct output buffer size
	  NULL,              // Must be NULL for CP_UTF8
	  NULL);             // Must be NULL for CP_UTF8

	if (utf8_length == 0) {
	  // Error - call GetLastError for details
	}

	char* utf8 = new char[utf8_length];

	utf8_length = WideCharToMultiByte(
	  CP_UTF8,           // Convert to UTF-8
	  0,                 // No special character conversions required 
						 // (UTF-16 and UTF-8 support the same characters)
	  utf16,             // UTF-16 string to convert
	  -1,                // utf16 is NULL terminated (if not, use length)
	  utf8,              // UTF-8 output buffer
	  utf8_length,       // UTF-8 output buffer size
	  NULL,              // Must be NULL for CP_UTF8
	  NULL);             // Must be NULL for CP_UTF8

	if (utf8_length == 0) {
	  // Error - call GetLastError for details
	}

	return utf8;
}

void growl(char* type, wchar_t* icon, wchar_t* title, wchar_t* notice)
{
	gntp_register(CurrentPath, NULL);
	gntp_notify(type, convertToChar(icon), convertToChar(title), convertToChar(notice), NULL);
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    static ARGB32 *album_art;
    static int album_art_width, album_art_height;

    if (lParam == IPC_PLAYING_FILEW) {
        wchar_t *filename = (wchar_t*)SendMessage(plugin.hwndParent,WM_WA_IPC,0,IPC_GET_PLAYING_FILENAME);

        extendedFileInfoStructW song_data;
        
        
        wchar_t title[buffer_length] = L"";   bool success_title = false;
        wchar_t artist[buffer_length] = L"";  bool success_artist = false;
        wchar_t album[buffer_length] = L"";   bool success_album = false;

        wchar_t buffer[buffer_length]; 
        song_data.filename = filename;
        song_data.metadata = L"title";
        song_data.ret = buffer;
        song_data.retlen = buffer_length;

        // ask for the infoz
        // returns 1 if the decoder supports a getExtendedFileInfo method
        static const int WINAMP_SUCCESS = 1;
        if (SendMessage(plugin.hwndParent,WM_WA_IPC,(WPARAM)&song_data,IPC_GET_EXTENDED_FILE_INFOW) == WINAMP_SUCCESS) {
            success_title = true;
            wcscpy_s(title, buffer);
        }   

        song_data.metadata = L"artist";
        // ask for the infoz
        if (SendMessage(plugin.hwndParent,WM_WA_IPC,(WPARAM)&song_data,IPC_GET_EXTENDED_FILE_INFOW) == WINAMP_SUCCESS) {
            success_artist = true;
            wcscpy_s(artist, buffer);
        }

        song_data.metadata = L"album";
        // ask for the infoz
        if (SendMessage(plugin.hwndParent,WM_WA_IPC,(WPARAM)&song_data,IPC_GET_EXTENDED_FILE_INFOW) == WINAMP_SUCCESS) {
            success_album = true;
            wcscpy_s(album, buffer);
        }

        wchar_t *default_album_art_path = L"";
        wchar_t saved_album_art_folder[MAX_PATH];
		wchar_t saved_album_art_path[MAX_PATH];
		GetTempPath(MAX_PATH, saved_album_art_folder);
		GetTempFileName(saved_album_art_folder, L"amp", 0, saved_album_art_path);
		//wcscat_s(saved_album_art_path, L"\\growl-album-art.bmp");
        wchar_t *album_art_path = default_album_art_path;

        // Get the album art
        if (album_art_manager->GetAlbumArt(filename, L"cover", &album_art_width,
                                       &album_art_height, &album_art) == ALBUMART_SUCCESS)
        {
            int bpp = 32; // bytes per pixel; could be 24?
            int stride = ((album_art_width * bpp + 31) & ~31) >> 3;
            Gdiplus::Bitmap album_art_bitmap(album_art_width, album_art_height, stride, PixelFormat32bppARGB, (BYTE*)album_art);
            
            //album_art_bitmap.from
            Gdiplus::Status result = album_art_bitmap.Save(saved_album_art_path, &bitmap_guid);
            if (result == Gdiplus::Status::Ok)
                album_art_path = saved_album_art_path;
            
            memory_manager->sysFree(album_art);
        }

		growl(notifications[0], album_art_path, title, artist);
    }

	int ret = CallWindowProc(lpWndProcOld,hwnd,message,wParam,lParam);

	return ret;
}

 
// This is an export function called by winamp which returns this plugin info.
// We wrap the code in 'extern "C"' to ensure the export isn't mangled if used in a CPP file.
extern "C" __declspec(dllexport) winampGeneralPurposePlugin * winampGetGeneralPurposePlugin() {
  return &plugin;
}



