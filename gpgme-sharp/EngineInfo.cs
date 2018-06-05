using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class EngineInfo : IEnumerable<EngineInfo>
    {
        private readonly Context _ctx;

        private string _filename;
        private string _homedir;
        private EngineInfo _next;
        private Protocol _protocol;
        private string _reqversion;
        private string _version;

        internal EngineInfo(Context ctx, IntPtr enginePtr) {
            _ctx = ctx;

            if (enginePtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid EngineInfo pointer has been given." +
                    " Bad programmer! *spank* *spank*");
            }

            UpdateFromMem(enginePtr);
        }

        internal EngineInfo(IntPtr enginePtr)
            : this(null, enginePtr) {
        }

        public Protocol Protocol {
            get {
                if (CtxValid || (!HasCtx)) {
                    return _protocol;
                }
                throw new InvalidContextException();
            }
            set {
                if (!CtxValid) {
                    throw new InvalidContextException();
                }

                _ctx.SetEngineInfo(value, _filename, _homedir);
                _protocol = value;
            }
        }

        public string HomeDir {
            get {
                if (!CtxValid && (HasCtx)) {
                    throw new InvalidContextException();
                }
                return _homedir;
            }
            set {
                if (!CtxValid) {
                    throw new InvalidContextException();
                }
                _ctx.SetEngineInfo(_protocol, _filename, value);
                _homedir = value;
            }
        }

        public string FileName {
            get {
                if (!CtxValid && (HasCtx)) {
                    throw new InvalidContextException();
                }
                return _filename;
            }
            set {
                if (!CtxValid) {
                    throw new InvalidContextException();
                }
                _ctx.SetEngineInfo(_protocol, value, _homedir);
                _filename = value;
            }
        }

        public string Version {
            get {
                if (!CtxValid && (HasCtx)) {
                    throw new InvalidContextException();
                }
                return _version;
            }
        }

        public string ReqVersion {
            get {
                if (!CtxValid && (HasCtx)) {
                    throw new InvalidContextException();
                }
                return _reqversion;
            }
        }

        public bool CtxValid {
            // If the context invalides the engine object(s) invailde(s) as well.
            get {
                return HasCtx && _ctx.IsValid;
            }
        }

        public bool HasCtx {
            get { return (_ctx != null); }
        }

        public EngineInfo Next {
            get {
                if (!CtxValid && (HasCtx)) {
                    throw new InvalidContextException();
                }
                return _next;
            }
        }

        #region IEnumerable<EngineInfo> Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<EngineInfo> GetEnumerator() {
            EngineInfo info = this;
            while (info != null) {
                yield return info;
                info = info.Next;
            }
        }

        #endregion

        internal void UpdateFromMem(IntPtr enginePtr) {
            var engine = (_gpgme_engine_info)
                Marshal.PtrToStructure(enginePtr,
                    typeof(_gpgme_engine_info));

            _protocol = (Protocol) engine.protocol;

            _homedir = engine.home_dir != IntPtr.Zero 
                ? PtrToStringAnsi(engine.home_dir) 
                : null;

            _filename = engine.file_name != IntPtr.Zero 
                ? PtrToStringAnsi(engine.file_name) 
                : null;

            _version = engine.version != IntPtr.Zero 
                ? PtrToStringAnsi(engine.version) 
                : null;

            _reqversion = engine.req_version != IntPtr.Zero 
                ? PtrToStringAnsi(engine.req_version) 
                : null;

            _next = engine.next != IntPtr.Zero 
                ? new EngineInfo(_ctx, engine.next) 
                : null;
        }

        private string PtrToStringAnsi(IntPtr ptr) {
            string tmp = Marshal.PtrToStringAnsi(ptr);
            return (tmp !=null) ? tmp.Replace("\r", "") : null;
        }
    }
}