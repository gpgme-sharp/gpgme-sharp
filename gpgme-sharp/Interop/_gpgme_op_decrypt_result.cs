using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_op_decrypt_result // gpgme_decrypt_result_t
    {
        public IntPtr unsupported_algorithm; // char *

        /* Key should not have been used for encryption.  
        uint wrong_key_usage : 1;

        /* Internal to GPGME, do not use.  
        int _unused : 31;
        */
        public uint flags;

        public IntPtr recipients; //gpgme_recipient_t

        /* The original file name of the plaintext message, if
           available.  */
        public IntPtr file_name; //char *

        public bool wrong_key_usage {
            get { return ((flags & 1) > 0); }
            set {
                if (value) {
                    flags |= 1;
                } else {
                    flags &= (~(uint) 1);
                }
            }
        }
    }
}