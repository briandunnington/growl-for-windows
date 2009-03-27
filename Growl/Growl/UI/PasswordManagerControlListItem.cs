using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.UI
{
    internal class PasswordManagerControlListItem
    {
        private const char PASSWORD_CHAR = '\u25CF';

        private Growl.Connector.Password password;

        public PasswordManagerControlListItem()
        {
        }

        public PasswordManagerControlListItem(Growl.Connector.Password password)
        {
            this.password = password;
        }

        public Growl.Connector.Password Password
        {
            get
            {
                return this.password;
            }
        }

        public override string ToString()
        {
            string mask = new String(PASSWORD_CHAR, this.password.ActualPassword.Length);
            return mask;
        }
    }
}
