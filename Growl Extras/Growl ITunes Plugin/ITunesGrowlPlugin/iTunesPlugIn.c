#include "iTunesVisualAPI.h"
#include "process.h"

#define	MAIN iTunesPluginMain
#define IMPEXP	__declspec(dllexport)

#define kTVisualPluginName              "\023Growl Notifications"
#define	kTVisualPluginCreator			'hook'
#define	kTVisualPluginMajorVersion		1
#define	kTVisualPluginMinorVersion		2
#define	kTVisualPluginReleaseStage		developStage
#define	kTVisualPluginNonFinalRelease	0

struct VisualPluginData {
	void *				appCookie;
	ITAppProcPtr			appProc;
	HWND				destPort;
	Rect				destRect;
	OptionBits			destOptions;
	UInt32				destBitDepth;
	RenderVisualData		renderData;
	UInt32				renderTimeStampID;
	ITTrackInfo			trackInfo;
	ITStreamInfo			streamInfo;
	Boolean				playing;
	Boolean				padding[3];
	UInt8				minLevel[kVisualMaxDataChannels];		// 0-128
	UInt8				maxLevel[kVisualMaxDataChannels];		// 0-128
	UInt8				min, max;
};
typedef struct VisualPluginData VisualPluginData;


//########################################
//	static ( local ) functions
//########################################

static void MyMemClear( LogicalAddress dest, SInt32 length )
{
	register unsigned char	*ptr;

	ptr = ( unsigned char* ) dest;
	
	while ( length-- > 0 )
		*ptr++ = 0;
}

//########################################
//	RenderVisualPort
//########################################

static void RenderVisualPort(VisualPluginData *visualPluginData, GRAPHICS_DEVICE destPort,const Rect *destRect,Boolean onlyUpdate)
{

	(void) visualPluginData;
	(void) onlyUpdate;
	
	if (destPort == nil)
		return;

	#if TARGET_OS_MAC	
	{	
		GrafPtr		oldPort;
		GDHandle	oldDevice;
		Rect		srcRect;
		RGBColor	foreColor;
			
		srcRect		= *destRect;

		GetGWorld(&oldPort, &oldDevice);
		SetGWorld(destPort, nil);
	
		foreColor.blue = ((UInt16)visualPluginData->maxLevel[0] << 9);
		foreColor.red = foreColor.green = foreColor.blue;
			
		RGBForeColor(&foreColor);
		PaintRect(destRect);
		
		SetGWorld(oldPort, oldDevice);
	}
	#else
	{
		RECT	srcRect;
		HBRUSH	hBrush;
		HDC		hdc;
		
		srcRect.left = destRect->left;
		srcRect.top = destRect->top;
		srcRect.right = destRect->right;
		srcRect.bottom = destRect->bottom;
		
		hdc = GetDC(destPort);		
		hBrush = CreateSolidBrush(RGB((UInt16)visualPluginData->maxLevel[1]<<1, (UInt16)visualPluginData->maxLevel[1]<<1, (UInt16)visualPluginData->maxLevel[1]<<1));
		FillRect(hdc, &srcRect, hBrush);
		DeleteObject(hBrush);
		ReleaseDC(destPort, hdc);
	}
	#endif
}


//########################################
//	ChangeVisualPort
//########################################

static OSStatus ChangeVisualPort(VisualPluginData *visualPluginData,GRAPHICS_DEVICE destPort,const Rect *destRect)
{
	OSStatus		status;
	
	status = noErr;
			
	visualPluginData->destPort = destPort;
	if (destRect != nil)
		visualPluginData->destRect = *destRect;

	return status;
}

//########################################
//	ResetRenderData
//########################################
static void ResetRenderData(VisualPluginData *visualPluginData)
{
	MyMemClear(&visualPluginData->renderData,sizeof(visualPluginData->renderData));

	visualPluginData->minLevel[0] = 
		visualPluginData->minLevel[1] =
		visualPluginData->maxLevel[0] =
		visualPluginData->maxLevel[1] = 0;
}


