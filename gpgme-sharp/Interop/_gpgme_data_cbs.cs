using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_data_cbs
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public gpgme_data_read_cb_t read;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public gpgme_data_write_cb_t write;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public gpgme_data_seek_cb_t seek;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public gpgme_data_release_cb_t release;

        internal _gpgme_data_cbs() {
            read = null;
            write = null;
            seek = null;
            release = null;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_data_cbs_lfs
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public gpgme_data_read_cb_t read;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public gpgme_data_write_cb_t write;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public gpgme_data_seek_cb_t_lfs seek;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public gpgme_data_release_cb_t release;

        internal _gpgme_data_cbs_lfs() {
            read = null;
            write = null;
            seek = null;
            release = null;
        }
    }
}