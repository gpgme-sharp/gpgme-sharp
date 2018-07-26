using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class Context : IDisposable
    {
        public const int CERTIFICATES_NO = 0;
        public const int CERTIFICATES_ALL = -1;
        public const int CERTIFICATES_ALL_EXCEPT_ROOT = -2;
        public const int CERTIFICATES_SENDER_ONLY = 1;
        public const int CERTIFICATES_DEFAULT = -256;

        private IntPtr _ctx_ptr = IntPtr.Zero;
        private readonly object _ctx_lock = new object();

        private gpgme_passphrase_cb_t _passphrase_callback;
        private PassphraseDelegate _passphrase_delegate;

        private readonly KeyStore _keystore;
        public Exception _last_callback_exception;

        static Context() {
            // We need to call gpgme_check_version at least once!
            try {
                Gpgme.CheckVersion();
            } catch(Exception exception) {
                // Do not throw in type constructor!
                Debug.Print(exception.Message);
            }
        }

        /// <summary>
        /// Creates a new Context and sets the DllDirectory to the provided 
        /// location
        /// This directory will be used to find to libgpgme-11.dll in case all previous 
        /// tries to find the dll have failed
        /// (Windows has some kind of steps it does per default to discover the dll, for example search at
        /// the default installation directory of gnupg you can find the order here
        /// https://docs.microsoft.com/en-us/windows/desktop/Dlls/dynamic-link-library-search-order) 
        /// </summary>
        /// <param name="dllPath">Path to the Folder containg the libgpgme-11.dll</param>
        public Context(string dllDirectory)
        {
            if (String.IsNullOrEmpty(dllDirectory)) throw new ArgumentException("The provided Path to dll Directory is empty or null!");

            //Sets the Dll Directory (Method will take care about other issues like non existing path etc.) 
            SetDllDirectory(dllDirectory);

            Initialize(ref _signers, ref _signots, ref _keystore);
        }

        public Context()
        {
            Initialize(ref _signers, ref _signots, ref _keystore); 
        }

        /// <summary>
        /// Initializes the Context 
        /// </summary>
        private void Initialize(ref ContextSigners signers, ref ContextSignatureNotations signots, ref KeyStore keystore )
        {
            IntPtr ptr;

            var err = libgpgme.gpgme_new(out ptr);
            gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

            switch (errcode)
            {
                case gpg_err_code_t.GPG_ERR_NO_ERROR:
                    _ctx_ptr = ptr;
                    signers = new ContextSigners(this);
                    signots = new ContextSignatureNotations(this);
                    keystore = new KeyStore(this);
                    break;
                case gpg_err_code_t.GPG_ERR_INV_VALUE:
                    throw new InvalidPtrException("CTX is not a valid pointer.\nBad programmer *spank* *spank*");
                case gpg_err_code_t.GPG_ERR_ENOMEM:
                    throw new OutOfMemoryException();
                default:
                    throw new GeneralErrorException("An unexpected error occurred during context creation. " +
                        errcode.ToString());
            }
        }

        ~Context() {
            Dispose();
        }

        public void Dispose() {
            if (_ctx_ptr != IntPtr.Zero) {
                libgpgme.gpgme_release(_ctx_ptr);
                _ctx_ptr = IntPtr.Zero;
            }
        }

        public bool IsValid {
            get { return (_ctx_ptr != IntPtr.Zero); }
        }
        internal IntPtr CtxPtr {
            get { return _ctx_ptr; }
        }
        internal object CtxLock {
            get { return _ctx_lock; }
        }

        public Protocol Protocol {
            get {
                if (IsValid) {
                    lock (CtxLock) {
                        gpgme_protocol_t proto = libgpgme.gpgme_get_protocol(CtxPtr);
                        return (Protocol) proto;
                    }
                }
                throw new InvalidContextException();
            }
            set {
                if (IsValid) {
                    lock (CtxLock) {
                        var proto = (gpgme_protocol_t) value;
                        int err = libgpgme.gpgme_set_protocol(CtxPtr, proto);

                        gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);
                        switch (errcode) {
                            case gpg_err_code_t.GPG_ERR_NO_ERROR:
                                //UpdateFromMem();
                                break;
                            case gpg_err_code_t.GPG_ERR_INV_VALUE:
                                throw new InvalidProtocolException("The protocol "
                                    + value.ToString()
                                        + " is not supported by any installed GnuPG engine.");
                        }
                    }
                } else {
                    throw new InvalidContextException();
                }
            }
        }
        public EngineInfo EngineInfo {
            get {
                if (IsValid) {
                    lock (CtxLock) {
                        IntPtr engine_ptr = libgpgme.gpgme_ctx_get_engine_info(CtxPtr);
                        if (engine_ptr != IntPtr.Zero) {
                            return new EngineInfo(this, engine_ptr);
                        }
                        return null;
                    }
                }
                throw new InvalidContextException();
            }
        }

        public void SetEngineInfo(Protocol proto, string filename, string homedir) {
            if (IsValid) {
                lock (CtxLock) {
                    IntPtr filename_ptr = IntPtr.Zero, homedir_ptr = IntPtr.Zero;

                    if (filename != null) {
                        filename_ptr = Marshal.StringToCoTaskMemAnsi(filename);
                    }
                    if (homedir != null) {
                        homedir_ptr = Marshal.StringToCoTaskMemAnsi(homedir);
                    }

                    int err = libgpgme.gpgme_ctx_set_engine_info(
                        CtxPtr,
                        (gpgme_protocol_t) proto,
                        filename_ptr,
                        homedir_ptr);

                    if (filename_ptr != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(filename_ptr);
                    }
                    if (homedir_ptr != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(homedir_ptr);
                    }

                    gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);
                    if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                        string errmsg;
                        try {
                            Gpgme.GetStrError(err, out errmsg);
                        } catch {
                            errmsg = "No error message available.";
                        }
                        throw new ArgumentException(errmsg + " Error: " + err.ToString(CultureInfo.InvariantCulture));
                    }
                }
            } else {
                throw new InvalidContextException();
            }
        }

        public bool Armor {
            get {
                if (IsValid) {
                    lock (CtxLock) {
                        int yes = libgpgme.gpgme_get_armor(CtxPtr);
                        if (yes > 0) {
                            return true;
                        }
                        return false;
                    }
                }
                throw new InvalidContextException();
            }
            set {
                if (IsValid) {
                    lock (CtxLock) {
                        int yes = 0;
                        if (value) {
                            yes = 1;
                        }
                        libgpgme.gpgme_set_armor(CtxPtr, yes);
                    }
                } else {
                    throw new InvalidContextException();
                }
            }
        }

        public bool TextMode {
            get {
                if (IsValid) {
                    lock (CtxLock) {
                        int yes = libgpgme.gpgme_get_textmode(CtxPtr);
                        if (yes > 0) {
                            return true;
                        }
                        return false;
                    }
                }
                throw new InvalidContextException();
            }
            set {
                if (IsValid) {
                    lock (CtxLock) {
                        int yes = 0;
                        if (value) {
                            yes = 1;
                        }
                        libgpgme.gpgme_set_textmode(CtxPtr, yes);
                    }
                } else {
                    throw new InvalidContextException();
                }
            }
        }

        public int IncludedCerts {
            get {
                if (IsValid) {
                    lock (CtxLock) {
                        return libgpgme.gpgme_get_include_certs(CtxPtr);
                    }
                }
                throw new InvalidContextException();
            }
            set {
                if (IsValid) {
                    lock (CtxLock) {
                        libgpgme.gpgme_set_include_certs(CtxPtr, value);
                    }
                } else {
                    throw new InvalidContextException();
                }
            }
        }

        public void AddKeylistMode(KeylistMode mode) {
            KeylistMode |= mode;
        }

        public void RemoveKeylistMode(KeylistMode mode) {
            KeylistMode tmp = mode;

            // Notations can only be retrieved with attached signatures
            if ((tmp & KeylistMode.Signatures) == KeylistMode.Signatures) {
                tmp |= KeylistMode.SignatureNotations;
            }

            KeylistMode &= ~(tmp);
        }

        public KeylistMode KeylistMode {
            get {
                if (IsValid) {
                    lock (CtxLock) {
                        return (KeylistMode) libgpgme.gpgme_get_keylist_mode(CtxPtr);
                    }
                }
                throw new InvalidContextException();
            }
            set {
                if (IsValid) {
                    lock (CtxLock) {
                        if ((value & KeylistMode.SignatureNotations)
                            == KeylistMode.SignatureNotations) {
                            value |= KeylistMode.Signatures;
                        }

                        var mode = (gpgme_keylist_mode_t) value;

                        int err = libgpgme.gpgme_set_keylist_mode(CtxPtr, mode);
                        gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

                        if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                            string errmsg;
                            try {
                                Gpgme.GetStrError(err, out errmsg);
                            } catch {
                                errmsg = "Unknown error.";
                            }
                            throw new ArgumentException(errmsg + " Error: " + err.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                } else {
                    throw new InvalidContextException();
                }
            }
        }

        private int PassphraseCb(IntPtr hook, IntPtr uid_hint, IntPtr passphrase_info,
            int prev_was_bad, int fd) {
            char[] passwd = null;

            string hint = Gpgme.PtrToStringUTF8(uid_hint);
            string info = Gpgme.PtrToStringUTF8(passphrase_info);
            
            bool prevbad = prev_was_bad > 0;

            PassphraseResult result;

            try {
                var pinfo = new PassphraseInfo(hook, hint, info, prevbad);
                result = _passphrase_delegate(this, pinfo, ref passwd);
            } catch (Exception ex) {
                _last_callback_exception = ex;
                passwd = "".ToCharArray();
                result = PassphraseResult.Canceled;
            }

            if (fd > 0) {
                byte[] utf8_passwd = Gpgme.ConvertCharArrayToUTF8(passwd, 0);

                libgpgme.gpgme_io_write(fd, utf8_passwd, (UIntPtr)utf8_passwd.Length);
                libgpgme.gpgme_io_write(fd, new[] { (byte)0 }, (UIntPtr)1);

                // try to wipe the passwords
                int i;
                for (i = 0; i < utf8_passwd.Length; i++) {
                    utf8_passwd[i] = 0;
                }
            }

            return (int) result;
        }

        public void SetPassphraseFunction(PassphraseDelegate func) {
            SetPassphraseFunction(func, IntPtr.Zero);
        }

        public void SetPassphraseFunction(PassphraseDelegate func, IntPtr hook) {
            if (IsValid) {
                lock (CtxLock) {
                    if (_passphrase_delegate == null) {
                        _passphrase_callback = PassphraseCb;
                        libgpgme.gpgme_set_passphrase_cb(CtxPtr, _passphrase_callback, hook);

                        _passphrase_delegate = func;
                    } else {
                        throw new GpgmeException("Passphrase function is already set.");
                    }
                }
            } else {
                throw new InvalidContextException();
            }
        }

        public bool HasPassphraseFunction {
            get { return (_passphrase_delegate != null); }
        }

        public void ClearPassphraseFunction() {
            if (IsValid) {
                lock (CtxLock) {
                    if (_passphrase_delegate != null) {
                        libgpgme.gpgme_set_passphrase_cb(CtxPtr, null, IntPtr.Zero);
                        _passphrase_delegate = null;
                        _passphrase_callback = null;
                    }
                }
            } else {
                throw new InvalidContextException();
            }
        }

        public KeyStore KeyStore {
            get { return _keystore; }
        }

        public EncryptionResult Encrypt(Key[] recipients, EncryptFlags flags, GpgmeData plain, GpgmeData cipher) {
            if (plain == null) {
                throw new ArgumentNullException("plain", "Source data buffer must be supplied.");
            }
            if (!(plain.IsValid)) {
                throw new InvalidDataBufferException("The specified source data buffer is invalid.");
            }
            if (cipher == null) {
                throw new ArgumentNullException("cipher", "Destination data buffer must be supplied.");
            }
            if (!(cipher.IsValid)) {
                throw new InvalidDataBufferException("The specified destination data buffer is invalid.");
            }
            if (!IsValid) {
                throw new InvalidContextException();
            }

            IntPtr[] recp = Gpgme.KeyArrayToIntPtrArray(recipients);

            lock (CtxLock) {

#if (VERBOSE_DEBUG)
					DebugOutput("gpgme_op_encrypt(..) START");
#endif
                int err = libgpgme.gpgme_op_encrypt(
                    CtxPtr,
                    recp,
                    (gpgme_encrypt_flags_t) flags,
                    plain.dataPtr,
                    cipher.dataPtr);

#if (VERBOSE_DEBUG)
					DebugOutput("gpgme_op_encrypt(..) DONE");
#endif

                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                switch (errcode) {
                    case gpg_err_code_t.GPG_ERR_NO_ERROR:
                        break;
                    case gpg_err_code_t.GPG_ERR_UNUSABLE_PUBKEY:
                        break;
                    case gpg_err_code_t.GPG_ERR_GENERAL: // Bug? should be GPG_ERR_UNUSABLE_PUBKEY
                        break;
                    case gpg_err_code_t.GPG_ERR_INV_VALUE:
                        throw new InvalidPtrException(
                            "Either the context, recipient key array, plain text or cipher text pointer is invalid.");
                    case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                        throw new BadPassphraseException();
                    case gpg_err_code_t.GPG_ERR_EBADF:
                        throw new InvalidDataBufferException(
                            "The source (plain) or destination (cipher) data buffer is invalid for encryption.");
                    default:
                        throw new GeneralErrorException("An unexpected error "
                            + errcode.ToString()
                                + " (" + err.ToString(CultureInfo.InvariantCulture)
                                    + ") occurred.");
                }
                IntPtr rst_ptr = libgpgme.gpgme_op_encrypt_result(CtxPtr);
#if (VERBOSE_DEBUG)
					DebugOutput("gpgme_op_encrypt_result(..) DONE");
#endif
                GC.KeepAlive(recp);
                GC.KeepAlive(recipients);
                GC.KeepAlive(plain);
                GC.KeepAlive(cipher);

                if (rst_ptr != IntPtr.Zero) {
                    var enc_rst = new EncryptionResult(rst_ptr);
                    return enc_rst;
                }
                throw new GeneralErrorException("An unexpected error occurred. " + errcode.ToString());
            }
        }

        public EncryptionResult EncryptAndSign(Key[] recipients, EncryptFlags flags, GpgmeData plain, GpgmeData cipher) {
            if (IsValid) {
                if (plain == null) {
                    throw new ArgumentNullException("plain", "Source data buffer must be supplied.");
                }
                if (!(plain.IsValid)) {
                    throw new InvalidDataBufferException("The specified source data buffer is invalid.");
                }

                if (cipher == null) {
                    throw new ArgumentNullException("cipher", "Destination data buffer must be supplied.");
                }
                if (!(cipher.IsValid)) {
                    throw new InvalidDataBufferException("The specified destination data buffer is invalid.");
                }

                IntPtr[] recp = Gpgme.KeyArrayToIntPtrArray(recipients);

                lock (CtxLock) {
                    int err = libgpgme.gpgme_op_encrypt_sign(
                        CtxPtr,
                        recp,
                        (gpgme_encrypt_flags_t) flags,
                        plain.dataPtr,
                        cipher.dataPtr);

                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_UNUSABLE_PUBKEY:
                            break;
                        case gpg_err_code_t.GPG_ERR_GENERAL: // Bug? should be GPG_ERR_UNUSABLE_PUBKEY
                            break;
                        case gpg_err_code_t.GPG_ERR_UNUSABLE_SECKEY:
                            throw new InvalidKeyException(
                                "There is one or more invalid signing key(s) in the current context.");
                        case gpg_err_code_t.GPG_ERR_INV_VALUE:
                            throw new InvalidPtrException(
                                "Either the context, recipient key array, plain text or cipher text pointer is invalid.");
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException();
                        default:
                            throw new GeneralErrorException("An unexpected error "
                                + errcode.ToString()
                                    + " (" + err.ToString(CultureInfo.InvariantCulture)
                                        + ") occurred.");
                    }
                    IntPtr rst_ptr = libgpgme.gpgme_op_encrypt_result(CtxPtr);

                    GC.KeepAlive(recp);
                    GC.KeepAlive(recipients);
                    GC.KeepAlive(plain);
                    GC.KeepAlive(cipher);

                    if (rst_ptr != IntPtr.Zero) {
                        var enc_rst = new EncryptionResult(rst_ptr);
                        return enc_rst;
                    }
                    throw new GeneralErrorException("An unexpected error occurred. " + errcode.ToString());
                }
            }
            throw new InvalidContextException();
        }

        public SignatureResult Sign(GpgmeData plain, GpgmeData sig, SignatureMode mode) {
            if (IsValid) {
                if (plain == null) {
                    throw new ArgumentNullException("plain", "Source data buffer must be supplied.");
                }
                if (!(plain.IsValid)) {
                    throw new InvalidDataBufferException("The specified source data buffer is invalid.");
                }

                if (sig == null) {
                    throw new ArgumentNullException("sig", "Destination data buffer must be supplied.");
                }
                if (!(sig.IsValid)) {
                    throw new InvalidDataBufferException("The specified destination data buffer is invalid.");
                }

                lock (CtxLock) {
                    int err = libgpgme.gpgme_op_sign(
                        CtxPtr,
                        plain.dataPtr,
                        sig.dataPtr,
                        (gpgme_sig_mode_t) mode);

                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_UNUSABLE_SECKEY:
                            throw new InvalidKeyException(
                                "There is one or more invalid signing key(s) in the current context.");
                        case gpg_err_code_t.GPG_ERR_INV_VALUE:
                            throw new InvalidPtrException(
                                "Either the context, plain text or cipher text pointer is invalid.");
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException();
                        default:
                            throw new GeneralErrorException("An unexpected error "
                                + errcode.ToString()
                                    + " (" + err.ToString(CultureInfo.InvariantCulture)
                                        + ") occurred.");
                    }
                    IntPtr rst_ptr = libgpgme.gpgme_op_sign_result(CtxPtr);
                    if (rst_ptr != IntPtr.Zero) {
                        var sig_rst = new SignatureResult(rst_ptr);
                        return sig_rst;
                    }
                    throw new GeneralErrorException("An unexpected error occurred. " + errcode.ToString());
                }
            }
            throw new InvalidContextException();
        }

        public DecryptionResult Decrypt(GpgmeData cipher, GpgmeData plain) {
            if (IsValid) {
                if (cipher == null) {
                    throw new ArgumentNullException("cipher", "Source data buffer must be supplied.");
                }
                if (!(cipher.IsValid)) {
                    throw new InvalidDataBufferException("The specified source data buffer is invalid.");
                }

                if (plain == null) {
                    throw new ArgumentNullException("plain", "Destination data buffer must be supplied.");
                }
                if (!(plain.IsValid)) {
                    throw new InvalidDataBufferException("The specified destination data buffer is invalid.");
                }

                lock (CtxLock) {
#if (VERBOSE_DEBUG)
                    DebugOutput("gpgme_op_decrypt(..) START");
#endif
                    int err = libgpgme.gpgme_op_decrypt(
                        CtxPtr,
                        cipher.dataPtr,
                        plain.dataPtr);
#if (VERBOSE_DEBUG)
                    DebugOutput("gpgme_op_decrypt(..) DONE");
#endif
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_NO_DATA:
                            throw new NoDataException("The cipher does not contain any data to decrypt or is corrupt.");
                        case gpg_err_code_t.GPG_ERR_INV_VALUE:
                            throw new InvalidPtrException(
                                "Either the context, cipher text or plain text pointer is invalid.");
                    }

                    DecryptionResult dec_rst = null;

                    IntPtr rst_ptr = libgpgme.gpgme_op_decrypt_result(CtxPtr);
                    if (rst_ptr != IntPtr.Zero) {
                        dec_rst = new DecryptionResult(rst_ptr);
                    }

                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_DECRYPT_FAILED:
                            if (dec_rst == null) {
                                throw new DecryptionFailedException("An invalid cipher text has been supplied.");
                            }
                            throw new DecryptionFailedException(dec_rst);
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException(dec_rst);
                        default:
                            throw new GeneralErrorException("An unexpected error occurred. " + errcode.ToString());
                    }

                    GC.KeepAlive(cipher);
                    GC.KeepAlive(plain);

                    return dec_rst;
                }
            }
            throw new InvalidContextException();
        }

        public CombinedResult DecryptAndVerify(GpgmeData cipher, GpgmeData plain) {
            if (IsValid) {
                if (cipher == null) {
                    throw new ArgumentNullException("cipher", "Source data buffer must be supplied.");
                }
                if (!(cipher.IsValid)) {
                    throw new InvalidDataBufferException("The specified source data buffer is invalid.");
                }

                if (plain == null) {
                    throw new ArgumentNullException("plain", "Destination data buffer must be supplied.");
                }
                if (!(plain.IsValid)) {
                    throw new InvalidDataBufferException("The specified destination data buffer is invalid.");
                }

                lock (CtxLock) {
#if (VERBOSE_DEBUG)
                    DebugOutput("gpgme_op_decrypt_verify(..) START");
#endif
                    int err = libgpgme.gpgme_op_decrypt_verify(
                        CtxPtr,
                        cipher.dataPtr,
                        plain.dataPtr);
#if (VERBOSE_DEBUG)
                    DebugOutput("gpgme_op_decrypt_verify(..) DONE");
#endif
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_NO_DATA:
                            // no encrypted data found - maybe it is only signed.
                            break;
                        case gpg_err_code_t.GPG_ERR_INV_VALUE:
                            throw new InvalidPtrException(
                                "Either the context, cipher text or plain text pointer is invalid.");
                    }

                    DecryptionResult dec_rst = null;

                    IntPtr rst_ptr = libgpgme.gpgme_op_decrypt_result(CtxPtr);
                    if (rst_ptr != IntPtr.Zero) {
                        dec_rst = new DecryptionResult(rst_ptr);
                    }

                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;

                        case gpg_err_code_t.GPG_ERR_DECRYPT_FAILED:
                            if (dec_rst == null) {
                                throw new DecryptionFailedException("An invalid cipher text has been supplied.");
                            }
                            throw new DecryptionFailedException(dec_rst);

                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException(dec_rst);

                        default:
                            throw new GeneralErrorException("An unexpected error occurred. "
                                + errcode.ToString());
                    }

                    /* If decryption failed, verification cannot be proceeded */
                    VerificationResult ver_rst = null;

                    rst_ptr = libgpgme.gpgme_op_verify_result(CtxPtr);
                    if (rst_ptr != IntPtr.Zero) {
                        ver_rst = new VerificationResult(rst_ptr);
                    }

                    GC.KeepAlive(cipher);
                    GC.KeepAlive(plain);

                    return new CombinedResult(dec_rst, ver_rst);
                }
            }
            throw new InvalidContextException();
        }

        /// <summary>
        /// Sets the GNUPG directory where the libgpgme-11.dll can be found.
        /// </summary>
        /// <param name="path">Path to libgpgme-11.dll.</param>
        public void SetDllDirectory(string path) {
            if (Environment.OSVersion.Platform.ToString().Contains("Win32") ||
                Environment.OSVersion.Platform.ToString().Contains("Win64")) {
                if (!string.IsNullOrEmpty(path)) {
                    string tmp;
                    if (path[path.Length - 1] != '\\') {
                        tmp = path + "\\";
                    } else {
                        tmp = path;
                    }

                    if (!File.Exists(tmp + libgpgme.GNUPG_LIBNAME)) {
                        throw new FileNotFoundException("Could not find GPGME DLL file.", tmp + libgpgme.GNUPG_LIBNAME);
                    }

                    if (!libgpgme.SetDllDirectory(path)) {
                        throw new GpgmeException("Could not set DLL path " + path);
                    }

                    libgpgme.InitLibgpgme();
                }
            }
        }

        public VerificationResult Verify(GpgmeData signature, GpgmeData signedtext, GpgmeData plain) {
            if (!IsValid) {
                throw new InvalidContextException();
            }
            if (signature == null) {
                throw new ArgumentNullException("signature", "A signature data buffer must be supplied.");
            }
            if (!(signature.IsValid)) {
                throw new InvalidDataBufferException("The specified signature data buffer is invalid.");
            }

            if (
                (signedtext == null || !(signedtext.IsValid))
                    && (plain == null || !(plain.IsValid))
                ) {
                throw new InvalidDataBufferException(
                    "Either the signed text must be provided in order to prove the detached signature, or an empty data buffer to store the plain text result.");
            }
            lock (CtxLock) {
                IntPtr sigtxt_ptr = IntPtr.Zero, plain_ptr = IntPtr.Zero;

                IntPtr sig_ptr = signature.dataPtr;
                if (signedtext != null && signedtext.IsValid) {
                    sigtxt_ptr = signedtext.dataPtr;
                }
                if (plain != null && plain.IsValid) {
                    plain_ptr = plain.dataPtr;
                }

                int err = libgpgme.gpgme_op_verify(
                    CtxPtr,
                    sig_ptr,
                    sigtxt_ptr,
                    plain_ptr);

                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                switch (errcode) {
                    case gpg_err_code_t.GPG_ERR_NO_ERROR:
                        break;
                    case gpg_err_code_t.GPG_ERR_NO_DATA:
                        break;
                    case gpg_err_code_t.GPG_ERR_INV_VALUE:
                        throw new InvalidDataBufferException(
                            "Either the signature, signed text or plain text data buffer is invalid.");
                    default:
                        throw new GeneralErrorException("Unexpected error occurred. Error: " + errcode.ToString());
                }

                IntPtr rst_ptr = libgpgme.gpgme_op_verify_result(CtxPtr);
                if (rst_ptr != IntPtr.Zero) {
                    VerificationResult ver_rst = new VerificationResult(rst_ptr);
                    if (errcode == gpg_err_code_t.GPG_ERR_NO_DATA) {
                        throw new NoDataException("The signature does not contain any data to verify.");
                    }
                    return ver_rst;
                }
                throw new GeneralErrorException("Could not retrieve verification result.");
            }
        }


        private readonly ContextSigners _signers;
        public ContextSigners Signers {
            get { return _signers; }
        }

