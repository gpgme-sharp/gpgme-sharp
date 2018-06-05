using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_op_genkey_result
    {
        private uint flags;
        public IntPtr fpr;

        /* A primary key was generated.  */
        public bool primary {
            get { return ((flags & 1) > 0); }
            set {
                if (value) {
                    flags |= 1;
                } else {
                    flags &= (~(uint) 1);
                }
            }
        }
        /* A sub key was generated.  */
        public bool sub {
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