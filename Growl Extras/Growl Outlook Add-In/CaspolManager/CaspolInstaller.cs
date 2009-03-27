using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;

namespace CaspolManager
{
    [RunInstaller(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public sealed partial class CaspolInstaller : Installer
    {
        public CaspolInstaller()
        {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            // Call the base implementation.
            base.Install(stateSaver);

            string targetDir = this.Context.Parameters["targetDir"];
            string groupName = this.Context.Parameters["groupName"];

            try
            {
                try
                {
                    Caspol.RemoveSecurityPolicy(groupName);
                }
                catch { }

                Caspol.AddSecurityPolicy(targetDir, groupName);
                stateSaver.Add("groupName", groupName);

            }
            catch (Exception ex)
            {
                throw new InstallException("Cannot set the security policy.", ex);
            }
        }

        public override void Rollback(System.Collections.IDictionary savedState)
        {
            // Call the base implementation.
            base.Rollback(savedState);

            // Check whether the "groupName" property is saved.
            // If it is not set, the Install method did not set the security policy.
            if ((savedState == null) || (savedState["groupName"] == null))
                return;

            // The groupName must be a unique name; otherwise, the method might delete wrong code group.
            string groupName = this.Context.Parameters["groupName"];
            if (String.IsNullOrEmpty(groupName))
                throw new InstallException("Cannot remove the security policy. The specified solution code group name is not valid.");

            try
            {
                Caspol.RemoveSecurityPolicy(groupName);
            }
            catch (Exception ex)
            {
                throw new InstallException("Cannot remove the security policy.", ex);
            }
        }


        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            // Call the base implementation.
            base.Uninstall(savedState);

            // Check whether the "groupName" property is saved.
            // If it is not set, the Install method did not set the security policy.
            if ((savedState == null) || (savedState["groupName"] == null))
                return;

            // The groupName must be a unique name; otherwise, the method might delete wrong code group.
            string groupName = this.Context.Parameters["groupName"];
            if (String.IsNullOrEmpty(groupName))
                throw new InstallException("Cannot remove the security policy. The specified solution code group name is not valid.");

            try
            {
                Caspol.RemoveSecurityPolicy(groupName);
            }
            catch
            {
                // suppress exception so the uninstall does not stop
            }
        }
    }
}