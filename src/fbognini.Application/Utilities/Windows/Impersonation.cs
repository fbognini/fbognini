using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace fbognini.Core.Utilities
{
    public class Impersonation : IDisposable
    {
        //private System.Security.Principal.WindowsImpersonationContext impersonationContext;

        public Impersonation(
            string domain,
            string userName,
            string password)
        {
            ImpersonateUser(domain, userName, password);
        }
        
        private bool ImpersonateUser(
            string domain,
            string userName,
            string password)
        {
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (NativeMethods.RevertToSelf())
            {
                if (NativeMethods.LogonUser(userName, domain, password, NativeMethods.LogonType.NewCredentials, NativeMethods.LogonProvider.Default, out token))
                {
                    if (NativeMethods.DuplicateToken(token, NativeMethods.SecurityImpersonationLevel.Impersonation, out tokenDuplicate))
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        //impersonationContext = tempWindowsIdentity.Impersonate();
                        WindowsIdentity.RunImpersonated(tempWindowsIdentity.AccessToken, () =>
                        {
                            var useri = WindowsIdentity.GetCurrent();
                            return true;
                        });

                        //if (impersonationContext != null)
                        //{
                        //    CloseHandle(token);
                        //    CloseHandle(tokenDuplicate);
                        //    return true;
                        //}
                    }
                }
            }
            if (token != IntPtr.Zero)
                NativeMethods.CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                NativeMethods.CloseHandle(tokenDuplicate);
            return false;
        }
        
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
