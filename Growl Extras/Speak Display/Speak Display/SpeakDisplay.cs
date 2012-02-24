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
            get { return "1.1"; }
        }

        public override string Website
        {
            get { return null; }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            /*
            string xml = @"<p>
                            <s>You have 4 new messages.</s>
                            <s>The first is from Stephanie Williams and arrived at <break/> 3:45pm.
                            </s>
                            <s>
                              The subject is <prosody rate=""-20%"">ski trip</prosody>
                            </s>
                          </p>";
             * */

            PromptBuilder pb = new PromptBuilder();

            // handle title
            if (notification.CustomTextAttributes != null && notification.CustomTextAttributes.ContainsKey("Notification-Title-SSML"))
                pb.AppendSsmlMarkup(notification.CustomTextAttributes["Notification-Title-SSML"]);
            else
                pb.AppendText(notification.Title, PromptEmphasis.Strong);

            pb.AppendBreak();

            // handle text
            if (notification.CustomTextAttributes != null && notification.CustomTextAttributes.ContainsKey("Notification-Text-SSML"))
                pb.AppendSsmlMarkup(notification.CustomTextAttributes["Notification-Text-SSML"]);
            else
                pb.AppendText(notification.Description);

            try
            {
                ss.Speak(pb);
            }
            catch (Exception ex)
            {
                Growl.CoreLibrary.DebugInfo.WriteLine("Unable to speak input: " + ex.Message);

                // fall back to plain text (if the plain text is what failed the first time, it wont work this time either but wont hurt anything)
                pb.ClearContent();
                pb.AppendText(notification.Title, PromptEmphasis.Strong);
                pb.AppendBreak();
                pb.AppendText(notification.Description);
                ss.Speak(pb);
            }
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
