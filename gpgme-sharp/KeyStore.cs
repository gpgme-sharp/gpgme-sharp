using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class KeyStore : IKeyStore, IKeyGenerator
    {
        private readonly Context _ctx;

        public KeyStore(Context ctx) {
            _ctx = ctx;
        }

        public Context Context {
            get { return _ctx; }
        }
        #region IKeyGenerator Members

        public GenkeyResult GenerateKey(Protocol protocoltype, KeyParameters keyparms) {
            if (!_ctx.IsValid) {
                throw new InvalidContextException();
            }

            if (keyparms == null) {
                throw new ArgumentNullException("keyparms", "No KeyParameters object supplied. Bad programmer! *spank* *spank*");
            }

            if (keyparms.Email == null ||
                keyparms.Email.Equals(String.Empty)) {
                throw new ArgumentException("No email address has been supplied.");
            }

            // Convert the key parameter to an XML string for GPGME
            string parms = keyparms.GetXmlText(protocoltype);

            // Convert key parameter XML string to UTF8 and retrieve the memory pointer
            IntPtr parms_ptr = Gpgme.StringToCoTaskMemUTF8(parms);

            GenkeyResult keyresult = null;
            int err;
            gpg_err_code_t errcode;

            lock (_ctx.CtxLock) {
                // Protocol specific key generation
                switch (protocoltype) {
                    case Protocol.OpenPGP:
                        err = libgpgme.gpgme_op_genkey(_ctx.CtxPtr, parms_ptr, IntPtr.Zero, IntPtr.Zero);
                        errcode = libgpgme.gpgme_err_code(err);
                        if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                            IntPtr result_ptr = libgpgme.gpgme_op_genkey_result(_ctx.CtxPtr);
                            if (!result_ptr.Equals(IntPtr.Zero)) {
                                keyresult = new GenkeyResult(result_ptr);
                            } else {
                                errcode = gpg_err_code_t.GPG_ERR_GENERAL;
                            }
                        }
                        break;
                    default:
                        // free memory
                        if (parms_ptr != IntPtr.Zero) {
                            Marshal.FreeCoTaskMem(parms_ptr);
                        }
                        throw new NotSupportedException("The protocol " + protocoltype +
                            " is currently not supported for key generation.");
                }
            }

            // free memory
            if (parms_ptr != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(parms_ptr);
            }

            if (errcode == gpg_err_code_t.GPG_ERR_INV_VALUE) {
                throw new ArgumentException("The key parameters are invalid.");
            }
            if (errcode == gpg_err_code_t.GPG_ERR_NOT_SUPPORTED) {
                throw new NotSupportedException("The PUBLIC or SECRET part is invalid. Error: " + err.ToString(CultureInfo.InvariantCulture));
            }
            if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                throw new GeneralErrorException("No key has been created by the backend.");
            }

            return keyresult;
        }

        #endregion
        #region IKeyStore Members

        public ImportResult Import(GpgmeData keydata) {
            if (_ctx == null
                || !(_ctx.IsValid)) {
                throw new InvalidContextException();
            }

            if (keydata == null) {
                throw new ArgumentNullException("An invalid data buffer has been specified.",
                    new InvalidDataBufferException());
            }

            if (!(keydata.IsValid)) {
                throw new InvalidDataBufferException();
            }

            lock (_ctx.CtxLock) {
                int err = libgpgme.gpgme_op_import(_ctx.CtxPtr, keydata.dataPtr);
                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                    throw new KeyImportException("Error " + errcode, err);
                }

                IntPtr result = libgpgme.gpgme_op_import_result(_ctx.CtxPtr);
                return new ImportResult(result);
            }
        }

        public void Export(string pattern, GpgmeData keydata) {
            Export(new[] {pattern}, keydata);
        }

        public void Export(string[] pattern, GpgmeData keydata) {
            if (_ctx == null ||
                !(_ctx.IsValid)) {
                throw new InvalidContextException();
            }

            if (keydata == null) {
                throw new ArgumentNullException("Invalid data buffer",
                    new InvalidDataBufferException());
            }

            if (!(keydata.IsValid)) {
                throw new InvalidDataBufferException();
            }

            IntPtr[] parray = null;
            if (pattern != null) {
                parray = Gpgme.StringToCoTaskMemUTF8(pattern);
            }

            int err;
            const uint RESERVED_FLAG = 0;

            lock (_ctx.CtxLock) {
                if (parray != null) {
                    err = libgpgme.gpgme_op_export_ext(
                        _ctx.CtxPtr,
                        parray,
                        RESERVED_FLAG,
                        keydata.dataPtr);
                } else {
                    err = libgpgme.gpgme_op_export(
                        _ctx.CtxPtr,
                        IntPtr.Zero,
                        RESERVED_FLAG,
                        keydata.dataPtr);
                }
            }

            GC.KeepAlive(keydata);

            // Free memory 
            Gpgme.FreeStringArray(parray);

            gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

            if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                throw new KeyExportException("Error " + errcode, err);
            }
        }

        public Key GetKey(string fpr, bool secretOnly) {
            if (_ctx == null ||
                !(_ctx.IsValid)) {
                throw new InvalidContextException();
            }

            if (fpr == null || fpr.Equals(string.Empty)) {
                throw new InvalidKeyFprException();
            }

            int secret = secretOnly ? 1 : 0;

            lock (_ctx.CtxLock) {
                // no deadlock because the query is made by the same thread
                Protocol proto = _ctx.Protocol;

                IntPtr rkey_ptr;
                gpg_err_code_t errcode = GetKey(fpr, secret, out rkey_ptr);

                if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR &&
                    !(rkey_ptr.Equals(IntPtr.Zero))) {
                    Key key;

                    if (proto == Protocol.OpenPGP) {
                        key = new PgpKey(rkey_ptr);
                    } else if (proto == Protocol.CMS) {
                        key = new X509Key(rkey_ptr);
                    } else {
                        key = new Key(rkey_ptr);
                    }

                    //libgpgme.gpgme_key_release(rkeyPtr);
                    return key;
                }
                throw new KeyNotFoundException("The key " + fpr + " could not be found in the keyring.");
            }
        }

        public Key[] GetKeyList(string pattern, bool secretOnly) {
            return GetKeyList(new[] {pattern}, secretOnly);
        }

        public Key[] GetKeyList(string[] pattern, bool secretOnly) {
            if (_ctx == null ||
                !(_ctx.IsValid)) {
                throw new InvalidContextException();
            }

            var list = new List<Key>();

            const int RESERVED_FLAG = 0;
            int secret_only = 0;
            if (secretOnly) {
                secret_only = 1;
            }

            IntPtr[] parray = null;
            if (pattern != null) {
                parray = Gpgme.StringToCoTaskMemUTF8(pattern);
            }

            lock (_ctx.CtxLock) {
                // no deadlock because the query is made by the same thread
                Protocol proto = _ctx.Protocol;

                int err;

                if (parray != null) {
                    err = libgpgme.gpgme_op_keylist_ext_start(
                        _ctx.CtxPtr,
                        parray,
                        secret_only,
                        RESERVED_FLAG);
                } else {
                    err = libgpgme.gpgme_op_keylist_start(
                        _ctx.CtxPtr,
                        IntPtr.Zero,
                        secret_only);
                }

                while (err == 0) {
                    IntPtr key_ptr;
                    err = libgpgme.gpgme_op_keylist_next(_ctx.CtxPtr, out key_ptr);
                    if (err != 0) {
                        break;
                    }

                    Key key;

                    if (proto == Protocol.OpenPGP) {
                        key = new PgpKey(key_ptr);
                    } else if (proto == Protocol.CMS) {
                        key = new X509Key(key_ptr);
                    } else {
                        key = new Key(key_ptr);
                    }

                    list.Add(key);

                    //libgpgme.gpgme_key_release(keyPtr);
                }

                // Free memory 
                if (parray != null) {
                    Gpgme.FreeStringArray(parray);
                }

                gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);
                if (errcode != gpg_err_code_t.GPG_ERR_EOF) {
                    libgpgme.gpgme_op_keylist_end(_ctx.CtxPtr);
                    throw new GpgmeException(Gpgme.GetStrError(err), err);
                }
            }
            return list.ToArray();
        }

        public void DeleteKey(Key key, bool deleteSecret) {
            if (_ctx == null ||
                !(_ctx.IsValid)) {
                throw new InvalidContextException();
            }

            if (key == null || key.KeyPtr.Equals(IntPtr.Zero)) {
                throw new InvalidKeyException("An invalid key has been supplied.");
            }

            int secret = deleteSecret ? 1 : 0;

            lock (_ctx.CtxLock) {
                int err = libgpgme.gpgme_op_delete(_ctx.CtxPtr, key.KeyPtr, secret);

                gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

                switch (errcode) {
                    case gpg_err_code_t.GPG_ERR_NO_ERROR:
                        break;
                    case gpg_err_code_t.GPG_ERR_NO_PUBKEY:
                        throw new KeyNotFoundException("The public key could not be found.");
                    case gpg_err_code_t.GPG_ERR_CONFLICT:
                        throw new KeyConflictException(
                            "Cannot delete the public key without deleting the secret key as well.");
                    case gpg_err_code_t.GPG_ERR_INV_VALUE:
                        throw new InvalidPtrException("Either the context (ctx) or the key parameter was invalid.");
                    case gpg_err_code_t.GPG_ERR_AMBIGUOUS_NAME:
                        throw new AmbiguousKeyException("The key id was not unique.");
                }
            }
        }

        #endregion
        private gpg_err_code_t GetKey(string fpr, int secret, out IntPtr rkeyPtr) {
            // the fingerprint could be a UTF8 encoded name
            IntPtr fpr_ptr = Gpgme.StringToCoTaskMemUTF8(fpr);

            int err = libgpgme.gpgme_get_key(_ctx.CtxPtr, fpr_ptr, out rkeyPtr, secret);

            // free memory
            if (fpr_ptr != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(fpr_ptr);
            }

            gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

            switch (errcode) {
                case gpg_err_code_t.GPG_ERR_INV_VALUE:
                    throw new ArgumentException("Invalid key fingerprint has been given. Error: " + err.ToString(CultureInfo.InvariantCulture));
                case gpg_err_code_t.GPG_ERR_AMBIGUOUS_NAME:
                    throw new AmbiguousKeyException("The key id was not unique. Error: " + err.ToString(CultureInfo.InvariantCulture));
                case gpg_err_code_t.GPG_ERR_ENOMEM:
                    throw new OutOfMemoryException("Not enough memory available for this operation.");
                case gpg_err_code_t.GPG_ERR_EOF:
                    throw new KeyNotFoundException("The key " + fpr + " (secret=" + secret +
                        ") could not be found in the keyring.");
            }
            return errcode;
        }

        public TrustItem[] GetTrustList(string pattern, int maxlevel) {
            if (_ctx == null ||
                !(_ctx.IsValid)) {
                throw new InvalidContextException();
            }

            if (pattern == null || pattern.Equals(String.Empty)) {
                throw new ArgumentException("An invalid pattern has been specified.");
            }

            IntPtr pattern_ptr = Gpgme.StringToCoTaskMemUTF8(pattern);

            var lst = new List<TrustItem>();

            lock (_ctx.CtxLock) {
                int err = libgpgme.gpgme_op_trustlist_start(_ctx.CtxPtr, pattern_ptr, maxlevel);
                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                    if (pattern_ptr != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(pattern_ptr);
                    }
                    throw new GeneralErrorException("An unexpected error occurred. Error: " + err.ToString(CultureInfo.InvariantCulture));
                }

                while (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                    IntPtr item_ptr;
                    err = libgpgme.gpgme_op_trustlist_next(_ctx.CtxPtr, out item_ptr);
                    errcode = libgpgerror.gpg_err_code(err);

                    if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                        lst.Add(new TrustItem(item_ptr));
                    }
                }
                // Release context if there are any pending trustlist items
                libgpgme.gpgme_op_trustlist_end(_ctx.CtxPtr);

                if (pattern_ptr != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(pattern_ptr);
                }

                if (errcode != gpg_err_code_t.GPG_ERR_EOF) {
                    throw new GeneralErrorException("An unexpected error occurred. " + errcode.ToString());
                }
            }

            if (lst.Count == 0) {
                return new TrustItem[0];
            }
            return lst.ToArray();
        }
    }
}