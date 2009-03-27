using System;
using iTunesLib;

namespace ITunesPluginApp
{
    public class ITunesHandler : iTunesLib.iTunesAppClass
    {
        public delegate void PluginLoad(EventArgs e);
        public event PluginLoad Load;

        public ITunesHandler()
        {
            this.OnQuittingEvent += new _IiTunesEvents_OnQuittingEventEventHandler(ITunesHandler_OnQuittingEvent);
            this.OnAboutToPromptUserToQuitEvent += new _IiTunesEvents_OnAboutToPromptUserToQuitEventEventHandler(ITunesHandler_OnAboutToPromptUserToQuitEvent);
        }

        void ITunesHandler_OnQuittingEvent()
        {
            System.Windows.Forms.Application.Exit();
        }

        void ITunesHandler_OnAboutToPromptUserToQuitEvent()
        {
            System.Windows.Forms.Application.Exit();
        }
    }
}
