using System;
using System.IO;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public sealed class GpgmeMemoryData : GpgmeData
    {
        private static readonly object _global_lock = new object();
        private readonly bool _free_mem;

        private IntPtr _mem_ptr;
        private UIntPtr _mem_size;

        public GpgmeMemoryData() {
            int err = libgpgme.gpgme_data_new(out dataPtr);
            gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);


            if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                // everything went fine
                return;
            }

            if (errcode == gpg_err_code_t.GPG_ERR_ENOMEM) {
                throw new OutOfMemoryException("Not enough memory available to create GPGME data object.");
            }

            throw new GeneralErrorException("Unknown error " + errcode + " (" + err + ")");
        }

        public GpgmeMemoryData(int size) {
            IntPtr tmp_ptr = Marshal.AllocCoTaskMem(size);

            if (tmp_ptr.Equals(IntPtr.Zero)) {
                throw new OutOfMemoryException();
            }

            _free_mem = true;

            InitGpgmeMemoryData(tmp_ptr, size);
        }

        public GpgmeMemoryData(IntPtr memAddr, int size) {
            _free_mem = false;

            InitGpgmeMemoryData(memAddr, size);
        }

        public GpgmeMemoryData(string filename)
            : this(filename, 0, (long) -1) {
        }

        public GpgmeMemoryData(string filename, int offset, int length)
            : this(filename, offset, (long) length) {
        }

        public GpgmeMemoryData(string filename, long offset, long length) {
            var finfo = new FileInfo(filename);
            if (!finfo.Exists) {
                throw new FileNotFoundException("The supplied file could not be found.", filename);
            }

            if (offset == 0 && length == -1) {
                length = (int) finfo.Length;
            }

            if (finfo.Length < (offset + length)) {
                throw new ArgumentException("The file size is smaller than file offset + length.");
            }

            using (FileStream f = finfo.OpenRead()) {
                if (!f.CanRead) {
                    throw new FileLoadException("Cannot read file " + filename + ".", filename);
                }
            }

            IntPtr pfilepath = Marshal.StringToCoTaskMemAnsi(filename);
            IntPtr handle = IntPtr.Zero;
            var plen = (UIntPtr) length;

            int err;
            if (libgpgme.use_lfs) {
                err = libgpgme.gpgme_data_new_from_filepart(
                    out dataPtr,
                    pfilepath,
                    handle,
                    offset,
                    plen);
            } else {
                var poffset = (IntPtr) offset;
                err = libgpgme.gpgme_data_new_from_filepart(
                    out dataPtr,
                    pfilepath,
                    handle,
                    poffset,
                    plen);
            }

            gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

            if (pfilepath != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(pfilepath);
            }


            if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                // everything went fine, set default filename
                FileName = finfo.Name;

                return;
            }

            if (errcode == gpg_err_code_t.GPG_ERR_ENOMEM) {
                throw new OutOfMemoryException("Not enough memory available to create GPGME data object.");
            }

            throw new GeneralErrorException("Unknown error " + errcode + " (" + err + ")");
        }

        public override long Length {
            get {
                if (!_mem_ptr.Equals(IntPtr.Zero)) {
                    return (long) _mem_size;
                }

                // save the current position
                long pos = Position;

                // read the current stream length
                long len = Seek(0, SeekOrigin.End);

                // restore old position
                Position = pos;

                return len;
            }
        }

        public override bool IsValid {
            get { return (!dataPtr.Equals(IntPtr.Zero)); }
        }

        public override bool CanRead {
            get { return true; }
        }
        public override bool CanWrite {
            get { return true; }
        }
        public override bool CanSeek {
            get { return true; }
        }
        public IntPtr MemoryAddress {
            get { return _mem_ptr; }
        }
        public long MemorySize {
            get { return (long) _mem_size; }
        }

        ~GpgmeMemoryData() {
            ReleaseMemoryData();
        }

        private void InitGpgmeMemoryData(IntPtr memAddr, int size) {
            _mem_ptr = memAddr;
            if (_mem_ptr.Equals(IntPtr.Zero)) {
                _mem_size = UIntPtr.Zero;
                throw new ArgumentException("The supplied memory address was 0.");
            }

            _mem_size = (UIntPtr) size;

            /* If COPY is not zero, a private copy of the data is made. If COPY
               is zero, the data is taken from the specified buffer as needed,
               and the user has to ensure that the buffer remains valid for the
               whole life span of the data object. */
            const int COPY_FLAG = 0;
            int err = libgpgme.gpgme_data_new_from_mem(
                out dataPtr,
                _mem_ptr,
                _mem_size,
                COPY_FLAG);

            gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

            if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                // everything went fine
                return;
            }

            if (errcode == gpg_err_code_t.GPG_ERR_ENOMEM) {
                throw new OutOfMemoryException("Not enough memory available to create GPGME data object.");
            }

            throw new GeneralErrorException("Unknown error " + errcode + " (" + err + ")");
        }

        private void ReleaseMemoryData() {
            lock (_global_lock) {
                if (!dataPtr.Equals(IntPtr.Zero)) {
                    libgpgme.gpgme_data_release(dataPtr);
                    dataPtr = IntPtr.Zero;
                }
                if (!_mem_ptr.Equals(IntPtr.Zero)) {
                    if (_free_mem) {
                        Marshal.FreeCoTaskMem(_mem_ptr);
                    }
                    _mem_ptr = IntPtr.Zero;
                    _mem_size = UIntPtr.Zero;
                }
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                ReleaseMemoryData(); // forced in deconstructor anyway
                GC.SuppressFinalize(this);
            }
        }

        public override void Flush() {
        }
    }
}