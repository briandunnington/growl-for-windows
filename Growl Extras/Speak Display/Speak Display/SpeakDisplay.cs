using System;
using System.Collections.Generic;
using System.Text;
using System.Speech.Synthesis;
using Growl.DisplayStyle;

namespace Speak
{
    public class SpeakDisplay : Display
    {
        SpeechSynthesizer ss = new SpeechSynthesizer();

        public override string Name
        {
            get { return "Speak";  }
        }

        public override string Description
        {
            get { return "Speaks the notification aloud"; }
        }

        public override string Author
        {
            get { return "Growl"; }
        }

        public override string Version
        {
            get { return "1.0"; }
        }

        public override string Website
        {
            get { return null; }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            StringBuilder sb = new StringBuilder();

            PromptBuilder pb = new PromptBuilder();
            pb.AppendText(notification.Title, PromptEmphasis.Strong);
            pb.AppendBreak();
            pb.AppendText(notification.Description);
            
            ss.Speak(pb);
        }

        public override void CloseAllOpenNotifications()
        {
            // do nothing
        }

        public override void CloseLastNotification()
        {
            // do nothing
        }

        public override event Growl.CoreLibrary.NotificationCallbackEventHandler NotificationClicked;

        public override event Growl.CoreLibrary.NotificationCallbackEventHandler NotificationClosed;
    }
}
