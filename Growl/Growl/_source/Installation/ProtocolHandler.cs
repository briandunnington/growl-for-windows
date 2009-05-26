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

        public int Process(string uri)
        {
            // general format: growl://action*data1&data2&data3...etc
            // example: growl://display*http://www.somesite.org/display.xml

            int result = 0;

            Regex regex = new Regex(@"growl://(?<Action>[^\*]*)\*(?<Data>[\s\S]*)");
            Match match = regex.Match(uri);
            if (match.Success)
            {
                string action = match.Groups["Action"].Value.ToLower();
                string data = match.Groups["Data"].Value;
                switch (action)
                {
                    case "display":
                        InstallDisplay form = new InstallDisplay();
                        bool newDisplayLoaded = form.LaunchInstaller(data, this.appIsAlreadyRunning);
                        if (newDisplayLoaded) result = ApplicationMain.SIGNAL_RELOAD_DISPLAYS;
                        break;
                    case "extension":
                        // this isnt a real use case yet
                        break;
                }
            }
            return result;
        }
    }
}
