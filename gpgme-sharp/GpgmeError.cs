using System;
using Libgpgme.Interop;

namespace Libgpgme
{
    public static class GpgmeError
    {
        /// <summary>
        /// Throws an exception if <c>err</c> is not <c>GPG_ERR_NO_ERROR</c>.
        /// </summary>
        /// <param name="err">Error code</param>
        public static void Check(int err)
        {
            var code = libgpgme.gpgme_err_code(err);
            if (code != gpg_err_code_t.GPG_ERR_NO_ERROR)
            {
                throw CreateException(code);
            }
        }

        /// <summary>
        /// Converts a GPGME error code into an exception
        /// </summary>
        /// <param name="code">GPGME error code</param>
        /// <returns>Exception</returns>
        public static Exception CreateException(gpg_err_code_t code)
        {
            string message;
            Gpgme.GetStrError((int)code, out message);
            message = $"#{code}: ${message ?? "No error message available"}";

            switch (code)
            {
                case gpg_err_code_t.GPG_ERR_AMBIGUOUS_NAME:
                    return new AmbiguousKeyException(message);
                case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                    return new BadPassphraseException(message);
                case gpg_err_code_t.GPG_ERR_CONFLICT:
                    return new KeyConflictException(message);
                case gpg_err_code_t.GPG_ERR_DECRYPT_FAILED:
                    return new DecryptionFailedException(message);
                case gpg_err_code_t.GPG_ERR_INV_VALUE:
                    return new InvalidPtrException(message);
                case gpg_err_code_t.GPG_ERR_EBADF:
                    return new InvalidDataBufferException(message);
                case gpg_err_code_t.GPG_ERR_ENOMEM:
                    return new OutOfMemoryException(message);
                case gpg_err_code_t.GPG_ERR_NO_DATA:
                    return new NoDataException(message);
                case gpg_err_code_t.GPG_ERR_NO_PUBKEY:
                    return new KeyNotFoundException(message);
                case gpg_err_code_t.GPG_ERR_UNUSABLE_SECKEY:
                    return new InvalidKeyException(message);
                default:
                    return new GeneralErrorException(message);
            }
        }
    }
}
