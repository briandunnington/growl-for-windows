using System;
using System.Reflection;

namespace Growl.Connector
{
    /// <summary>
    /// Use the DisplayName class to retrieve the friendly display name from enumerated
    /// values that make use of the DisplayNameAttribute
    /// </summary>
    /// <remarks>
    /// This static class is the accessor for Enum fields that use the DisplayNameAttribute
    /// custom attribute.
    /// </remarks>
    public sealed class DisplayName
    {
        /// <summary>
        /// The default constructor is private so that this class can not be instanced
        /// </summary>
        private DisplayName() { }

        /// <summary>
        /// The Fetch method retrieves the <see cref="DisplayNameAttribute.DisplayName"/> value from the
        /// <see cref="DisplayNameAttribute"/> decorating the enum field.
        /// of the enumField passed in.
        /// </summary>
        /// <param name="enumField">A specific field of an enumeration (MyEnum.Field)</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="enumField" /> is null</exception>
        /// <returns cref="string">
        /// String containing the value of <see cref="DisplayNameAttribute.DisplayName"/> if set.
        /// If the DisplayNameAttribute was not set on the enum, the enumField's variable name is returned instead.
        /// </returns>
        public static string Fetch(object enumField)
        {
            // parameter checking
            if (enumField == null)
                throw new ArgumentNullException("enumField", "Fetch: 'enumField' parameter cannot be null.");

            try
            {
                // determine what type of object we are dealing with
                Type myType = enumField.GetType().UnderlyingSystemType;

                // get the specific field we are interested in
                FieldInfo field = myType.GetField(enumField.ToString());

                // load the DisplayNameAttribute for the object (there should be 1 and only 1)
                object[] attributes = field.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (attributes != null && attributes.Length == 1)
                {
                    // get the DisplayName property from the attribute
                    DisplayNameAttribute attribute = (DisplayNameAttribute)attributes[0];
                    return attribute.DisplayName;
                }
            }
            catch
            {
            }

            // if we couldn't get the DisplayNameAttribute (or it wasn't set),
            // then default back to the name of the field
            return enumField.ToString();
        }
    }
}