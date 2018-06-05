using System;

namespace Libgpgme
{
    public delegate PassphraseResult PassphraseDelegate(
        Context ctx,
        PassphraseInfo info,
        ref char[] passphrase);

    internal delegate int KeyEditDelegate(
        IntPtr handle,
        KeyEditStatusCode status,
        string args,
        int fd);
}