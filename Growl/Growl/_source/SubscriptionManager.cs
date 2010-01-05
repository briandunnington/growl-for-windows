using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public static class SubscriptionManager
    {
        private static List<ISubscriptionHandler> handlers = new List<ISubscriptionHandler>();
        private static Dictionary<Type, ISubscriptionHandler> types = new Dictionary<Type, ISubscriptionHandler>();

        public static void Initialize()
        {
            Register(new GNTPSubscriptionHandler());
            //Register(new RssSubscriptionHandler());
            //Register(new NotifyIOSubscriptionHandler());
        }

        private static void Register(ISubscriptionHandler ish)
        {
            if (ish != null)
            {
                handlers.Add(ish);

                List<Type> list = ish.Register();
                foreach (Type type in list)
                {
                    if (typeof(Subscription).IsAssignableFrom(type))
                    {
                        if (!types.ContainsKey(type))
                        {
                            lock (types)
                            {
                                if (!types.ContainsKey(type))
                                {
                                    types.Add(type, ish);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static ISubscriptionHandler GetHandler(DestinationBase db)
        {
            Type type = db.GetType();
            return GetHandler(type);
        }

        public static ISubscriptionHandler GetHandler(Type type)
        {
            if (types.ContainsKey(type))
            {
                return types[type];
            }
            else
                return null;
        }

        public static Growl.UI.DestinationSettingsPanel GetSettingsPanel(Subscription s)
        {
            ISubscriptionHandler handler = GetHandler(s);
            Growl.UI.DestinationSettingsPanel panel = handler.GetSettingsPanel(s);
            return panel;
        }

        public static List<DestinationListItem> GetListItems()
        {
            List<DestinationListItem> list = new List<DestinationListItem>();
            foreach (IDestinationHandler handler in handlers)
            {
                list.AddRange(handler.GetListItems());
            }
            return list;
        }
    }
}
