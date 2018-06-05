using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Libgpgme.Interop;

namespace Libgpgme
{
    public abstract class GpgmeCbsData : GpgmeData
    {
        private static IntPtr _global_handle = IntPtr.Zero;
        private static readonly object _global_lock = new object();
        private readonly object _local_lock = new object();
        private readonly ManualResetEvent _release_cbevent = new ManualResetEvent(false);

        private _gpgme_data_cbs _cbs;
        // See GPGME manual: 2.3 Largefile Support (LFS)

        private IntPtr _cbs_ptr;
        private _gpgme_data_cbs_lfs _cbs_lfs;
        private IntPtr _handle;
        private GCHandle _pinned_cbs;
        private GCHandle _pinned_cbs_lfs;

        private bool _release_cbfunc_init;

        public Exception LastCallbackException;

        protected GpgmeCbsData() {
            // Inherited class overrides CanRead etc.
            Init(CanRead, CanWrite, CanSeek, CanRelease);
        }

        protected GpgmeCbsData(bool canRead, bool canWrite, bool canSeek, bool canRelease) {
            // The user specifies the implemented callback functions directly.
            Init(canRead, canWrite, canSeek, canRelease);
        }

        public abstract override bool CanRead { get; }
        public abstract override bool CanWrite { get; }
        public abstract override bool CanSeek { get; }
        public abstract bool CanRelease { get; }

        private IntPtr IncGlobalHandle() {
            lock (_global_lock) {
                long value = _global_handle.ToInt64();
                ++value;
                _global_handle = (IntPtr) value;
                return _global_handle;
            }
        }

        ~GpgmeCbsData() {
            ReleaseCbsData();
        }

        private void ReleaseCbsData() {
            if (!dataPtr.Equals(IntPtr.Zero) && !_release_cbfunc_init) {
                libgpgme.gpgme_data_release(dataPtr);

                if (libgpgme.use_lfs) {
                    if (_cbs_lfs.release != null) {
                        _release_cbfunc_init = true;
                        // wait until libgpgme has called the _release callback method
                        _release_cbevent.WaitOne();
                    }
                } else {
                    if (_cbs.release != null) {
                        _release_cbfunc_init = true;
                        // wait until libgpgme has called the _release callback method
                        _release_cbevent.WaitOne();
                    }
                }

                dataPtr = IntPtr.Zero;
            }
            lock (_local_lock) {
                if (!_cbs_ptr.Equals(IntPtr.Zero)) {
                    Marshal.FreeCoTaskMem(_cbs_ptr);
                    _cbs_ptr = IntPtr.Zero;
                }
                if (_pinned_cbs.IsAllocated) {
                    _pinned_cbs.Free();
                }
                if (_pinned_cbs_lfs.IsAllocated) {
                    _pinned_cbs_lfs.Free();
                }
            }
        }

        protected override void Dispose(bool disposing) {
            GC.SuppressFinalize(this);
            ReleaseCbsData();

            base.Dispose(disposing);
        }

        private void Init(bool canRead, bool canWrite, bool canSeek, bool canRelease) {
#if (VERBOSE_DEBUG)
			DebugOutput("GpgmeCbsData.Init(" + canRead.ToString() + "," 
			            + canWrite.ToString() + ","
			            + canSeek.ToString() + ","
			            + canRelease.ToString() + ")");
#endif
            _handle = IncGlobalHandle(); // increment the global handle 

            _cbs = new _gpgme_data_cbs();
            _cbs_lfs = new _gpgme_data_cbs_lfs();

            // Read function
            if (canRead) {
                _cbs.read = InternalReadCallback;
                _cbs_lfs.read = InternalReadCallback;
            } else {
                _cbs.read = null;
                _cbs_lfs.read = null;
            }

            // Write function
            if (canWrite) {
                _cbs.write = InternalWriteCallback;
                _cbs_lfs.write = InternalWriteCallback;
            } else {
                _cbs.write = null;
                _cbs_lfs.write = null;
            }

            // Seek function
            if (canSeek) {
                _cbs.seek = InternalSeekCallback;
                _cbs_lfs.seek = InternalSeekLfsCallback;
            } else {
                _cbs.seek = null;
                _cbs_lfs.seek = null;
            }

            // Release
            if (canRelease) {
                _cbs.release = InternalReleaseCallback;
                _cbs_lfs.release = InternalReleaseCallback;
            } else {
                _cbs.release = null;
                _cbs_lfs.release = null;
            }

            _pinned_cbs = GCHandle.Alloc(_cbs);
            _pinned_cbs_lfs = GCHandle.Alloc(_cbs_lfs);
            if (libgpgme.use_lfs) {
                _cbs_ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(_cbs_lfs));
                Marshal.StructureToPtr(_cbs_lfs, _cbs_ptr, false);
            } else {
                _cbs_ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(_cbs));
                Marshal.StructureToPtr(_cbs, _cbs_ptr, false);
            }

