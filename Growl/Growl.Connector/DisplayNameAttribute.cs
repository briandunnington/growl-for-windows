using System;

namespace Growl.Connector
{
    /// <summary>
    /// Provides a friendly display name (string) for enumerated values.
    /// </summary>
    /// <remarks>
    /// The DisplayNameAttribute is only allowed on Fields, and only one DisplayNameAttribute is allowed per Field.
    /// <code>
    /// Usage:
    ///
    ///	enum MyEnum : int
    ///	{
    ///		[DisplayNameAttribute("Friendly display text goes here")]
    ///		None = 1,
    ///		[DisplayNameAttribute("This value has different display text")]
    ///		Some = 2,
    ///		All = 3
    ///	}
    /// </code>
    /// 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DisplayNameAttribute : System.Attribute
    {
        /// <summary>
        /// The friendly name of the value
        /// </summary>
        private readonly string displayName;

        /// <summary>
        /// Creates a new instance of the DisplayNameAttribute class
        /// </summary>
        /// <param name="displayName">Friendly name for the enum value</param>
        public DisplayNameAttribute(string displayName)
        {
            this.displayName = (displayName != null ?  displayName : String.Empty);
        }

        /// <summary>
        /// Gets the friendly name
        /// </summary>
        /// <value>
        /// The friendly name associated with the enumerated value
        /// </value>
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
        }
    }
}