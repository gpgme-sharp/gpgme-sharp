using System;
using System.Collections.Generic;
using System.Globalization;
using Libgpgme;

namespace SignPgpKey
{
    class Program
    {
        static void Main()
        {
            Context ctx = new Context();

            if (ctx.Protocol != Protocol.OpenPGP)
                ctx.SetEngineInfo(Protocol.OpenPGP, null, null);

            Console.WriteLine("Search Bob's and Alice's PGP keys in the default keyring..");

            String[] searchpattern = new[] {
                "bob@home.internal",
                "alice@home.internal" };

            IKeyStore keyring = ctx.KeyStore;

            /* Enable the listing of signatures. By default
             * key signatures are NOT passed.
             */
            ctx.KeylistMode = KeylistMode.Signatures;

            // retrieve all keys that have Bob's or Alice's email address
            Key[] keys = keyring.GetKeyList(searchpattern, false);

            PgpKey bob = null, alice = null;
            if (keys != null && keys.Length != 0)
            {
                foreach (Key k in keys)
                {
                    if (k.Uid == null) {
                        throw new InvalidKeyException();
                    }

                    if (bob == null && k.Uid.Email.ToLower().Equals("bob@home.internal")) {
                        bob = (PgpKey) k;
                    }
                    if (alice == null && k.Uid.Email.ToLower().Equals("alice@home.internal")) {
                        alice = (PgpKey) k;
                    }
                }
            }

            if (bob == null || alice == null)
            {
                Console.WriteLine("Cannot find Bob's or Alice's PGP key in your keyring.");
                Console.WriteLine("You may want to create the PGP key by using the appropriate\n"
                    + "sample in the Samples/ directory.");
                return;
            }

            // Print out all Uids from Bob's key
            PrintUidData(bob);

            // Print out all Uids from Alice's key
            PrintUidData(alice);


            Console.WriteLine("Set Alice's PGP key as signer key.");
            // Clear signer list (remove default key)
            ctx.Signers.Clear();
            // Add Alice's key as signer
            ctx.Signers.Add(alice);

            /* Set the password callback - needed if the user doesn't run
             * gpg-agent or any other password / pin-entry software.
             */
            ctx.SetPassphraseFunction(MyPassphraseCallback);

            Console.WriteLine("Sign Bob's PGP key with Alice's key.. ");

            /////// SIGN KEY ///////

            PgpSignatureOptions signopts = new PgpSignatureOptions {
                SelectedUids = new[] {1},
                TrustLevel = PgpSignatureTrustLevel.Full,
                Type = PgpSignatureType.Trust | PgpSignatureType.NonExportable
            };

            // sign the latest Uid only!

            try
            {
                bob.Sign(ctx, signopts);
            }
            catch (AlreadySignedException)
            {
                Console.WriteLine("Bob's key is already signed!");
            }

            // Refresh Bob's key 
            bob = (PgpKey)keyring.GetKey(bob.Fingerprint, false);

            PrintUidData(bob);

            /////// REVOKE SIGNATURE ///////

            Console.WriteLine("Revoke the signature..");

            // We need to find Alice's signature first
            int nsignature = 0;
            foreach (KeySignature keysig in bob.Uid.Signatures)
            {
                if (!keysig.Revoked)
                    nsignature++; // do not count revocation certificates

                if (keysig.KeyId.Equals(alice.KeyId) && 
                    !keysig.Revoked) // must not be a revocation certificate
                    break; // found!
            }

            PgpRevokeSignatureOptions revopts = new PgpRevokeSignatureOptions {
                SelectedUid = 1, 
                SelectedSignatures = new[] { nsignature }, 
                ReasonText = "Test revocation"
            };
            // latest uid

            bob.RevokeSignature(ctx, revopts);

            // Refresh Bob's key
            bob = (PgpKey)keyring.GetKey(bob.Fingerprint, false);

            PrintUidData(bob);

            /////// DELETE SIGNATURE ///////

            Console.WriteLine("Remove Alice's signature and revocation certificate(s)..");

            List<int> siglst = new List<int>();
            nsignature = 0;
            foreach (KeySignature keysig in bob.Uid.Signatures)
            {
                nsignature++;
                if (keysig.KeyId.Equals(alice.KeyId))
                    siglst.Add(nsignature);
            }

            PgpDeleteSignatureOptions delsigopts = new PgpDeleteSignatureOptions {
                DeleteSelfSignature = false, 
                SelectedUid = 1, 
                SelectedSignatures = siglst.ToArray()
            };

            bob.DeleteSignature(ctx, delsigopts);

            // Refresh Bob's key
            bob = (PgpKey)keyring.GetKey(bob.Fingerprint, false);

            PrintUidData(bob);
        }

        private static void PrintUidData(Key key)
        {
            if (key.Uid == null)
                throw new InvalidKeyException();
            
            Console.WriteLine("{0}'s key {1}\nhas the following Uids and signatures",
                key.Uid.Name,
                key.Fingerprint);
            foreach (UserId id in key.Uids)
            {
                Console.WriteLine("\tReal name: {0}\n\t"
                    + "Email: {1}\n\t"
                    + "Comment: {2}\n\t"
                    + "Invalid: {3}\n\t"
                    + "Revoked: {4}\n\t"
                    + "Validity: {5}\n\t",
                    id.Name,
                    id.Email,
                    id.Comment,
                    id.Invalid.ToString(CultureInfo.InvariantCulture),
                    id.Revoked.ToString(CultureInfo.InvariantCulture),
                    id.Validity);

                Console.WriteLine("\tSignatures:");
                if (id.Signatures != null)
                {
                    foreach (KeySignature keysig in id.Signatures)
                        Console.WriteLine("\t\tFrom: {0}\n\t\t"
                            + "Key id: {1}\n\t\t"
                            + "Date: {2}\n\t\t"
                            + "Revoked: {3}\n\t\t"
                            + "Expires: {4}\n\t\t"
                            + "Invalid: {5}\n",
                            keysig.Name,
                            keysig.KeyId,
                            keysig.Timestamp.ToString(CultureInfo.InvariantCulture),
                            keysig.Revoked.ToString(CultureInfo.InvariantCulture),
                            keysig.Expires.ToString(CultureInfo.InvariantCulture),
                            keysig.Invalid.ToString(CultureInfo.InvariantCulture));
                }
                else
                    Console.WriteLine("\t\tNone");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Passphrase callback method. Invoked if a action requires the user's password.
        /// </summary>
        /// <param name="ctx">Context that has invoked the callback.</param>
        /// <param name="info">Information about the key.</param>
        /// <param name="passwd">User supplied password.</param>
        /// <returns></returns>
        public static PassphraseResult MyPassphraseCallback(
               Context ctx,
               PassphraseInfo info,
               ref char[] passwd)
        {
            Console.Write("You need to enter your passphrase.\n"
             + "Uid: " + info.Uid
             + "\nKey id: " + info.UidKeyId
             + "\nPrevious passphrase was bad: " + info.PrevWasBad
             + "\nPassword: ");

            var read_line = Console.ReadLine();
            if (read_line != null) {
                passwd = read_line.ToCharArray();
                return PassphraseResult.Success;
            }

            return PassphraseResult.Canceled;
        }
    }
}
