using System;
using System.IO;
using Libgpgme;

namespace ExportKey
{
    class Program
    {
        static void Main(string[] args)
        {
            const string keyToExport = "A3E1D41E632142737E3EDC340E6B9269F9F982C1!";

            using (var ctx = new Context())
            {
                ctx.Armor = true;
                ctx.KeyStore.Export(keyToExport, "exported.pub");
                Console.WriteLine($"Exported public key {keyToExport} to exported.pub");

                ctx.KeyStore.Export(keyToExport, "exported.gpg", ExportMode.Secret);
                Console.WriteLine($"Exported private key {keyToExport} (if available) to exported.gpg");
            }
        }
    }
}
