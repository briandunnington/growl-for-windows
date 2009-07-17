using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public static class ForwardDestinationManager
    {
        private static List<IForwardDestinationHandler> handlers = new List<IForwardDestinationHandler>();
        private static Dictionary<Type, IForwardDestinationHandler> types = new Dictionary<Type, IForwardDestinationHandler>();

        public static void Initialize()
        {
            Register(new BonjourForwardDestinationHandler());   // make sure this is the first item in the list
            Register(new ManualForwardDestinationHandler());
            Register(new ProwlForwardDestinationHandler());
            //Register(new EmailForwardDestinationHandler());
            Register(new TwitterForwardDestinationHandler());
        }

        private static void Register(IForwardDestinationHandler ifdh)
        {
            if (ifdh != null)
            {
                handlers.Add(ifdh);

                List<Type> list = ifdh.Register();
                foreach (Type type in list)
                {
                    if (typeof(ForwardDestination).IsAssignableFrom(type))
                    {
                        if (!types.ContainsKey(type))
                        {
                            lock (types)
                            {
                                if (!types.ContainsKey(type))
                                {
                                    types.Add(type, ifdh);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static IForwardDestinationHandler GetHandler(ForwardDestination fd)
        {
            Type type = fd.GetType();
            return GetHandler(type);
        }

        public static IForwardDestinationHandler GetHandler(Type type)
        {
            if (types.ContainsKey(type))
            {
                return types[type];
            }
            else
                return null;
        }

        public static Growl.UI.ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestination fd)
        {
            IForwardDestinationHandler handler = GetHandler(fd);
            Growl.UI.ForwardDestinationSettingsPanel panel = handler.GetSettingsPanel(fd);
            return panel;
        }

        public static List<ForwardDestinationListItem> GetListItems()
        {
            List<ForwardDestinationListItem> list = new List<ForwardDestinationListItem>();
            foreach (IForwardDestinationHandler handler in handlers)
            {
                list.AddRange(handler.GetListItems());
            }
            return list;
        }
    }
}
