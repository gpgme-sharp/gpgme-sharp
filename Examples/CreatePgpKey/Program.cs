/*
 * gpgme-sharp - .NET wrapper classes for libgpgme (GnuPG Made Easy)
 *  Copyright (C) 2009 Daniel Mueller <daniel@danm.de>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

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
