using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    /* The engine information structure.  */

    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_engine_info //*gpgme_engine_info_t;
    {
        public IntPtr next;

        /* The protocol ID.  */
        public gpgme_protocol_t protocol;

        /* The file name of the engine binary.  */
        public IntPtr file_name;

        /* The version string of the installed engine.  */
        public IntPtr version;

        /* The minimum version required for GPGME.  */
        public IntPtr req_version;

        /* The home directory used, or NULL if default.  */
        public IntPtr home_dir;

        internal _gpgme_engine_info() {
            next = IntPtr.Zero;
            protocol = gpgme_protocol_t.GPGME_PROTOCOL_UNKNOWN;
            file_name = IntPtr.Zero;
            version = IntPtr.Zero;
            req_version = IntPtr.Zero;
            home_dir = IntPtr.Zero;
        }
    }
}