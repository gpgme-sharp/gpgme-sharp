using System;
using System.Globalization;
using System.Text;
using System.IO;

using Libgpgme;

namespace PgpEncryptDecrypt
{
    class Program
    {
        static void Main()
        {
            Context ctx = new Context();
            ctx.PinentryMode = PinentryMode.Loopback;

            if (ctx.Protocol != Protocol.OpenPGP)
                ctx.SetEngineInfo(Protocol.OpenPGP, null, null);

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
            foreach (Key key in keys)
                if (key.Uid != null
                    && key.Fingerprint != null)
                    Console.WriteLine("Found key {0} with fingerprint {1}",
                        key.Uid.Name,
                        key.Fingerprint);

            // we are going to use the first key in the list
            PgpKey bob = (PgpKey)keys[0];
            if (bob.Uid == null || bob.Fingerprint == null)
                throw new InvalidKeyException();

            // Create a sample string
            StringBuilder randomtext = new StringBuilder();
            for (int i = 0; i < 80 * 6; i++)
                randomtext.Append((char)(34 + i%221));
            string secrettext = new string('+', 508)
                + " Die Gedanken sind frei "
                + new string('+', 508)
                + randomtext;

            Console.WriteLine("Text to be encrypted:\n\n{0}", secrettext);

            // we want our string UTF8 encoded.
            UTF8Encoding utf8 = new UTF8Encoding();

            // create a (dynamic) memory based data buffer to place the unencrypted (plain) text
            GpgmeData plain = new GpgmeMemoryData();

            // set a filename for this data buffer
            plain.FileName = "my_document.txt";

            BinaryWriter binwriter = new BinaryWriter(plain, utf8);
            
            // write our secret text to the memory buffer
            binwriter.Write(secrettext.ToCharArray());
            binwriter.Flush();
            // go to the beginning(!)
            binwriter.Seek(0, SeekOrigin.Begin);

            /////// ENCRYPT DATA ///////

            // we want or PGP encrypted data RADIX/BASE64 encoded.
            ctx.Armor = true;

            // create another (dynamic) memory based data buffer as destination
            GpgmeData cipher = new GpgmeMemoryData();
            cipher.FileName = "my_document.txt";

            Console.Write("Encrypt data for {0} ({1}).. ", 
                bob.Uid.Name, bob.KeyId);

            ctx.Encrypt(
                new Key[] { bob },          // encrypt data to Bob's key only
                EncryptFlags.AlwaysTrust,   // trust our sample PGP key
                plain,                      // source buffer
                cipher);

            Console.WriteLine("done.");
            Console.WriteLine("Cipher text:");

            // move cursor to the beginning
            cipher.Seek(0, SeekOrigin.Begin);

            /* Read cipher text from libgpgme's memory based buffer and print
             * it to the console screen.
             */
            // the cipher text is UTF8 encoded
            BinaryReader binreader = new BinaryReader(cipher, utf8);
            while (true)
            {
                try
                {
                    char[] buf = binreader.ReadChars(255);
                    if (buf.Length == 0)
                        break;
                    Console.Write(buf);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }

            /////// DECRYPT DATA ///////

            /* Set the password callback - needed if the user doesn't run
             * gpg-agent or any other password / pin-entry software.
             */
            ctx.SetPassphraseFunction(MyPassphraseCallback);

            // go to the beginning(!)
            cipher.Seek(0, SeekOrigin.Begin);

            Console.Write("Decrypt data.. ");
            GpgmeData decrypted_text = new GpgmeMemoryData();

            DecryptionResult decrst = ctx.Decrypt(
                cipher,         // source buffer
                decrypted_text); // destination buffer

            Console.WriteLine("Done. Filename: \"{0}\" Recipients:", 
                decrst.FileName);

            /* print out all recipients key ids (a PGP package can be 
             * encrypted to various recipients).
             */
            if (decrst.Recipients != null)
                foreach (Recipient recp in decrst.Recipients)
                    Console.WriteLine("\tKey id {0} with {1} algorithm",
                        recp.KeyId,
                        Gpgme.GetPubkeyAlgoName(recp.KeyAlgorithm));
            else
                Console.WriteLine("\tNone");

            // TEST: Compare original data and decrypted data
            byte[] orig = new byte[255], cmp = new byte[255];

            plain.Seek(0, SeekOrigin.Begin);
            decrypted_text.Seek(0, SeekOrigin.Begin);

            while (true)
            {
                try
                {
                    int a = plain.Read(orig, orig.Length);
                    int b = decrypted_text.Read(cmp, cmp.Length);

                    if (a != b)
                        throw new DecryptionFailedException("The two data buffers have different sizes.");

                    if (a == 0)
                        break; // everything okay - end of stream reached.

                    for (int i = 0; i < a; i++)
                        if (orig[i] != cmp[i])
                            throw new DecryptionFailedException("The two data buffers differ at position "
                                + i.ToString(CultureInfo.InvariantCulture) + ".");
                }
                catch (EndOfStreamException)
                {
                    throw new DecryptionFailedException("The two data buffers have different sizes.");
                }
            }

            // we do not want our GpgmeData buffers destroyed
            GC.KeepAlive(binwriter);
            GC.KeepAlive(binreader);

            /////// FILE BASED DATA BUFFERS ///////

            /////// ENCRYPT FILE ///////

            // Now let's use a file based data buffers

            // create the "source" file first and fill it with our sample text.
            Console.WriteLine("Create new file plainfile.txt.");
            File.WriteAllText("plainfile.txt", secrettext, utf8);
            
            GpgmeData plainfile = new GpgmeFileData(
                "plainfile.txt",
                FileMode.Open,
                FileAccess.Read);

            GpgmeData cipherfile = new GpgmeFileData(
                "cipherfile.asc",
                FileMode.Create,
                FileAccess.ReadWrite);

            Console.Write("Encrypt file plainfile.txt to cipherfile.asc.. ");

            ctx.Encrypt(
                new Key[] { bob },
                EncryptFlags.AlwaysTrust,
                plainfile,
                cipherfile);

            Console.WriteLine("done.");

            plainfile.Close();
            cipherfile.Close();

            /////// DECRYPT FILE ///////

            //cipherfile = new GpgmeFileData("cipherfile.asc");
            // load the file content into the system memory
            cipherfile = new GpgmeMemoryData("cipherfile.asc");

            plainfile = new GpgmeFileData(
                "decrypted.txt",
                FileMode.Create,
                FileAccess.Write);

            Console.WriteLine("Decrypt file cipherfile.asc to decrypted.txt.. ");

            decrst = ctx.Decrypt(
                 cipherfile,    // source buffer
                 plainfile);    // destination buffer

            Console.WriteLine("Done. Filename: \"{0}\" Recipients:",
                decrst.FileName);

            /* print out all recipients key ids (a PGP package can be 
             * encrypted to various recipients).
             */
            if (decrst.Recipients != null)
                foreach (Recipient recp in decrst.Recipients)
                    Console.WriteLine("\tKey id {0} with {1} algorithm",
                        recp.KeyId,
                        Gpgme.GetPubkeyAlgoName(recp.KeyAlgorithm));
            else
                Console.WriteLine("\tNone");

            cipherfile.Close();
            plainfile.Close();
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

            passwd = Console.ReadLine().ToCharArray();

            return PassphraseResult.Success;
        }
    }
}
