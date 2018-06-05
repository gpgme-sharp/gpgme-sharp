using System;

namespace Libgpgme
{
    public class GpgmeException : Exception
    {
        public int GPGMEError;

        public GpgmeException() {
        }

        public GpgmeException(String message)
            : base(message) {
        }

        public GpgmeException(String message, int GPGMEError)
            : base(message) {
            this.GPGMEError = GPGMEError;
        }
    }
}