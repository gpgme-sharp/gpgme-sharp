using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_op_verify_result //gpgme_verify_result_t
    {
        public IntPtr signature; //gpgme_signature_t

        /* The original file name of the plaintext message, if
           available.  */
        public IntPtr file_name; //char *
    }
}