//########################################
//	VisualPluginHandler
//########################################
static OSStatus VisualPluginHandler(OSType message,VisualPluginMessageInfo *messageInfo,void *refCon)
{
	OSStatus			status;
	VisualPluginData *	visualPluginData;
	char strPathName[_MAX_PATH];

	visualPluginData = (VisualPluginData*) refCon;
	status = noErr;

	switch (message)
	{
		/*
			Sent when the visual plugin is registered.  The plugin should do minimal
			memory allocations here.  The resource fork of the plugin is still available.
		*/		
		case kVisualPluginInitMessage:
		{
			visualPluginData = (VisualPluginData*) calloc(1, sizeof(VisualPluginData));
			if (visualPluginData == nil)
			{
				status = memFullErr;
				break;
			}

			visualPluginData->appCookie	= messageInfo->u.initMessage.appCookie;
			visualPluginData->appProc	= messageInfo->u.initMessage.appProc;

			messageInfo->u.initMessage.refCon	= (void*) visualPluginData;

			GetPluginHelperAppPath(strPathName);
			_spawnl(P_NOWAITO, strPathName, "-load", NULL);
			//_spawnl(P_NOWAITO, "C:\\Program Files\\Internet Explorer\\iexplore.exe", "C:\\Program Files\\Internet Explorer\\iexplore.exe", strPathName, NULL);

			break;
		}

		case kVisualPluginConfigureMessage:
			GetPluginHelperAppPath(strPathName);
			_spawnl(P_NOWAITO, strPathName, "-configure", "-configure", NULL);
			break;
			
		/*
			Sent when the visual plugin is unloaded
		*/		
		case kVisualPluginCleanupMessage:
			if (visualPluginData != nil)
				free(visualPluginData);
			break;
			
		/*
			Sent when the visual plugin is enabled.  iTunes currently enables all
			loaded visual plugins.  The plugin should not do anything here.
		*/
		case kVisualPluginEnableMessage:
		case kVisualPluginDisableMessage:
			break;

		/*
			Sent if the plugin requests idle messages.  Do this by setting the kVisualWantsIdleMessages
			option in the RegisterVisualMessage.options field.
		*/
		case kVisualPluginIdleMessage:
			if (false == visualPluginData->playing)
				RenderVisualPort(visualPluginData,visualPluginData->destPort,&visualPluginData->destRect,false);
			break;
					
		/*
			Sent when iTunes is going to show the visual plugin in a port.  At
			this point,the plugin should allocate any large buffers it needs.
		*/
		case kVisualPluginShowWindowMessage:
			visualPluginData->destOptions = messageInfo->u.showWindowMessage.options;

			status = ChangeVisualPort(	visualPluginData,
										#if TARGET_OS_WIN32
											messageInfo->u.setWindowMessage.window,
										#endif
										#if TARGET_OS_MAC
											messageInfo->u.setWindowMessage.port,
										#endif
										&messageInfo->u.showWindowMessage.drawRect);
			if (status == noErr)
				RenderVisualPort(visualPluginData,visualPluginData->destPort,&visualPluginData->destRect,true);

			break;
			
		/*
			Sent when iTunes is no longer displayed.
		*/
		case kVisualPluginHideWindowMessage:
			(void) ChangeVisualPort(visualPluginData,nil,nil);

			MyMemClear(&visualPluginData->trackInfo,sizeof(visualPluginData->trackInfo));
			MyMemClear(&visualPluginData->streamInfo,sizeof(visualPluginData->streamInfo));
			break;
		
		/*
			Sent when iTunes needs to change the port or rectangle of the currently
			displayed visual.
		*/
		case kVisualPluginSetWindowMessage:
			visualPluginData->destOptions = messageInfo->u.setWindowMessage.options;

			status = ChangeVisualPort(	visualPluginData,
										#if TARGET_OS_WIN32
											messageInfo->u.showWindowMessage.window,
										#endif
										#if TARGET_OS_MAC
											messageInfo->u.showWindowMessage.port,
										#endif
										&messageInfo->u.setWindowMessage.drawRect);

			if (status == noErr)
				RenderVisualPort(visualPluginData,visualPluginData->destPort,&visualPluginData->destRect,true);
			break;
		
		/*
			Sent for the visual plugin to render a frame.
		*/
		case kVisualPluginRenderMessage:
			//visualPluginData->renderTimeStampID	= messageInfo->u.renderMessage.timeStampID;

			//ProcessRenderData(visualPluginData,messageInfo->u.renderMessage.renderData);
				
			//RenderVisualPort(visualPluginData,visualPluginData->destPort,&visualPluginData->destRect,false);
			break;
#if 0			
		/*
			Sent for the visual plugin to render directly into a port.  Not necessary for normal
			visual plugins.
		*/
		case kVisualPluginRenderToPortMessage:
			status = unimpErr;
			break;
#endif 0
		/*
			Sent in response to an update event.  The visual plugin should update
			into its remembered port.  This will only be sent if the plugin has been
			previously given a ShowWindow message.
		*/	
		case kVisualPluginUpdateMessage:
			RenderVisualPort(visualPluginData,visualPluginData->destPort,&visualPluginData->destRect,true);
			break;
		
		/*
			Sent when the player starts.
		*/
		case kVisualPluginPlayMessage:
			if (messageInfo->u.playMessage.trackInfo != nil)
				visualPluginData->trackInfo = *messageInfo->u.playMessage.trackInfoUnicode;
			else
				MyMemClear(&visualPluginData->trackInfo,sizeof(visualPluginData->trackInfo));

			if (messageInfo->u.playMessage.streamInfo != nil)
				visualPluginData->streamInfo = *messageInfo->u.playMessage.streamInfoUnicode;
			else
				MyMemClear(&visualPluginData->streamInfo,sizeof(visualPluginData->streamInfo));
		
			visualPluginData->playing = true;
			break;

		/*
			Sent when the player changes the current track information.  This
			is used when the information about a track changes,or when the CD
			moves onto the next track.  The visual plugin should update any displayed
			information about the currently playing song.
		*/
		case kVisualPluginChangeTrackMessage:
			if (messageInfo->u.changeTrackMessage.trackInfo != nil)
				visualPluginData->trackInfo = *messageInfo->u.changeTrackMessage.trackInfoUnicode;
			else
				MyMemClear(&visualPluginData->trackInfo,sizeof(visualPluginData->trackInfo));

			if (messageInfo->u.changeTrackMessage.streamInfo != nil)
				visualPluginData->streamInfo = *messageInfo->u.changeTrackMessage.streamInfoUnicode;
			else
				MyMemClear(&visualPluginData->streamInfo,sizeof(visualPluginData->streamInfo));
			break;

		/*
			Sent when the player stops.
		*/
		case kVisualPluginStopMessage:
			visualPluginData->playing = false;
			
			ResetRenderData(visualPluginData);

			RenderVisualPort(visualPluginData,visualPluginData->destPort,&visualPluginData->destRect,true);
			break;
		
		/*
			Sent when the player changes position.
		*/
		case kVisualPluginSetPositionMessage:
			break;

		/*
			Sent when the player pauses.  iTunes does not currently use pause or unpause.
			A pause in iTunes is handled by stopping and remembering the position.
		*/
		case kVisualPluginPauseMessage:
			visualPluginData->playing = false;

			ResetRenderData(visualPluginData);

			RenderVisualPort(visualPluginData,visualPluginData->destPort,&visualPluginData->destRect,true);
			break;
			
		/*
			Sent when the player unpauses.  iTunes does not currently use pause or unpause.
			A pause in iTunes is handled by stopping and remembering the position.
		*/
		case kVisualPluginUnpauseMessage:
			visualPluginData->playing = true;
			break;

		default:
			status = unimpErr;
			break;
	}
	return status;	
}

