using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    /* An user ID from a key.  */

    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_user_id // gpgme_user_id_t
    {
        public IntPtr next;
        /* True if the user ID is revoked.  
            unsigned int revoked : 1;

           True if the user ID is invalid.  
            unsigned int invalid : 1;

           Internal to GPGME, do not use.  
            unsigned int _unused : 30; */
        public uint flags;

        /* The validity of the user ID.  */
        public gpgme_validity_t validity;

        /* The user ID string.  */
        public IntPtr uid; // char*

        /* The name part of the user ID.  */
        public IntPtr name; // char*

        /* The email part of the user ID.  */
        public IntPtr email; // char*

        /* The comment part of the user ID.  */
        public IntPtr comment; // char*

        /* The signatures of the user ID.  */
        public IntPtr signatures; // gpgme_key_sig_t

        /* Internal to GPGME, do not use.  */
        public IntPtr _last_keysig; //gpgme_key_sig_t

        public bool revoked {
            get { return ((flags & 1) > 0); }
            set {
                if (value) {
                    flags |= 1;
                } else {
                    flags &= (~(uint) 1);
                }
            }
        }
        public bool invalid {
            get { return ((flags & 2) > 0); }
            set {
                if (value) {
                    flags |= 2;
                } else {
                    flags &= (~(uint) 2);
                }
            }
        }
    }
}