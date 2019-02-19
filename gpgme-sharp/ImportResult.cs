using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class ImportResult
    {
        private int _imported_rsa;
        private int _new_user_ids;
        private int _secret_unchanged;
        private int _unchanged;

        internal ImportResult(IntPtr resultPtr) {
            if (resultPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid key import result pointer has been given.");
            }

            UpdateFromMem(resultPtr);
        }

        public int Considered { get; private set; }

        public int NoUserId { get; private set; }

        /* Imported keys.  */
        public int Imported { get; private set; }

        /* Imported RSA keys.  */
        public int ImportedRSA => _imported_rsa;

        /* Unchanged keys.  */
        public int Unchanged => _unchanged;

        /* Number of new user ids.  */
        public int NewUserIds => _new_user_ids;

        /* Number of new sub keys.  */
        public int NewSubkeys { get; private set; }

        /* Number of new signatures.  */
        public int NewSignatures { get; private set; }

        /* Number of new revocations.  */
        public int NewRevocations { get; private set; }

        /* Number of secret keys read.  */
        public int SecretRead { get; private set; }

        /* Number of secret keys imported.  */
        public int SecretImported { get; private set; }

        /* Number of secret keys unchanged.  */
        public int SecretUnchanged => _secret_unchanged;

        /* Number of new keys skipped.  */
        public int SkippedNewKeys { get; private set; }

        /* Number of keys not imported.  */
        public int NotImported { get; private set; }

        public ImportStatus Imports { get; private set; }

        private void UpdateFromMem(IntPtr resultPtr) {
            var result = new _gpgme_op_import_result();
            Marshal.PtrToStructure(resultPtr, result);

            Considered = result.considered;
            NoUserId = result.no_user_id;
            Imported = result.imported;
            _imported_rsa = result.imported_rsa;
            _unchanged = result.unchanged;
            _new_user_ids = result.new_user_ids;
            NewSubkeys = result.new_sub_keys;
            NewSignatures = result.new_signatures;
            NewRevocations = result.new_revocations;
            SecretRead = result.secret_read;
            SecretImported = result.secret_imported;
            _secret_unchanged = result.secret_unchanged;
            SkippedNewKeys = result.skipped_new_keys;
            NotImported = result.not_imported;

            if (result.imports != IntPtr.Zero) {
                Imports = new ImportStatus(result.imports);
            }
        }
    }
}