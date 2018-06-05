using System;
using System.Globalization;
using System.Linq;
using Libgpgme;

namespace ModifyPgpKey
{
    class Program
    {
        static void Main()
        {
            Context ctx = new Context();

            if (ctx.Protocol != Protocol.OpenPGP) {
                ctx.SetEngineInfo(Protocol.OpenPGP, null, null);
            }

            Console.WriteLine("Search Bob's PGP key in the default keyring..");
            
            const string SEARCHSTR = "bob@home.internal";
            IKeyStore keyring = ctx.KeyStore;
            
            // retrieve all keys that have Bob's email address 
            Key[] keys = keyring.GetKeyList(SEARCHSTR, false);
            if (keys == null || keys.Length == 0)
            {
                Console.WriteLine("Cannot find Bob's PGP key {0} in your keyring.", SEARCHSTR);
                Console.WriteLine("You may want to create the PGP key by using the appropriate\n"
                    + "sample in the Samples/ directory.");
                return;
            }

            // print a list of all returned keys
            foreach (Key key in keys) {
                if (key.Uid != null && key.Fingerprint != null) {
                    Console.WriteLine("Found key {0} with fingerprint {1}", key.Uid.Name, key.Fingerprint);
                }
            }

            // we are going to use the first key in the list
            PgpKey bob = (PgpKey)keys.First();
            if (bob.Uid == null || bob.Fingerprint == null) {
                throw new InvalidKeyException();
            }
            Console.WriteLine("\nUsing key {0}", bob.Fingerprint);
            
            // Change Bob's passphrase. This will usually pop-up a pin-entry window!
            ChangePassphrase(ctx, bob);
            
            // Add another PGP sub key to Bob's key.
            AddSubKey(ctx, bob);
            
            // Reload Bobs key (otherwise the new sub key is NOT visible)
            bob = (PgpKey)keyring.GetKey(bob.Fingerprint, false);

            // Display all sub keys
            DisplaySubKeys(bob);

            // Switch owner trust to "Never" an then back to "Ultimate"
            SwitchOwnerTrust(ctx, bob);

            // Disable & Enable Bob's key for usage.
            ToggleEnableDisableState(ctx, bob);

            // Set an expiration date for Bob's key (today + 5 years)
            SetExpirationDate(ctx, bob);
        }

        private static void SetExpirationDate(Context ctx, PgpKey bob) {
            DateTime newdate = DateTime.Now.AddYears(5);
            Console.WriteLine("Set new expire date: {0}", newdate);

            PgpExpirationOptions expopts = new PgpExpirationOptions {
                ExpirationDate = newdate,
                SelectedSubkeys = null // only the primary key
            };

            bob.SetExpirationDate(ctx, expopts);
        }

        private static void ToggleEnableDisableState(Context ctx, PgpKey bob) {
            Console.Write("Disable Bob's key.. ");
            bob.Disable(ctx);
            Console.WriteLine("done.");
            Console.Write("Enable Bob's key.. ");
            bob.Enable(ctx);
            Console.WriteLine("done.");
        }

        private static void SwitchOwnerTrust(Context ctx, PgpKey bob) {
            Console.WriteLine("Set owner trust of Bob's key.");
            Console.Write("\tto never.. ");
            bob.SetOwnerTrust(ctx, PgpOwnerTrust.Never);
            Console.WriteLine("done.");
            Console.Write("\tto ultimate.. ");
            bob.SetOwnerTrust(ctx, PgpOwnerTrust.Ultimate);
            Console.WriteLine("done.");
        }

        private static void DisplaySubKeys(PgpKey bob) {
            Console.WriteLine("Bob has now the following sub keys:");
            int subkeycount = 0;
            foreach (Subkey subkey in bob.Subkeys) {
                subkeycount++;
                Console.WriteLine("{0}\n\tAlgorithm: {1}\n\t"
                    + "Length: {2}\n\t"
                        + "Expires: {3}\n",
                    subkey.Fingerprint,
                    Gpgme.GetPubkeyAlgoName(subkey.PubkeyAlgorithm),
                    subkey.Length.ToString(CultureInfo.InvariantCulture),
                    subkey.Expires.ToString(CultureInfo.InvariantCulture));
            }
            Console.WriteLine("Found {0} sub keys.", subkeycount.ToString(CultureInfo.InvariantCulture));
        }

        private static void ChangePassphrase(Context ctx, PgpKey bob) {
            Console.WriteLine("Change the secret key's password.");

            PgpPassphraseOptions passopts = new PgpPassphraseOptions {
                // We need to specify our own passphrase callback methods
                // in case the user does not use gpg-agent.
                OldPassphraseCallback = StaticOldPassphraseCallback,
                NewPassphraseCallback = StaticNewPassphraseCallback,
                // we do not allow an empty passphrase
                EmptyOkay = false
            };

            bob.ChangePassphrase(ctx, passopts);
        }

        private static void AddSubKey(Context ctx, PgpKey bob) {
            Console.Write("Add a new subkey to Bob's key.. ");

            /* Set the password callback - needed if the user doesn't run
             * gpg-agent or any other password / pin-entry software.
             */
            ctx.SetPassphraseFunction(StaticOldPassphraseCallback);

            PgpSubkeyOptions subopts = new PgpSubkeyOptions {
                /* Same as:
                   subopts.SetAlgorithm(KeyAlgorithm.RSA);
                   subopts.Capability = AlgorithmCapability.CanEncrypt;
                 */
                Algorithm = PgpSubkeyAlgorithm.RSAEncryptOnly,
                KeyLength = PgpSubkeyOptions.KEY_LENGTH_4096,
                ExpirationDate = DateTime.Now.AddDays(90)
            };

            bob.AddSubkey(ctx, subopts);

            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Passphrase callback method. Invoked if a action requires the user's password.
        /// </summary>
        /// <param name="ctx">Context that has invoked the callback.</param>
        /// <param name="info">Information about the key.</param>
        /// <param name="passwd">User supplied password.</param>
        /// <returns></returns>
        public static PassphraseResult StaticOldPassphraseCallback(
               Context ctx,
               PassphraseInfo info,
               ref char[] passwd)
        {
            Console.Write("You need to enter your current passphrase.\n"
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

        /// <summary>
        /// Passphrase callback method. Invoked if a action requires the user's password.
        /// </summary>
        /// <param name="ctx">Context that has invoked the callback.</param>
        /// <param name="info">Information about the key.</param>
        /// <param name="passwd">User supplied password.</param>
        /// <returns></returns>
        public static PassphraseResult StaticNewPassphraseCallback(
               Context ctx,
               PassphraseInfo info,
               ref char[] passwd)
        {
            Console.Write("Please enter your new passphrase.\n"
             + "Uid: " + info.Uid
             + "\nKey id: " + info.UidKeyId
             + "\nNew password: ");

            var read_line = Console.ReadLine();
            
            if (read_line != null) {
                passwd = read_line.ToCharArray();
                return PassphraseResult.Success;
            }

            return PassphraseResult.Canceled;
        }       

    }
 
}
