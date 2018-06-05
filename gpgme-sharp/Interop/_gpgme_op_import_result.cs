using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_op_import_result //gpgme_import_result_t
    {
        /* Number of considered keys.  */
        public int considered;

        /* Keys without user ID.  */
        public int no_user_id;

        /* Imported keys.  */
        public int imported;

        /* Imported RSA keys.  */
        public int imported_rsa;

        /* Unchanged keys.  */
        public int unchanged;

        /* Number of new user ids.  */
        public int new_user_ids;

        /* Number of new sub keys.  */
        public int new_sub_keys;

        /* Number of new signatures.  */
        public int new_signatures;

        /* Number of new revocations.  */
        public int new_revocations;

        /* Number of secret keys read.  */
        public int secret_read;

        /* Number of secret keys imported.  */
        public int secret_imported;

        /* Number of secret keys unchanged.  */
        public int secret_unchanged;

        /* Number of new keys skipped.  */
        public int skipped_new_keys;

        /* Number of keys not imported.  */
        public int not_imported;

        /* List of keys for which an import was attempted.  */
        public IntPtr imports; //gpgme_import_status_t
    }
}