//########################################
//	RegisterVisualPlugin
//########################################
static OSStatus RegisterVisualPlugin(PluginMessageInfo *messageInfo)
{
	OSStatus			status;
	PlayerMessageInfo	playerMessageInfo;
	Str255				pluginName = kTVisualPluginName;
		
	MyMemClear(&playerMessageInfo.u.registerVisualPluginMessage,sizeof(playerMessageInfo.u.registerVisualPluginMessage));
	
	memcpy(&playerMessageInfo.u.registerVisualPluginMessage.name[0], &pluginName[0], pluginName[0] + 1);

	SetNumVersion(&playerMessageInfo.u.registerVisualPluginMessage.pluginVersion,kTVisualPluginMajorVersion,kTVisualPluginMinorVersion,kTVisualPluginReleaseStage,kTVisualPluginNonFinalRelease);

	playerMessageInfo.u.registerVisualPluginMessage.options					=	kVisualWantsIdleMessages | kVisualWantsConfigure;

	playerMessageInfo.u.registerVisualPluginMessage.handler					= (VisualPluginProcPtr)VisualPluginHandler;
	playerMessageInfo.u.registerVisualPluginMessage.registerRefCon			= 0;
	playerMessageInfo.u.registerVisualPluginMessage.creator					= kTVisualPluginCreator;
	
	playerMessageInfo.u.registerVisualPluginMessage.timeBetweenDataInMS		= 0xFFFFFFFF; // 16 milliseconds = 1 Tick,0xFFFFFFFF = Often as possible.
	playerMessageInfo.u.registerVisualPluginMessage.numWaveformChannels		= 2;
	playerMessageInfo.u.registerVisualPluginMessage.numSpectrumChannels		= 2;
	
	playerMessageInfo.u.registerVisualPluginMessage.minWidth				= 64;
	playerMessageInfo.u.registerVisualPluginMessage.minHeight				= 64;
	playerMessageInfo.u.registerVisualPluginMessage.maxWidth				= 32767;
	playerMessageInfo.u.registerVisualPluginMessage.maxHeight				= 32767;
	playerMessageInfo.u.registerVisualPluginMessage.minFullScreenBitDepth	= 0;
	playerMessageInfo.u.registerVisualPluginMessage.maxFullScreenBitDepth	= 0;
	playerMessageInfo.u.registerVisualPluginMessage.windowAlignmentInBytes	= 0;
	
	status = PlayerRegisterVisualPlugin(messageInfo->u.initMessage.appCookie,messageInfo->u.initMessage.appProc,&playerMessageInfo);
		
	return status;
	
}

//########################################
//	GetPluginHelperAppPath
//########################################
static int GetPluginHelperAppPath(char *pathName)
{
	char *c;

	GetModuleFileNameA(NULL, pathName, _MAX_PATH);
	c = strrchr(pathName, '\\');
	*c = NULL;
	strcat(pathName, "\\Plug-Ins\\GrowlPlugin\\GrowlExtras.ITunesPlugin.exe");
	return 0;
}

//########################################
//	main entrypoint
//########################################

IMPEXP OSStatus MAIN(OSType message,PluginMessageInfo *messageInfo,void *refCon)
{
	OSStatus		status;
	
	(void) refCon;
	
	switch (message)
	{
		case kPluginInitMessage:
			status = RegisterVisualPlugin(messageInfo);
			break;
			
		case kPluginCleanupMessage:
			status = noErr;
			break;
			
		default:
			status = unimpErr;
			break;
	}
	
	return status;
}
