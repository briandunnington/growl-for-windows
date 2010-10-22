using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    public class ToastyForwardDestinationHandler : IForwardDestinationHandler
    {
        #region IDestinationHandler Members

        public string Name
        {
            get
            {
                return "Toasty";
            }
        }

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(ToastyForwardDestination));
            return list;
        }

        public List<DestinationListItem> GetListItems()
        {
            ForwardDestinationListItem item = new ForwardDestinationListItem("Forward to your Windows Phone\nwith Toasty", GetIcon(), this);
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        public DestinationSettingsPanel GetSettingsPanel(DestinationListItem dbli)
        {
            return new Growl.UI.ToastyForwardInputs();
        }

        public DestinationSettingsPanel GetSettingsPanel(DestinationBase db)
        {
            return new Growl.UI.ToastyForwardInputs();
        }

        #endregion

        internal static System.Drawing.Image GetIcon()
        {
            return new System.Drawing.Bitmap(Properties.Resources.toasty);
        }

        internal static string Fetch(object enumField)
        {
            if (enumField == null)
                return null;

            try
            {
                // determine what type of object we are dealing with
                Type myType = enumField.GetType().UnderlyingSystemType;

                // get the specific field we are interested in
                System.Reflection.FieldInfo field = myType.GetField(enumField.ToString());

                // load the DisplayNameAttribute for the object (there should be 1 and only 1)
                object[] attributes = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attributes != null && attributes.Length == 1)
                {
                    // get the DescriptionAttribute property from the attribute
                    System.ComponentModel.DescriptionAttribute attribute = (System.ComponentModel.DescriptionAttribute)attributes[0];
                    return attribute.Description;
                }
            }
            catch
            {
            }

            // if we couldn't get the DescriptionAttribute (or it wasn't set),
            // then default back to the name of the field
            return enumField.ToString();
        }
    }
}
