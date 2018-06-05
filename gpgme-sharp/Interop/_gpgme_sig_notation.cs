using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_sig_notation // *gpgme_sig_notation_t
    {
        public IntPtr next;

        /* If NAME is a null pointer, then VALUE contains a policy URL
           rather than a notation.  */
        public IntPtr name; // char*

        /* The value of the notation data.  */
        public IntPtr value; // char*

        /* The length of the name of the notation data.  */
        public int name_len;

        /* The length of the value of the notation data.  */
        public int value_len;

        /* The accumulated flags.  */
        public gpgme_sig_notation_flags_t flags;

        /* Notation data is human-readable.  
                uint human_readable : 1;
           Notation data is critical.  
                uint critical : 1;
           Internal to GPGME, do not use.  
                int _unused : 30;
         */
        public uint additionalflags;

        public bool human_readable {
            get { return ((additionalflags & 1) > 0); }
            set {
                if (value) {
                    additionalflags |= 1;
                } else {
                    additionalflags &= (~(uint) 1);
                }
            }
        }
        public bool critical {
            get { return ((additionalflags & 2) > 0); }
            set {
                if (value) {
                    additionalflags |= 2;
                } else {
                    additionalflags &= (~(uint) 2);
                }
            }
        }
    }
}