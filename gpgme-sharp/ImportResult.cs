using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class ImportResult
    {
        /* Number of considered keys.  */
        private int _considered;
        private int _imported;
        private int _imported_rsa;
        private ImportStatus _imports;
        private int _new_revocations;
        private int _new_signatures;
        private int _new_sub_keys;
        private int _new_user_ids;

        /* Keys without user ID.  */
        private int _no_user_id;
        private int _not_imported;
        private int _secret_imported;
        private int _secret_read;
        private int _secret_unchanged;
        private int _skipped_new_keys;
        private int _unchanged;

        internal ImportResult(IntPtr resultPtr) {
            if (resultPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid key import result pointer has been given.");
            }

            UpdateFromMem(resultPtr);
        }

        public int Considered {
            get { return _considered; }
        }

        public int NoUserId {
            get { return _no_user_id; }
        }

        /* Imported keys.  */
        public int Imported {
            get { return _imported; }
        }

        /* Imported RSA keys.  */
        public int ImportedRSA {
            get { return _imported_rsa; }
        }

        /* Unchanged keys.  */
        public int Unchanged {
            get { return _unchanged; }
        }

        /* Number of new user ids.  */
        public int NewUserIds {
            get { return _new_user_ids; }
        }

        /* Number of new sub keys.  */
        public int NewSubkeys {
            get { return _new_sub_keys; }
        }

        /* Number of new signatures.  */
        public int NewSignatures {
            get { return _new_signatures; }
        }

        /* Number of new revocations.  */
        public int NewRevocations {
            get { return _new_revocations; }
        }

        /* Number of secret keys read.  */
        public int SecretRead {
            get { return _secret_read; }
        }

        /* Number of secret keys imported.  */
        public int SecretImported {
            get { return _secret_imported; }
        }

        /* Number of secret keys unchanged.  */
        public int SecretUnchanged {
            get { return _secret_unchanged; }
        }

        /* Number of new keys skipped.  */
        public int SkippedNewKeys {
            get { return _skipped_new_keys; }
        }

        /* Number of keys not imported.  */
        public int NotImported {
            get { return _not_imported; }
        }

        public ImportStatus Imports {
            get { return _imports; }
        }

        private void UpdateFromMem(IntPtr resultPtr) {
            var result = new _gpgme_op_import_result();
            Marshal.PtrToStructure(resultPtr, result);

            _considered = result.considered;
            _no_user_id = result.no_user_id;
            _imported = result.imported;
            _imported_rsa = result.imported_rsa;
            _unchanged = result.unchanged;
            _new_user_ids = result.new_user_ids;
            _new_sub_keys = result.new_sub_keys;
            _new_signatures = result.new_signatures;
            _new_revocations = result.new_revocations;
            _secret_read = result.secret_read;
            _secret_imported = result.secret_imported;
            _secret_unchanged = result.secret_unchanged;
            _skipped_new_keys = result.skipped_new_keys;
            _not_imported = result.not_imported;

            if (result.imports != IntPtr.Zero) {
                _imports = new ImportStatus(result.imports);
            }
        }
    }
}