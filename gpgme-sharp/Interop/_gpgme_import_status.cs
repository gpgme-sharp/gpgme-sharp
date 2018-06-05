using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_import_status
    {
        public IntPtr next; //_gpgme_import_status

        /* Fingerprint.  */
        public IntPtr fpr; // char *

        /* If a problem occured, the reason why the key could not be
           imported.  Otherwise GPGME_No_Error.  */
        public int result; //gpgme_error_t

        /* The result of the import, the GPGME_IMPORT_* values bit-wise
           ORed.  0 means the key was already known and no new components
           have been added.  */
        public uint status;

        internal _gpgme_import_status() {
            next = IntPtr.Zero;
            fpr = IntPtr.Zero;
            result = 0;
            status = 0;
        }
    }
}