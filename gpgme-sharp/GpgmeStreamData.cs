using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Libgpgme
{
    public class GpgmeStreamData : GpgmeCbsData
    {
        protected Stream iostream;

        // give sub classes the possibility to set "iostream" by themself
        protected GpgmeStreamData() {
        }

        protected GpgmeStreamData(bool canRead, bool canWrite, bool canSeek, bool canRelease)
            : base(canRead, canWrite, canSeek, canRelease) {
        }

        public GpgmeStreamData(Stream streamobj)
            : base(streamobj.CanRead, streamobj.CanWrite, streamobj.CanSeek, false) {
            iostream = streamobj;
            if (streamobj == null) {
                throw new ArgumentNullException();
            }
        }

        public override bool IsValid {
            get {
                return (iostream != null &&
                    (!dataPtr.Equals(IntPtr.Zero)));
            }
        }

        public override long Length {
            get {
                if (iostream != null) {
                    return iostream.Length;
                }
                throw new ObjectDisposedException("iostream");
            }
        }

        public override bool CanRead {
            get {
                if (iostream != null) {
                    return iostream.CanRead;
                }
                return false;
            }
        }
        public override bool CanSeek {
            get {
                if (iostream != null) {
                    return iostream.CanSeek;
                }
                return false;
            }
        }
        public override bool CanRelease {
            get { return true; }
        }
        public override bool CanWrite {
            get {
                if (iostream != null) {
                    return iostream.CanWrite;
                }
                return false;
            }
        }
        public override bool CanTimeout {
            get {
                if (iostream != null) {
                    return iostream.CanTimeout;
                }
                return false;
            }
        }
        public Stream OriginStream {
            get { return iostream; }
            protected set { iostream = value; }
        }

        ~GpgmeStreamData() {
            CleanUp();
        }

        protected override IntPtr ReadCB(IntPtr bufPtr, long size) {
            if (iostream != null && iostream.CanRead) {
                var buf = new byte[(int) size];
                int bytes_read = iostream.Read(buf, 0, (int) size);
                if (bytes_read > 0) {
                    Marshal.Copy(buf, 0, bufPtr, bytes_read);
                }

                return (IntPtr) bytes_read;
            }
            return (IntPtr) ERROR;
        }

        protected override IntPtr WriteCB(IntPtr bufPtr, long size) {
            if (iostream != null && iostream.CanWrite) {
                long oldpos = iostream.Position;
                var buf = new byte[(int) size];
                if (size > 0) {
                    Marshal.Copy(bufPtr, buf, 0, (int) size);
                }

                iostream.Write(buf, 0, (int) size);

                return (IntPtr) (iostream.Position - oldpos);
            }
            return (IntPtr) ERROR;
        }

        protected override long SeekCB(long offset, SeekOrigin whence) {
            if (iostream != null && iostream.CanSeek) {
                return iostream.Seek(offset, whence);
            }
            return ERROR;
        }

        protected override void ReleaseCB() {
            //
        }

        private void CleanUp() {
            iostream = null;
        }

        protected override void Dispose(bool disposing) {
            GC.SuppressFinalize(this);
            CleanUp();

            base.Dispose(disposing);
        }

        public override void Flush() {
            if (iostream != null) {
                iostream.Flush();
            } else {
                throw new IOException();
            }
        }

        public override void SetLength(long value) {
            if (iostream != null) {
                iostream.SetLength(value);
            } else {
                throw new ObjectDisposedException("iostream");
            }
        }
    }
}