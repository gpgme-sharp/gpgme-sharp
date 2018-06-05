using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_invalid_key //gpgme_invalid_key_t
    {
        public IntPtr next; //_gpgme_invalid_key 
        public IntPtr fpr; //char *
        public int reason; // gpgme_error_t

        internal _gpgme_invalid_key() {
            next = IntPtr.Zero;
            fpr = IntPtr.Zero;
            reason = 0;
        }
    }
}