namespace Libgpgme.Interop
{
    internal partial class libgpgerror
    {
        internal static int gpg_err_make(gpg_err_source_t source, gpg_err_code_t code) {
            return code == gpg_err_code_t.GPG_ERR_NO_ERROR
                ? (int) gpg_err_code_t.GPG_ERR_NO_ERROR
                : ((((int) source & (int) Masks.GPG_ERR_SOURCE_MASK) << (int) Masks.GPG_ERR_SOURCE_SHIFT)
                    | ((int) code & (int) Masks.GPG_ERR_CODE_MASK));
        }

        internal static gpg_err_code_t gpg_err_code(int err) {
            return (gpg_err_code_t) (err & (int) Masks.GPG_ERR_CODE_MASK);
        }

        internal static gpg_err_source_t gpg_err_source(int err) {
            return (gpg_err_source_t) ((err >> (int) Masks.GPG_ERR_SOURCE_SHIFT)
                & (int) Masks.GPG_ERR_SOURCE_MASK);
        }
    }
}