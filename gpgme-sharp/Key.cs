using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class Key : IDisposable
    {
        // Key attributes
        private gpgme_edit_cb_t _instance_key_edit_callback;
        private IntPtr _key_ptr = IntPtr.Zero;
        
        protected object editlock = new object();

        internal Key(IntPtr keyPtr) {
            if (keyPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid Key pointer has been given." +
                    " Bad programmer! *spank* *spank*");
            }
            UpdateFromMem(keyPtr);
        }

        public Exception LastCallbackException;
        public KeylistMode KeylistMode { get; private set; }
        public bool Revoked { get; private set; }
        public bool Expired { get; private set; }
        public bool Disabled { get; private set; }
        public bool Invalid { get; private set; }
        public bool CanEncrypt { get; private set; }
        public bool CanSign { get; private set; }
        public bool CanCertify { get; private set; }
        public bool CanAuthenticate { get; private set; }
        public bool IsQualified { get; private set; }
        public bool Secret { get; private set; }
        public Protocol Protocol { get; private set; }
        public string IssuerSerial { get; private set; }
        public string IssuerName { get; private set; }
        public string ChainId { get; private set; }
        public Validity OwnerTrust { get; private set; }

        public UserId Uids { get; private set; }
        // The first one in the list is the main/primary UserID
        public UserId Uid { get { return Uids; } }

        public Subkey Subkeys { get; private set; }
        // The first subkey in the linked list is the primary key
        public string KeyId {
            get {
                if (Subkeys != null) {
                    return Subkeys.KeyId;
                }
                return null;
            }
        }
        // The first subkey in the linked list is the primary key
        public string Fingerprint {
            get {
                return Subkeys != null 
                    ? Subkeys.Fingerprint 
                    : null;
            }
        }
        internal virtual IntPtr KeyPtr {
            get { return _key_ptr; }
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
        
        ~Key() {
            CleanUp();
        }

        private void CleanUp() {
            if (_key_ptr != IntPtr.Zero) {
                libgpgme.gpgme_key_release(_key_ptr);
                _key_ptr = IntPtr.Zero;
            }
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                CleanUp();
            }
        }

        internal void UpdateFromMem(IntPtr keyPtr) {
            var key = (_gpgme_key)Marshal.PtrToStructure(keyPtr, typeof(_gpgme_key));

            _key_ptr = keyPtr;

            Revoked = key.revoked;
            Expired = key.expired;
            Disabled = key.disabled;
            Invalid = key.invalid;
            CanEncrypt = key.can_encrypt;
            CanSign = key.can_sign;
            CanCertify = key.can_certify;
            CanAuthenticate = key.can_authenticate;
            IsQualified = key.is_qualified;
            Secret = key.secret;

            Protocol = (Protocol) key.protocol;
            OwnerTrust = (Validity) key.owner_trust;
            KeylistMode = (KeylistMode) key.keylist_mode;

            IssuerName = Gpgme.PtrToStringUTF8(key.issuer_name);
            IssuerSerial = Gpgme.PtrToStringAnsi(key.issuer_serial);
            ChainId = Gpgme.PtrToStringAnsi(key.chain_id);

            if (key.subkeys != IntPtr.Zero) {
                Subkeys = new Subkey(key.subkeys);
            }

            if (key.uids != IntPtr.Zero) {
                Uids = new UserId(key.uids);
            }
        }

        protected int StartEdit(Context ctx, IntPtr handle, GpgmeData data) {
            if (KeyPtr == IntPtr.Zero) {
                throw new InvalidKeyException();
            }

            if (ctx == null || !(ctx.IsValid)) {
                throw new InvalidContextException();
            }

            if (data == null || !(data.IsValid)) {
                throw new InvalidDataBufferException();
            }

            lock (editlock) {
                LastCallbackException = null;

                // set the instance's _edit_cb() method as callback function for libgpgme
                _instance_key_edit_callback = KeyEditCallback;

                // start key editing
                int err = libgpgme.gpgme_op_edit(
                    ctx.CtxPtr,
                    KeyPtr,
                    _instance_key_edit_callback,
                    handle,
                    data.dataPtr);

                GC.KeepAlive(_instance_key_edit_callback);

                if (LastCallbackException != null) {
                    throw LastCallbackException;
                }

                return err;
            }
        }

        // internal callback function 
        private int KeyEditCallback(
            IntPtr opaque,
            int status,
            IntPtr args,
            int fd) 
        {
            var statuscode = (gpgme_status_code_t) status;
            string cmdargs = Gpgme.PtrToStringUTF8(args);

            int result = 0;
            try {
                // call user callback function.
                result = KeyEditCallback(
                    opaque,
                    (KeyEditStatusCode) statuscode,
                    cmdargs,
                    fd);
            } catch (Exception ex) {
                LastCallbackException = ex;
            }

            return result;
        }

        protected virtual int KeyEditCallback(IntPtr handle, KeyEditStatusCode status, string args, int fd) {
            libgpgme.gpgme_io_write(fd, new[] {(byte) 'q', (byte) 'u', (byte) 'i', (byte) 't', (byte) '\n'}, (UIntPtr)5);
            throw new NotImplementedException("The function KeyEditCallback is not implemented.");
        }
    }
}