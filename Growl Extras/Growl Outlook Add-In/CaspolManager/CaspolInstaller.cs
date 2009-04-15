using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Policy;

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
            try
            {
                PolicyLevel enterprise;
                PolicyLevel machine;
                PolicyLevel user;

                string assemblyLocation = this.Context.Parameters["assemblyLocation"];
                string groupName = this.Context.Parameters["groupName"];

                IEnumerator enumerator = SecurityManager.PolicyHierarchy();
                // 1st one is enterprise
                enumerator.MoveNext();
                enterprise = (PolicyLevel)enumerator.Current;
                // 2nd one is machine
                enumerator.MoveNext();
                machine = (PolicyLevel)enumerator.Current;
                // 3rd one is user
                enumerator.MoveNext();
                user = (PolicyLevel)enumerator.Current;

                PermissionSet permissionSet = user.GetNamedPermissionSet("FullTrust");
                PolicyStatement statement = new PolicyStatement(permissionSet, PolicyStatementAttribute.Nothing);
                UrlMembershipCondition condition = new UrlMembershipCondition(assemblyLocation);
                CodeGroup codeGroup = new UnionCodeGroup(condition, statement);
                codeGroup.Name = groupName;

                // see if the code group already exists, and if so, remove it
                CodeGroup existingCodeGroup = null;
                foreach (CodeGroup group in user.RootCodeGroup.Children)
                {
                    if (group.Name == codeGroup.Name)
                    {
                        existingCodeGroup = group;
                        break;
                    }
                }
                if (existingCodeGroup != null) user.RootCodeGroup.RemoveChild(existingCodeGroup);
                SecurityManager.SavePolicy();

                // add the code group
                user.RootCodeGroup.AddChild(codeGroup);
                SecurityManager.SavePolicy();
            }
            catch (Exception ex)
            {
                throw new InstallException("Cannot set the security policy.", ex);
            }

            // Call the base implementation.
            base.Install(stateSaver);
        }

        public override void Rollback(System.Collections.IDictionary savedState)
        {
            // Call the base implementation.
            base.Rollback(savedState);
        }


        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            // Call the base implementation.
            base.Uninstall(savedState);
        }
    }
}