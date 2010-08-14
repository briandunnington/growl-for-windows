using System;
using System.Security.Principal;
using System.Diagnostics;

namespace Growl
{
    /// <summary>
    /// A set of User Account Control helper methods.
    /// </summary>
    internal static class UserAccountControlHelper
    {
        /// <summary>
        /// Gets a value indicating whether the process is elevated.
        /// </summary>
        /// <returns>Returns whether the current process is elevated.</returns>
        public static bool IsElevated()
        {
            if (WindowsSupportsUserAccountControl())
            {
                AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);
                WindowsIdentity wi = WindowsIdentity.GetCurrent();
                WindowsPrincipal wp = new WindowsPrincipal(wi);

                return (wp.IsInRole(WindowsBuiltInRole.Administrator));
            }
            else
            {
                // Always "elevated" for previous Windows operating systems.
                return true;
            }
        }

        /// <summary>
        /// Simple check for whether the operating system version supports User
        /// Account Control. Windows Vista, Windows 7, and server variants are
        /// all supported with this major version greater-than-or-equal math.
        /// </summary>
        /// <returns>Returns true on modern Windows operating systems.</returns>
        internal static bool WindowsSupportsUserAccountControl()
        {
            return Environment.OSVersion.Version.Major >= 6;
        }

        /// <summary>
        /// Launches the application with elevated privileges
        /// </summary>
        /// <param name="path">The path to the application to launch.</param>
        /// <param name="arguments">Any arguments to pass to the application.</param>
        /// <param name="wait"><c>true</c> to wait for the application to exit before proceeding;<c>false otherwise</c></param>
        public static void LaunchElevatedApplication(string path, string arguments, bool wait)
        {
            Process process = null;
            ProcessStartInfo psi = new ProcessStartInfo(path, arguments);
            if(WindowsSupportsUserAccountControl())
                psi.Verb = "runas";

            psi.UseShellExecute = true;

            try
            {
                process = System.Diagnostics.Process.Start(psi);
                if(wait) process.WaitForExit();
            }
            catch (Exception ex)
            {
                Utility.WriteDebugInfo(String.Format("EXCEPTION: Could not launch elevated process. - {0}", ex.ToString()));
            }
            finally
            {
                if (process != null)
                {
                    process.Dispose();
                }
            }
        }
    }
}
