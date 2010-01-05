using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Growl.Installation
{
    class ProtocolHandler
    {
        private const string GROWL_PROTOCOL_PREFIX = "growl:";

        private bool appIsAlreadyRunning;

        public ProtocolHandler(bool appIsAlreadyRunning)
        {
            this.appIsAlreadyRunning = appIsAlreadyRunning;
        }

        public ApplicationMain.Signal Process(string uri, ref List<InternalNotification> queuedNotifications, ref int signalValue)
        {
            // general format: growl:action*data1&data2&data3...etc
            // example: growl:display*http://www.somesite.org/display.xml
            // 09.22.2009 - changed protocol scheme from growl:// to just growl: because of a bug in Google Chrome: http://code.google.com/p/chromium/issues/detail?id=160
            //              the old syntax (with //) is still supported as well

            ApplicationMain.Signal result = 0;

            Regex regex = new Regex(@"growl:(//)?(?<Action>[^\*]*)\*(?<Data>[\s\S]*)");
            Match match = regex.Match(uri);
            if (match.Success)
            {
                string action = match.Groups["Action"].Value.ToLower();
                string data = match.Groups["Data"].Value;
                switch (action)
                {
                    case "display":
                        InstallDisplay id = new InstallDisplay();
                        bool newDisplayLoaded = id.LaunchInstaller(data, this.appIsAlreadyRunning, ref queuedNotifications);
                        if (newDisplayLoaded) result = ApplicationMain.Signal.ReloadDisplays;
                        id.Close();
                        id = null;
                        break;
                    case "extension":
                        // this isnt a real use case yet
                        break;
                    case "language":
                        InstallLanguage il = new InstallLanguage();
                        bool languageInstalled = il.LaunchInstaller(data, this.appIsAlreadyRunning, ref queuedNotifications, ref signalValue);
                        if (languageInstalled) result = ApplicationMain.Signal.UpdateLanguage;
                        il.Close();
                        il = null;
                        break;
                }
            }
            return result;
        }
    }
}
