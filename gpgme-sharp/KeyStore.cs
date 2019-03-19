using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class KeyStore : IKeyStore, IKeyGenerator
    {
        public KeyStore(Context ctx) {
            Context = ctx;
        }

        public Context Context { get; private set; }

        #region IKeyGenerator Members

        public GenkeyResult GenerateKey(Protocol protocoltype, KeyParameters keyparms) {
            if (!Context.IsValid) {
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

            lock (Context.CtxLock) {
                // Protocol specific key generation
                switch (protocoltype) {
                    case Protocol.OpenPGP:
                        err = libgpgme.NativeMethods.gpgme_op_genkey(Context.CtxPtr, parms_ptr, IntPtr.Zero, IntPtr.Zero);
                        errcode = libgpgme.gpgme_err_code(err);
                        if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                            IntPtr result_ptr = libgpgme.NativeMethods.gpgme_op_genkey_result(Context.CtxPtr);
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
            if (Context == null
                || !(Context.IsValid)) {
                throw new InvalidContextException();
            }

            if (keydata == null) {
                throw new ArgumentNullException("An invalid data buffer has been specified.",
                    new InvalidDataBufferException());
            }

            if (!(keydata.IsValid)) {
                throw new InvalidDataBufferException();
            }

            lock (Context.CtxLock) {
                GpgmeError.Check(libgpgme.NativeMethods.gpgme_op_import(Context.CtxPtr, keydata.dataPtr));
                IntPtr result = libgpgme.NativeMethods.gpgme_op_import_result(Context.CtxPtr);
                return new ImportResult(result);
            }
        }

        public void Export(string pattern, GpgmeData keydata, ExportMode? mode = null) {
            Export(new[] {pattern}, keydata, mode);
        }

        public void Export(string[] pattern, GpgmeData keydata, ExportMode? mode = null) {
            if (Context == null ||
                !(Context.IsValid)) {
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
            var apiMode = mode == null ? 0 : (uint) mode;

            lock (Context.CtxLock) {
                if (parray != null) {
                    err = libgpgme.NativeMethods.gpgme_op_export_ext(
                        Context.CtxPtr,
                        parray,
                        apiMode,
                        keydata.dataPtr);
                } else {
                    err = libgpgme.NativeMethods.gpgme_op_export(
                        Context.CtxPtr,
                        IntPtr.Zero,
                        apiMode,
                        keydata.dataPtr);
                }
            }

            GC.KeepAlive(keydata);

            // Free memory 
            Gpgme.FreeStringArray(parray);

            GpgmeError.Check(err);
        }

        public void Export(string pattern, string filename, ExportMode? mode = null) {
            using (var keyfile = new GpgmeFileData(filename, FileMode.Create, FileAccess.ReadWrite)) {
                Export(pattern, keyfile, mode);
            }
        }

        public void Export(string[] pattern, string filename, ExportMode? mode = null) {
            using (var keyfile = new GpgmeFileData(filename, FileMode.Create, FileAccess.ReadWrite)) {
                Export(pattern, keyfile, mode);
            }
        }

        public Key GetKey(string fpr, bool secretOnly) {
            if (Context == null ||
                !(Context.IsValid)) {
                throw new InvalidContextException();
            }

            if (fpr == null || fpr.Equals(string.Empty)) {
                throw new InvalidKeyFprException();
            }

            int secret = secretOnly ? 1 : 0;

            lock (Context.CtxLock) {
                // no deadlock because the query is made by the same thread
                Protocol proto = Context.Protocol;

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
            if (Context == null ||
                !(Context.IsValid)) {
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

            lock (Context.CtxLock) {
                // no deadlock because the query is made by the same thread
                Protocol proto = Context.Protocol;

                int err;

                if (parray != null) {
                    err = libgpgme.NativeMethods.gpgme_op_keylist_ext_start(
                        Context.CtxPtr,
                        parray,
                        secret_only,
                        RESERVED_FLAG);
                } else {
                    err = libgpgme.NativeMethods.gpgme_op_keylist_start(
                        Context.CtxPtr,
                        IntPtr.Zero,
                        secret_only);
                }

                while (err == 0) {
                    IntPtr key_ptr;
                    err = libgpgme.NativeMethods.gpgme_op_keylist_next(Context.CtxPtr, out key_ptr);
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
                    libgpgme.NativeMethods.gpgme_op_keylist_end(Context.CtxPtr);
                    throw new GpgmeException(Gpgme.GetStrError(err), err);
                }
            }
            return list.ToArray();
        }

        public void DeleteKey(Key key, bool deleteSecret) {
            if (Context == null ||
                !(Context.IsValid)) {
                throw new InvalidContextException();
            }

            if (key == null || key.KeyPtr.Equals(IntPtr.Zero)) {
                throw new InvalidKeyException("An invalid key has been supplied.");
            }

            int secret = deleteSecret ? 1 : 0;

            lock (Context.CtxLock) {
                int err = libgpgme.NativeMethods.gpgme_op_delete(Context.CtxPtr, key.KeyPtr, secret);

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

            int err = libgpgme.NativeMethods.gpgme_get_key(Context.CtxPtr, fpr_ptr, out rkeyPtr, secret);

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
            if (Context == null ||
                !(Context.IsValid)) {
                throw new InvalidContextException();
            }

            if (pattern == null || pattern.Equals(String.Empty)) {
                throw new ArgumentException("An invalid pattern has been specified.");
            }

            IntPtr pattern_ptr = Gpgme.StringToCoTaskMemUTF8(pattern);

            var lst = new List<TrustItem>();

            lock (Context.CtxLock) {
                int err = libgpgme.NativeMethods.gpgme_op_trustlist_start(Context.CtxPtr, pattern_ptr, maxlevel);
                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                    if (pattern_ptr != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(pattern_ptr);
                    }
                    throw GpgmeError.CreateException(errcode);
                }

                while (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                    IntPtr item_ptr;
                    err = libgpgme.NativeMethods.gpgme_op_trustlist_next(Context.CtxPtr, out item_ptr);
                    errcode = libgpgerror.gpg_err_code(err);

                    if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                        lst.Add(new TrustItem(item_ptr));
                    }
                }
                // Release context if there are any pending trustlist items
                libgpgme.NativeMethods.gpgme_op_trustlist_end(Context.CtxPtr);

                if (pattern_ptr != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(pattern_ptr);
                }

                if (errcode != gpg_err_code_t.GPG_ERR_EOF) {
                    throw GpgmeError.CreateException(errcode);
                }
            }

            if (lst.Count == 0) {
                return new TrustItem[0];
            }
            return lst.ToArray();
        }
    }
}