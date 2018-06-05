using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_op_keylist_result // *gpgme_keylist_result_t
    {
        public uint flags;

        public bool truncated {
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