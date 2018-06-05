using System;
using System.Globalization;
using Libgpgme;

namespace CreatePgpKey
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("This example will create PGP keys in your default keyring.\n");

            // First step is to create a context
            Context ctx = new Context();

            EngineInfo info = ctx.EngineInfo;
            
            if (info.Protocol != Protocol.OpenPGP)
            {
                ctx.SetEngineInfo(Protocol.OpenPGP, null, null);
                info = ctx.EngineInfo;
            }

            Console.WriteLine("GnuPG home directory: {0}\n"
                + "Version: {1}\n"
                + "Reqversion: {2} \n"
                + "Program: {3}\n",
                info.HomeDir,
                info.Version,
                info.ReqVersion,
                info.FileName);

            IKeyGenerator keygen = ctx.KeyStore;

            // Create 3 PGP keys..
            CreatePgpKeyForAlice(keygen);
            CreatePgpKeyForBob(keygen);
            CreatePgpKeyForMallory(keygen);
        }

        private static void CreatePgpKeyForAlice(IKeyGenerator keygen) {
            KeyParameters aliceparam = new KeyParameters {
                RealName = "Alice",
                Comment = "my comment",
                Email = "alice@home.internal",
                ExpirationDate = DateTime.Now.AddYears(3),
                KeyLength = KeyParameters.KEY_LENGTH_2048,
                // primary key parameters
                PubkeyAlgorithm = KeyAlgorithm.RSA,
                // the primary key algorithm MUST have the "Sign" capability
                PubkeyUsage = AlgorithmCapability.CanSign | AlgorithmCapability.CanAuth | AlgorithmCapability.CanCert,
                // subkey parameters (optional)
                SubkeyLength = KeyParameters.KEY_LENGTH_4096,
                SubkeyAlgorithm = KeyAlgorithm.RSA,
                SubkeyUsage = AlgorithmCapability.CanEncrypt,
                Passphrase = "topsecret"
            };

            Console.WriteLine(
                @"Create a new PGP key for Alice.
Name: {0}
Comment: {1}
Email: {2}
Secret passphrase: {3}
Expire date: {4}
Primary key algorithm = {5} ({6} bit)
Sub key algorithm = {7} ({8} bit)",
                aliceparam.RealName,
                aliceparam.Comment,
                aliceparam.Email,
                aliceparam.Passphrase,
                aliceparam.ExpirationDate.ToString(CultureInfo.InvariantCulture),
                Gpgme.GetPubkeyAlgoName(aliceparam.PubkeyAlgorithm),
                aliceparam.PubkeyLength,
                Gpgme.GetPubkeyAlgoName(aliceparam.SubkeyAlgorithm),
                aliceparam.SubkeyLength);

            Console.Write("Start key generation.. ");
            
            GenkeyResult result = keygen.GenerateKey(
                Protocol.OpenPGP,
                aliceparam);

            Console.WriteLine("done.\nFingerprint: {0}\n",
                result.Fingerprint);
        }

        private static void CreatePgpKeyForBob(IKeyGenerator keygen) {
            Console.Write("Create PGP key for Bob.. ");
            KeyParameters bobparam = new KeyParameters {
                RealName = "Bob",
                Email = "bob@home.internal",
                ExpirationDate = DateTime.Now.AddYears(2),
                Passphrase = "topsecret"
            };

            GenkeyResult result = keygen.GenerateKey(
                Protocol.OpenPGP,
                bobparam);

            Console.WriteLine("done.\nFingerprint: {0}\n",
                result.Fingerprint);
        }

        private static void CreatePgpKeyForMallory(IKeyGenerator keygen) {
            Console.Write("Create PGP key for Mallory.. ");

            KeyParameters malloryparam = new KeyParameters {
                RealName = "Mallory",
                Email = "mallory@home.internal"
            };

            malloryparam.MakeInfinitely(); // PGP key does not expire
            malloryparam.Passphrase = "topsecret";

            GenkeyResult result = keygen.GenerateKey(
                Protocol.OpenPGP,
                malloryparam);
            Console.WriteLine("done.\nFingerprint: {0}",
                result.Fingerprint);
        }
    }
}
