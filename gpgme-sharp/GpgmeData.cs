using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public abstract class GpgmeData : Stream
    {
        public const long EOF = 0;
        public const long ERROR = -1;

        protected const int SEEK_SET = 0; /* Seek from beginning of file. */
        protected const int SEEK_CUR = 1; /* Seek from current position. */
        protected const int SEEK_END = 2; /* Seek from end of file. */

        internal IntPtr dataPtr = IntPtr.Zero;

        public abstract bool IsValid { get; }

        internal GpgmeData() {
        }

        public int Read(byte[] buffer) {
            return Read(buffer, buffer.Length);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (!IsValid) {
                throw new InvalidDataBufferException("The data buffer is invalid.");
            }
            if (buffer == null) {
                throw new ArgumentNullException("buffer", "An empty destination buffer has been given.");
            }
            if (buffer.Length < (offset + count)) {
                throw new ArgumentException("The sum of offset and count is bigger than the destination buffer size.");
            }
            if (offset < 0 || count < 0) {
                throw new ArgumentOutOfRangeException("offset", "Invalid / negative offset or count value supplied.");
            }

            GCHandle pinned_buffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            long memaddr = pinned_buffer.AddrOfPinnedObject().ToInt64() + offset;
            var memaddr_ptr = (IntPtr) memaddr;

            var size = (UIntPtr) count;
            IntPtr bytes_read = libgpgme.gpgme_data_read(
                dataPtr,
                memaddr_ptr,
                size);

            pinned_buffer.Free();
            return bytes_read.ToInt32();
        }

        public int Read(byte[] buffer, int count) {
            if (!IsValid) {
                throw new InvalidDataBufferException("The data buffer is invalid.");
            }
            if (buffer == null) {
                throw new ArgumentNullException("buffer", "An empty destination buffer has been given.");
            }
            if (buffer.Length < count) {
                throw new ArgumentException("Requested number of bytes to read is bigger than the destination buffer.");
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "Negative read count value supplied.");
            }

            var bufsize = (UIntPtr) count;
            IntPtr bytes_read = libgpgme.gpgme_data_read(
                dataPtr,
                buffer,
                bufsize);

            return bytes_read.ToInt32();
        }

        public override long Seek(long offset, SeekOrigin whence) {
            if (!IsValid) {
                throw new InvalidDataBufferException("Invalid data buffer.");
            }

            int iwhence = SEEK_CUR;
            switch (whence) {
                case SeekOrigin.Begin:
                    iwhence = SEEK_SET;
                    break;
                case SeekOrigin.Current:
                    iwhence = SEEK_CUR;
                    break;
                case SeekOrigin.End:
                    iwhence = SEEK_END;
                    break;
            }

            if (libgpgme.use_lfs) {
                return libgpgme.gpgme_data_seek(
                    dataPtr,
                    offset,
                    iwhence);
            }
            var poffset = (IntPtr) offset;
            IntPtr offs = libgpgme.gpgme_data_seek(
                dataPtr,
                poffset,
                iwhence);

            return offs.ToInt64();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (!IsValid) {
                throw new InvalidDataBufferException("Invalid data buffer");
            }
            if (buffer == null) {
                throw new ArgumentNullException("buffer", "Empty source buffer given.");
            }
            if (buffer.Length < (offset + count)) {
                throw new ArgumentException(
                    "Requested number of bytes to write is bigger than the source buffer starting at offset " +
                        offset.ToString(CultureInfo.InvariantCulture) + ".");
            }
            if (offset < 0 || count < 0) {
                throw new ArgumentOutOfRangeException("offset", "The offset or count is negative.");
            }

            GCHandle pinned_buffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            long memaddr = pinned_buffer.AddrOfPinnedObject().ToInt64() + offset;
            IntPtr memaddr_ptr = (IntPtr) memaddr;

            UIntPtr bufsize = (UIntPtr) count;
            
            libgpgme.gpgme_data_write(
                dataPtr,
                memaddr_ptr,
                bufsize);

            pinned_buffer.Free();
        }

        public int Write(byte[] buffer, int count) {
            if (!IsValid) {
                throw new InvalidDataBufferException("Invalid data buffer");
            }
            if (buffer == null) {
                throw new ArgumentNullException("buffer", "An empty source buffer has been given.");
            }
            if (buffer.Length < count) {
                throw new ArgumentException("Requested number of bytes to write is bigger than the source buffer.");
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "The read count is negative.");
            }

            var bufsize = (UIntPtr) count;
            IntPtr bytes_written = libgpgme.gpgme_data_write(
                dataPtr,
                buffer,
                bufsize);

            return bytes_written.ToInt32();
        }

        public override void SetLength(long value) {
            if (!IsValid) {
                throw new InvalidDataBufferException("Invalid data buffer");
            }

            throw new NotSupportedException("SetLength(long) is not supported.");
        }

        public string FileName {
            get {
                if (!IsValid) {
                    throw new InvalidDataBufferException();
                }
                IntPtr ptr = libgpgme.gpgme_data_get_file_name(dataPtr);
                
                return (!ptr.Equals(IntPtr.Zero))
                    ? Gpgme.PtrToStringAnsi(ptr) 
                    : null;
            }
            set {
                if (!IsValid) {
                    throw new InvalidDataBufferException();
                }
                if (value == null) {
                    throw new ArgumentNullException("value", "Invalid file path.");
                }

                IntPtr ptr = Marshal.StringToCoTaskMemAnsi(value);
                if (!ptr.Equals(IntPtr.Zero)) {
                    int err = libgpgme.gpgme_data_set_file_name(dataPtr, ptr);
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                    if (ptr != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                        if (errcode == gpg_err_code_t.GPG_ERR_ENOMEM) {
                            throw new OutOfMemoryException();
                        }
                        throw new GeneralErrorException("Unknown error " + errcode + " (" + err + ")");
                    }
                } else {
                    throw new OutOfMemoryException();
                }
            }
        }
        public DataEncoding Encoding {
            get {
                if (!IsValid) {
                    throw new InvalidDataBufferException();
                }

                gpgme_data_encoding_t enc = libgpgme.gpgme_data_get_encoding(dataPtr);

                return (DataEncoding) enc;
            }
            set {
                if (!IsValid) {
                    throw new InvalidDataBufferException();
                }

                var enc = (gpgme_data_encoding_t) value;

                int err = libgpgme.gpgme_data_set_encoding(dataPtr, enc);
                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                    throw new GeneralErrorException("Could not set data encoding to " + value);
                }
            }
        }

        public void Rewind() {
            Seek(0, SeekOrigin.Begin);
        }

        public override long Position {
            get {
                if (!CanSeek) {
                    throw new NotSupportedException();
                }
                return Seek(0, SeekOrigin.Current);
            }
            set {
                if (!CanSeek) {
                    throw new NotSupportedException();
                }
                Seek(value, SeekOrigin.Begin);
            }
        }

#if (VERBOSE_DEBUG)
		internal void DebugOutput(string text)
        {
            Console.WriteLine("Debug: " + text);
			Console.Out.Flush();
        }
#endif
    }
}