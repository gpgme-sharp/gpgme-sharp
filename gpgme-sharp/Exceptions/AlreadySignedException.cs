namespace Libgpgme
{
    public class AlreadySignedException : GpgmeException
    {
        public string KeyId;

        public AlreadySignedException(string keyid) {
            KeyId = keyid;
        }
    }
}