            int err = libgpgme.gpgme_data_new_from_cbs(
                out dataPtr,
                _cbs_ptr,
                _handle);

#if (VERBOSE_DEBUG)
			DebugOutput("gpgme_data_new_from_cbs(..) DONE.");
#endif
            gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

            if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                return;
            }

            if (errcode == gpg_err_code_t.GPG_ERR_ENOMEM) {
                throw new OutOfMemoryException("Not enough memory available to create user defined GPGME data object.");
            }

            throw new GeneralErrorException("Unknown error " + errcode + " (" + err + ")");
        }

        private IntPtr InternalReadCallback(IntPtr handle, IntPtr buffer, UIntPtr size) {
#if (VERBOSE_DEBUG)
			DebugOutput("_read_cb(..)");
#endif
            if (_handle.Equals(handle)) {
                try {
                    return ReadCB(buffer, (long) size);
                } catch (Exception ex) {
                    LastCallbackException = ex;
                }
            }


            return (IntPtr) ERROR;
        }

        protected virtual IntPtr ReadCB(IntPtr bufPtr, long size) {
            throw new NotSupportedException("The read callback function 'ReadCB' is not implemented.");
        }

        private IntPtr InternalWriteCallback(IntPtr handle, IntPtr buffer, UIntPtr size) {
#if (VERBOSE_DEBUG)
			DebugOutput("_write_cb(..)");
#endif
            if (_handle.Equals(handle)) {
                try {
                    return WriteCB(buffer, (long) size);
                } catch (Exception ex) {
                    LastCallbackException = ex;
                }
            }

            return (IntPtr) ERROR;
        }

        protected virtual IntPtr WriteCB(IntPtr bufPtr, long size) {
            throw new NotSupportedException("The write callback function 'WriteCB' is not implemented.");
        }

        private IntPtr InternalSeekCallback(IntPtr handle, IntPtr offset, int whence) {
#if (VERBOSE_DEBUG)
			DebugOutput("_seek_cb(..)");
#endif
            if (_handle.Equals(handle)) {
                SeekOrigin sorigin = SeekOrigin.Current;
                switch (whence) {
                    case SEEK_SET:
                        sorigin = SeekOrigin.Begin;
                        break;
                    case SEEK_CUR:
                        sorigin = SeekOrigin.Current;
                        break;
                    case SEEK_END:
                        sorigin = SeekOrigin.End;
                        break;
                }
                try {
                    return (IntPtr) SeekCB((long) offset, sorigin);
                } catch (Exception ex) {
                    LastCallbackException = ex;
                }
            }
            return (IntPtr) ERROR;
        }

        // LFS Hack
        private long InternalSeekLfsCallback(IntPtr handle, long offset, int whence) {
#if (VERBOSE_DEBUG)
			DebugOutput("_seek_cb_lfs(..)");
#endif
            if (_handle.Equals(handle)) {
                SeekOrigin sorigin = SeekOrigin.Current;
                switch (whence) {
                    case SEEK_SET:
                        sorigin = SeekOrigin.Begin;
                        break;
                    case SEEK_CUR:
                        sorigin = SeekOrigin.Current;
                        break;
                    case SEEK_END:
                        sorigin = SeekOrigin.End;
                        break;
                }
                try {
                    return SeekCB(offset, sorigin);
                } catch (Exception ex) {
                    LastCallbackException = ex;
                }
            }
            return ERROR;
        }

        protected virtual long SeekCB(long offset, SeekOrigin whence) {
            throw new NotSupportedException("The seek callback function 'SeekCB' is not implemented.");
        }

        private void InternalReleaseCallback(IntPtr handle) {
            if (_handle.Equals(handle)) {
                try {
                    ReleaseCB();
                } catch (Exception ex) {
                    LastCallbackException = ex;
                }

                // cbs structure can be freed in memory now
                _release_cbfunc_init = false;
                _release_cbevent.Set();
            }
        }

        protected virtual void ReleaseCB() {
            throw new NotSupportedException("The release callback function 'ReleaseCB' is not implemented.");
        }
    }
}