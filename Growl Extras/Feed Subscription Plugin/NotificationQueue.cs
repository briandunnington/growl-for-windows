using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Growl.Connector;

namespace GrowlExtras.Subscriptions.FeedMonitor
{
    static class NotificationQueue
    {
        private const int INTERVAL = 3; // seconds

        private static GrowlConnector growl;
        private static Queue<QueuedNotification> queue;
        private static Timer timer;

        static NotificationQueue()
        {
            growl = new GrowlConnector();
            growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;

            queue = new Queue<QueuedNotification>();

            timer = new Timer(INTERVAL * 1000);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (queue.Count > 0)
            {
                QueuedNotification qn = queue.Dequeue();
                growl.Notify(qn.Notification, qn.Context);
            }

            // if the queue has been emptied, stop the timer (it will get restarted when another item is enqueued)
            if (queue.Count == 0) timer.Stop();
        }

        public static void Enqueue(Notification notification, CallbackContext context)
        {
            QueuedNotification qn = new QueuedNotification(notification, context);
            queue.Enqueue(qn);
            timer.Start();
        }

        private class QueuedNotification
        {
            public QueuedNotification(Notification notification, CallbackContext context)
            {
                this.Notification = notification;
                this.Context = context;
            }

            public Notification Notification;
            public CallbackContext Context;
        }
    }
}
