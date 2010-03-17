#include "stdafx.h"
#include <windows.h>
#include "SystemBalloonIntercepter.h"

// Store the application instance of this module to pass to hook initialization. This is set in DLLMain().
HINSTANCE g_appInstance = NULL;

UINT msg_unsubclass = 0;
UINT msg_replaced = 0;
HHOOK hookCallWndProc = NULL;
LRESULT CALLBACK CallWndProcHookCallback(UINT code, WPARAM wparam, LPARAM lparam);
LRESULT CALLBACK SubclassWndProc(HWND hwnd, UINT code, WPARAM wparam, LPARAM lparam);
LRESULT oldWndProc;
UINT WM_UNSUBCLASS = 0;
BOOL isSubclassed = false;

bool InitializeCallWndProcHook(DWORD threadID, HWND destination)
{
	if (g_appInstance == NULL)
	{
		return false;
	}

	msg_unsubclass = RegisterWindowMessage("GFW_HOOK_UNSUBCLASS");
	msg_replaced = RegisterWindowMessage("GFW_HOOK_CALLWNDPROC_REPLACED");
	HWND dstWnd = (HWND)GetProp(GetDesktopWindow(), "GFW_HOOK_HWND_CALLWNDPROC");
	if (dstWnd != NULL)
	{
		PostMessage(dstWnd, msg_replaced, 0, 0);
	}
	dstWnd = destination;
	SetProp(GetDesktopWindow(), "GFW_HOOK_HWND_CALLWNDPROC", dstWnd);

	hookCallWndProc = SetWindowsHookEx(WH_CALLWNDPROC, (HOOKPROC)CallWndProcHookCallback, g_appInstance, threadID);
	return hookCallWndProc != NULL;
}

void UninitializeCallWndProcHook()
{
	// stop subclassing
	HWND trayHwnd = FindWindow("Shell_TrayWnd", NULL);
	SendMessageW(trayHwnd, msg_unsubclass, 0, 0);
	RemoveProp(GetDesktopWindow(), "GFW_HOOK_HWND_CALLWNDPROC");

	// unhook
	if (hookCallWndProc != NULL)
	{
		UnhookWindowsHookEx(hookCallWndProc);
		hookCallWndProc = NULL;
	}
}


LRESULT CALLBACK CallWndProcHookCallback(UINT code, WPARAM wparam, LPARAM lparam)
{
	if(WM_UNSUBCLASS == 0) WM_UNSUBCLASS = RegisterWindowMessage("GFW_HOOK_UNSUBCLASS");

	if (code >= 0)
	{
		PCWPSTRUCT pCwpStruct = (PCWPSTRUCT)lparam;
		HWND hwnd = pCwpStruct->hwnd;
		HWND trayHwnd = FindWindow("Shell_TrayWnd", NULL);

		if((LONG_PTR) hwnd == (LONG_PTR) trayHwnd)
		{
			if(!isSubclassed)
			{
				isSubclassed = true;
				oldWndProc = (LRESULT) SetWindowLongPtr(trayHwnd, GWLP_WNDPROC, (LONG_PTR) SubclassWndProc);
			}
		}
	}

	return CallNextHookEx(hookCallWndProc, code, wparam, lparam);
}

LRESULT CALLBACK SubclassWndProc(HWND hwnd, UINT code, WPARAM wparam, LPARAM lparam)
{
	LRESULT r = 0;
	BOOL handled = false;

	if(code == WM_COPYDATA)
	{
		PCOPYDATASTRUCT cpData = (PCOPYDATASTRUCT) lparam;

		if (cpData->dwData == 1)
		{
			DWORD trayCommand = *(DWORD *) (((BYTE *)cpData->lpData) + 4);
			PNOTIFYICONDATA iconData = (PNOTIFYICONDATA) (((BYTE *)cpData->lpData) + 8);

			BOOL isBalloon = (iconData->uFlags & NIF_INFO);
			if(isBalloon)
			{
				HWND dstWnd = (HWND)GetProp(GetDesktopWindow(), "GFW_HOOK_HWND_CALLWNDPROC");
				r = SendMessageW(dstWnd, WM_COPYDATA, 97, lparam);
				if(r == 0) handled = true;
			}
		}
	}
	else if(code == WM_UNSUBCLASS)
	{
		SetWindowLongPtr(hwnd, GWLP_WNDPROC, (LONG) oldWndProc);
		isSubclassed = false;
		handled = true;
	}

	if(!handled) r = CallWindowProc((WNDPROC)oldWndProc, hwnd, code, wparam, lparam);
	return r;
}