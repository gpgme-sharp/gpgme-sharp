using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    /* Trust items and operations.  */

    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_trust_item
    {
        public _gpgme_trust_item() {
            _keyid = new byte[17];
            _owner_trust = new byte[2];
            _validity = new byte[2];
        }

        /* Internal to GPGME, do not use.  */
        public uint _refs;

        /* The key ID to which the trust item belongs.  */
        public IntPtr keyid; // char *

        /* Internal to GPGME, do not use.  */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public byte[] _keyid;

        /* The type of the trust item, 1 refers to a key, 2 to a user ID.  */
        public int type;

        /* The trust level.  */
        public int level;

        /* The owner trust if TYPE is 1.  */
        public IntPtr owner_trust; //char *

        /* Internal to GPGME, do not use.  */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] _owner_trust;

        /* The calculated validity.  */
        public IntPtr validity; //char *

        /* Internal to GPGME, do not use.  */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] _validity;

        /* The user name if TYPE is 2.  */
        public IntPtr name; //char *
    }
}