#if (VERBOSE_DEBUG)
		private void DebugOutput(string text)
        {
            Console.WriteLine("Debug: " + text);
			Console.Out.Flush();
        }
#endif

        public class ContextSigners : IEnumerable<Key>
        {
            private readonly Context _ctx;

            internal ContextSigners(Context ctx) {
                _ctx = ctx;
            }
            #region IEnumerable<Key> Members

            public IEnumerator<Key> GetEnumerator() {
                int i = 0;
                Key key;

                while ((key = Enum(i++)) != null) {
                    yield return key;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            #endregion
            public void Add(Key signer) {
                if (signer == null) {
                    throw new ArgumentNullException("signer", "A signer key must be supplied.");
                }
                if (signer.KeyPtr.Equals(IntPtr.Zero)) {
                    throw new InvalidKeyException("An invalid signer key has been supplied.");
                }
                if (!_ctx.IsValid) {
                    throw new InvalidContextException();
                }

                lock (_ctx.CtxLock) {
                    int err = libgpgme.gpgme_signers_add(_ctx.CtxPtr, signer.KeyPtr);
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                    if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                        throw new GeneralErrorException("An unexpected error occurred. Error: " + errcode.ToString());
                    }
                }
            }

            public void Clear() {
                if (!_ctx.IsValid) {
                    throw new InvalidContextException();
                }

                lock (_ctx.CtxLock) {
                    libgpgme.gpgme_signers_clear(_ctx.CtxPtr);
                }
            }

            public Key Enum(int seq) {
                if (!_ctx.IsValid) {
                    throw new InvalidContextException();
                }

                lock (_ctx.CtxLock) // could be locked twice by Get()
                {
                    IntPtr rst_ptr = libgpgme.gpgme_signers_enum(_ctx.CtxPtr, seq);

                    if (rst_ptr.Equals(IntPtr.Zero)) {
                        return null;
                    }

                    return new Key(rst_ptr);
                }
            }

            public Key[] Get() {
                if (!_ctx.IsValid) {
                    throw new InvalidContextException();
                }

                lock (_ctx.CtxLock) {
                    var lst = new List<Key>();
                    int i = 0;
                    Key key;
                    while ((key = Enum(i++)) != null) {
                        lst.Add(key);
                    }

                    return (lst.Count == 0)
                        ? new Key[0]
                        : lst.ToArray();
                }
            }
        }

        private readonly ContextSignatureNotations _signots;
        public ContextSignatureNotations SignatureNotations {
            get { return _signots; }
        }

        public class ContextSignatureNotations : IEnumerable<SignatureNotation>
        {
            private readonly Context _ctx;

            internal ContextSignatureNotations(Context ctx) {
                _ctx = ctx;
            }
            #region IEnumerable<SignatureNotation> Members

            public IEnumerator<SignatureNotation> GetEnumerator() {
                return Get().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            #endregion
            public void Add(string name, string value, SignatureNotationFlags flags) {
                if (!_ctx.IsValid) {
                    throw new InvalidContextException();
                }

                IntPtr name_ptr = IntPtr.Zero;
                IntPtr value_ptr = IntPtr.Zero;
                if (name != null) {
                    name_ptr = Gpgme.StringToCoTaskMemUTF8(name);
                }
                if (value != null) {
                    value_ptr = Gpgme.StringToCoTaskMemUTF8(value);
                }

                int err = libgpgme.gpgme_sig_notation_add(
                    _ctx.CtxPtr,
                    name_ptr,
                    value_ptr,
                    (gpgme_sig_notation_flags_t) flags);

                if (name_ptr != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(name_ptr);
                }
                if (value_ptr != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(value_ptr);
                }

                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                    return;
                }
                if (errcode == gpg_err_code_t.GPG_ERR_INV_VALUE) {
                    throw new ArgumentException("NAME and VALUE are in an invalid combination.");
                }

                throw new GeneralErrorException("An unexpected error occurred. Error: "
                    + errcode.ToString());
            }

            public void Clear() {
                if (_ctx.IsValid) {
                    libgpgme.gpgme_sig_notation_clear(_ctx.CtxPtr);
                } else {
                    throw new InvalidContextException();
                }
            }

            public SignatureNotation Get() {
                if (!_ctx.IsValid) {
                    throw new InvalidContextException();
                }
                IntPtr rst_ptr = libgpgme.gpgme_sig_notation_get(_ctx.CtxPtr);
                if (rst_ptr.Equals(IntPtr.Zero)) {
                    return null;
                }
                return new SignatureNotation(rst_ptr);
            }
        }
    }
}