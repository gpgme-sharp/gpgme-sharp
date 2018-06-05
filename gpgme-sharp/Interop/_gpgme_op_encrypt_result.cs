using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    /* Encryption.  */

    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_op_encrypt_result //gpgme_encrypt_result_t
    {
        /* The list of invalid recipients.  */
        public IntPtr invalid_recipients; //gpgme_invalid_key_t
    }
}