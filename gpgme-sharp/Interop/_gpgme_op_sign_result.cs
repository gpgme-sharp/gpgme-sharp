using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_op_sign_result
    {
        /* The list of invalid signers.  */
        public IntPtr invalid_signers; // gpgme_invalid_key_t
        public IntPtr signatures; // gpgme_new_signature_t
    }
}