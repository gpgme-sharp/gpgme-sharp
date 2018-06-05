using System;
using System.Globalization;
using System.Text;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class PgpKey : Key
    {
        internal enum KeyEditOp
        {
            Signature, // sign, lsign, tsign, nrsign, ..
            Passphrase, // passwd
            RevokeSignature, // revsig
            DeleteSignature, // delsig
            EnableDisable, // enable, disable
            Trust, // trust
            AddSubkey, // addkey
            Expire // expire
        };

        // Variables for key editing

        // general key edit settings
        private readonly Settings _settings;


        internal PgpKey(IntPtr keyPtr)
            : base(keyPtr) {
            _settings = new Settings(this);
        }

        public Settings EditSettings {
            get { return _settings; }
        }

        protected override int KeyEditCallback(IntPtr handle, KeyEditStatusCode status, string args, int fd) {
            var op = (KeyEditOp) handle;
            byte[] output = null;
            
#if (VERBOSE_DEBUG)
            DebugOutput("Callback op=" + op.ToString() + " status=" + status.ToString() + " args=" + args);
#endif

            // Ignore ACK calls
            bool runhandler = !(status == KeyEditStatusCode.GotIt && args == null);

            // actions that are equal in all key editing queries - except passphrase changes
            if (op != KeyEditOp.Passphrase) {
                switch (status) {
                    case KeyEditStatusCode.GoodPassphrase:
                        _settings.passSettings.PassphrasePrevWasBad = false;
                        output = new byte[0];
                        runhandler = false;
                        break;
                }
                if (args != null) {
                    switch (status) {
                        case KeyEditStatusCode.UserIdHint:
                            _settings.passSettings.PassphraseUserIdHint = args;
                            output = new byte[0];
                            runhandler = false;
                            break;

                        case KeyEditStatusCode.NeedPassphrase:
                            _settings.passSettings.PassphraseInfo = args;
                            output = new byte[0];
                            runhandler = false;
                            break;

                        case KeyEditStatusCode.MissingPassphrase:
                        case KeyEditStatusCode.BadPassphrase:
                            _settings.passSettings.PassphrasePrevWasBad = true;
                            output = new byte[0];
                            runhandler = false;
                            break;

                        case KeyEditStatusCode.GetHidden:
                            if (args.Equals("passphrase.enter")) {
                                char[] passphrase = null;

                                /* "passphrase.enter" appears if the context has no passphrase 
                                 * callback function specified.
                                 */
                                if (_settings.passSettings.PassphraseFunction != null && _settings.passSettings.PassphraseLastResult != PassphraseResult.Canceled) {
                                    _settings.passSettings.PassphraseLastResult =
                                        _settings.passSettings.PassphraseFunction(
                                            null,
                                            new PassphraseInfo(IntPtr.Zero,
                                                _settings.passSettings.PassphraseUserIdHint,
                                                _settings.passSettings.PassphraseInfo,
                                                _settings.passSettings.PassphrasePrevWasBad),
                                            ref passphrase);
                                    if (passphrase != null) {
                                        byte[] p = Gpgme.ConvertCharArrayToUTF8(passphrase, 0);
                                        libgpgme.gpgme_io_write(fd, p, (UIntPtr)p.Length);

                                        int i;
                                        // try to clear passphrase in memory
                                        for (i = 0; i < p.Length; i++) {
                                            p[i] = 0;
                                        }
                                        for (i = 0; i < passphrase.Length; i++) {
                                            passphrase[i] = '\0';
                                        }
                                    }
                                } else if (_settings.passSettings.Passphrase != null) {
                                    byte[] p = Gpgme.ConvertCharArrayToUTF8(
                                        _settings.passSettings.Passphrase,
                                        0);
                                    libgpgme.gpgme_io_write(fd, p, (UIntPtr)p.Length);

                                    int i;
                                    // try to clear passphrase in memory
                                    for (i = 0; i < p.Length; i++) {
                                        p[i] = 0;
                                    }
                                } else {
                                    // No password or password callback function specified!
                                    libgpgme.gpgme_io_write(fd, new[] { (byte)0 }, (UIntPtr)1);
                                }

                                output = new byte[0]; // confirm password (send \n)

                                runhandler = false;
                            }

                            break;
                    }
                }
            }

            if (runhandler) {
#if (VERBOSE_DEBUG)
				DebugOutput("Run handler " + op.ToString());
#endif
                switch (op) {
                    case KeyEditOp.Signature:
                        output = SignHandler(status, args);
                        break;
                    case KeyEditOp.Passphrase:
                        output = PassphraseHandler(status, args, fd);
                        if (output == null && _settings.passOptions.aborthandler) {
                            return 1; // abort
                        }
                        break;
                    case KeyEditOp.RevokeSignature:
                        output = RevokeSignatureHandler(args);
                        break;
                    case KeyEditOp.DeleteSignature:
                        output = DeleteSignatureHandler(args);
                        break;
                    case KeyEditOp.EnableDisable:
                        output = EnableDisableHandler(args);
                        break;
                    case KeyEditOp.Trust:
                        output = TrustHandler(args);
                        break;
                    case KeyEditOp.AddSubkey:
                        output = AddSubkeyHandler(args);
                        break;
                    case KeyEditOp.Expire:
                        output = ExpireHandler(args);
                        break;
                }

#if (VERBOSE_DEBUG)
				DebugOutput("Handler " + op.ToString() + " finished.");
#endif
            }

            if (output != null) {
#if (VERBOSE_DEBUG)
                DebugOutput(output);
#endif
                libgpgme.gpgme_io_write(fd, output, (UIntPtr)output.Length);
                libgpgme.gpgme_io_write(fd, new[] {(byte) '\n'}, (UIntPtr)1);
            }

            return 0;
        }

#if (VERBOSE_DEBUG)
        private void DebugOutput(byte[] barray)
        {
            if (barray != null)
            {
                Console.Write("Debug: ");
                foreach (byte b in barray)
                    Console.Write((char)b);
                Console.WriteLine();
				Console.Out.Flush();
            }
        }
        private void DebugOutput(string text)
        {
            Console.WriteLine("Debug: " + text);
			Console.Out.Flush();
        }
#endif

        private byte[] DeleteSignatureHandler(string args) {
            PgpDeleteSignatureOptions delsig_options = _settings.delsigOptions;

            if (args != null) {
                string output;
                if (args.Equals("keyedit.prompt") && !delsig_options.cmdSend) {
                    if (!delsig_options.uidSend) {
                        // send uid number
                        delsig_options.uidSend = true;
                        output = "uid " + delsig_options.SelectedUid.ToString(CultureInfo.InvariantCulture);
                        return ToU8(output);
                    }

                    // send command
                    delsig_options.cmdSend = true;
                    output = "delsig";
                    return ToU8(output);
                }

                if (args.Equals("keyedit.delsig.unknown")
                    || args.Equals("keyedit.delsig.valid")) {
                    if (delsig_options.SelectedSignatures == null) {
                        output = "Y";
                    } else {
                        delsig_options.ndeletenum++;
                        if (Array.Exists(delsig_options.SelectedSignatures,
                            v => (v == delsig_options.ndeletenum))) {
                            output = "Y";
                        } else {
                            output = "N";
                        }
                    }
                    return ToU8(output);
                }

                if (args.Equals("keyedit.delsig.selfsig")) {
                    output = delsig_options.DeleteSelfSignature 
                        ? "Y" 
                        : "N";
                    return ToU8(output);
                }

                if (args.Equals("keyedit.prompt") && delsig_options.cmdSend) {
                    output = "save";
                    return ToU8(output);
                }
            }

            return new byte[0];
        }

        public void DeleteSignature(Context ctx, PgpDeleteSignatureOptions options) {
            if (ctx == null || !ctx.IsValid) {
                throw new InvalidContextException();
            }

            if (options == null) {
                throw new ArgumentNullException("options", "No PgpDeleteSignatureOptions object specified.");
            }

            if (options.SelectedSignatures == null ||
                options.SelectedSignatures.Length == 0) {
                throw new ArgumentException("No signatures selected.");
            }

            lock (_settings.passLock) {
                lock (_settings.delsigLock) {
                    _settings.delsigOptions = options;

                    // reset object
                    options.cmdSend = false;
                    options.uidSend = false;
                    options.ndeletenum = 0;

                    // specify key edit operation;
                    const KeyEditOp OP = KeyEditOp.DeleteSignature;

                    // output data
                    GpgmeData data = new GpgmeMemoryData();

                    int err = StartEdit(ctx, (IntPtr) OP, data);
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());

                        default:
                            throw new GpgmeException("An unknown error occurred. Error: "
                                + err.ToString(CultureInfo.InvariantCulture), err);
                    }
                }
            }
        }

        private byte[] RevokeSignatureHandler(string args) {
            PgpRevokeSignatureOptions revsig_options = _settings.revsigOptions;

            if (args != null) {
                // specify the uids from that the signature shall be revoked
                string output;
                if (args.Equals("keyedit.prompt") && !revsig_options.uidSend) {
                    revsig_options.uidSend = true;
                    output = "uid " + revsig_options.SelectedUid.ToString(CultureInfo.InvariantCulture);
                    return ToU8(output);
                }

                if (args.Equals("keyedit.prompt") && !revsig_options.cmdSend) {
                    revsig_options.cmdSend = true;
                    output = "revsig";
                    return ToU8(output);
                }

                if (args.Equals("ask_revoke_sig.one")
                    || args.Equals("ask_revoke_sig.expired")) {
                    revsig_options.nrevokenum++;
                    // the user can specify his signatures that shall be revoked
                    if (Array.Exists(revsig_options.SelectedSignatures,
                        v => (v == revsig_options.nrevokenum))) {
                        output = "Y";
                    } else {
                        output = "N";
                    }
                    return ToU8(output);
                }
                if (args.Equals("ask_revoke_sig.okay")
                    || args.Equals("ask_revocation_reason.okay")) {
                    output = "Y"; // we can revoke all signatures that were signed by private key from our store
                    return ToU8(output);
                }

                if (args.Equals("ask_revocation_reason.code")) {
                    output = ((int) revsig_options.ReasonCode).ToString(CultureInfo.InvariantCulture);
                    return ToU8(output);
                }

                if (args.Equals("ask_revocation_reason.text")) {
                    if (revsig_options.reasonTxt == null) {
                        output = "";
                    } else {
                        if (revsig_options.nreasonTxt >= revsig_options.reasonTxt.Length) {
                            output = libgpgme.IsWindows 
                                ? String.Empty
                                : " ";
                        } else {
                            output = revsig_options.reasonTxt[revsig_options.nreasonTxt++];
                        }
                    }
                    return ToU8(output);
                }

                if (args.Equals("keyedit.prompt") && revsig_options.cmdSend) {
                    output = "save";
                    return ToU8(output);
                }
            }

            return new byte[0];
        }

        public void RevokeSignature(Context ctx, PgpRevokeSignatureOptions options) {
            if (ctx == null) {
                throw new ArgumentNullException("ctx", "No context object supplied.");
            }
            if (!ctx.IsValid) {
                throw new InvalidContextException("An invalid context has been supplied.");
            }

            if (options == null) {
                throw new ArgumentNullException("options", "No revocation options specified.");
            }

            if (options.SelectedSignatures == null ||
                options.SelectedSignatures.Length == 0) {
                throw new ArgumentException("No signatures selected.");
            }

            lock (_settings.passLock) {
                lock (_settings.revsigLock) {
                    _settings.revsigOptions = options;

                    // reset object
                    options.cmdSend = false;
                    options.uidSend = false;
                    options.reasonSend = false;
                    // reset reason text counter (gnupg prompts for each line)
                    options.nreasonTxt = 0;
                    /* reset own signature counter (user could have signed the key with more
                     * than one of his keys. */
                    options.nrevokenum = 0;

                    // specify key edit operation;
                    const KeyEditOp OP = KeyEditOp.RevokeSignature;

                    // output data
                    GpgmeData data = new GpgmeMemoryData();

                    int err = StartEdit(ctx, (IntPtr) OP, data);
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());

                        default:
                            throw new GpgmeException("An unknown error occurred. Error: "
                                + err.ToString(CultureInfo.InvariantCulture), err);
                    }
                }
            }
        }

        private byte[] TrustHandler(string args) {
            PgpTrustOptions trust_options = _settings.trustOptions;

            if (args != null) {
                string output;
                if (args.Equals("keyedit.prompt")) {
                    if (!trust_options.cmdSend) {
                        output = "trust";
                        trust_options.cmdSend = true;
                    } else {
                        output = "quit";
                    }

                    return ToU8(output);
                }
                if (args.Equals("edit_ownertrust.set_ultimate.okay")) {
                    output = "Y";
                    return ToU8(output);
                }
                if (args.Equals("edit_ownertrust.value")) {
                    output = ((int) trust_options.trust).ToString(CultureInfo.InvariantCulture);
                    return ToU8(output);
                }
            }

            return new byte[0];
        }


        public void SetOwnerTrust(Context ctx, PgpOwnerTrust trust) {
            if (ctx == null) {
                throw new ArgumentNullException("ctx", "No context object supplied.");
            }
            if (!ctx.IsValid) {
                throw new InvalidContextException("An invalid context has been supplied.");
            }

            lock (_settings.trustLock) {
                _settings.trustOptions = new PgpTrustOptions {
                    trust = trust
                };

                // specify key edit operation;
                const KeyEditOp OP = KeyEditOp.Trust;

                // output data
                GpgmeData data = new GpgmeMemoryData();

                int err = StartEdit(ctx, (IntPtr) OP, data);
                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                switch (errcode) {
                    case gpg_err_code_t.GPG_ERR_NO_ERROR:
                        break;
                    case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                        throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());

                    default:
                        throw new GpgmeException("An unknown error occurred.", err);
                }
            }
        }

        private byte[] EnableDisableHandler(string args) {
            PgpEnableDisableOptions endis_options = _settings.endisOptions;
            string output = "";

            if (args != null) {
                if (args.Equals("keyedit.prompt")) {
                    if (!endis_options.cmdSend) {
                        switch (endis_options.OperationMode) {
                            case PgpEnableDisableOptions.Mode.Enable:
                                output = "enable";
                                break;
                            case PgpEnableDisableOptions.Mode.Disable:
                                output = "disable";
                                break;
                        }
                        endis_options.cmdSend = true;
                    } else {
                        output = "quit";
                    }

                    return ToU8(output);
                }
            }

            return new byte[0];
        }

        public void Enable(Context ctx) {
            if (ctx == null) {
                throw new ArgumentNullException("ctx", "No context object supplied.");
            }
            if (!ctx.IsValid) {
                throw new InvalidContextException("An invalid context has been supplied.");
            }

            lock (_settings.endisLock) {
                _settings.endisOptions = new PgpEnableDisableOptions {
                    OperationMode = PgpEnableDisableOptions.Mode.Enable
                };

                // specify key edit operation;
                const KeyEditOp OP = KeyEditOp.EnableDisable;

                // output data
                GpgmeData data = new GpgmeMemoryData();

                int err = StartEdit(ctx, (IntPtr) OP, data);
                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                switch (errcode) {
                    case gpg_err_code_t.GPG_ERR_NO_ERROR:
                        break;
                    case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                        throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());

                    default:
                        throw new GpgmeException("An unknown error occurred.", err);
                }
            }
        }

        public void Disable(Context ctx) {
            if (ctx == null) {
                throw new ArgumentNullException("ctx", "No context object supplied.");
            }
            if (!ctx.IsValid) {
                throw new InvalidContextException("An invalid context has been supplied.");
            }

            lock (_settings.endisLock) {
                _settings.endisOptions = new PgpEnableDisableOptions {
                    OperationMode = PgpEnableDisableOptions.Mode.Disable
                };

                // specify key edit operation;
                const KeyEditOp OP = KeyEditOp.EnableDisable;

                // output data
                GpgmeData data = new GpgmeMemoryData();

                int err = StartEdit(ctx, (IntPtr) OP, data);
                gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                switch (errcode) {
                    case gpg_err_code_t.GPG_ERR_NO_ERROR:
                        break;
                    case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                        throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());

                    default:
                        throw new GpgmeException("An unknown error occurred.", err);
                }
            }
        }

        private byte[] SignHandler(KeyEditStatusCode status, string args) {
            PgpSignatureOptions sig_options = _settings.sigOptions;

            if (args != null) {
                if (status == KeyEditStatusCode.AlreadySigned) {
                    throw new AlreadySignedException(args);
                }

                // specify the uids that shall be signed
                if (args.Equals("keyedit.prompt") && !sig_options.cmdSend
                    && sig_options.nUid == 0) {
                    // do we want to specify the uids that we want to sign?
                    if (sig_options.SelectedUids != null && sig_options.SelectedUids.Length > 0) {
                        sig_options.signAllUids = false;
                    } else {
                        sig_options.signAllUids = true;
                    }
                }

                string output;
                if (sig_options.SelectedUids != null 
                    && (args.Equals("keyedit.prompt") 
                    && (!sig_options.signAllUids)
                    && sig_options.nUid < sig_options.SelectedUids.Length)) 
                {
                    output = "uid " + sig_options.SelectedUids[sig_options.nUid++];
                    return ToU8(output);
                }

                if (args.Equals("keyedit.prompt") && !sig_options.cmdSend) {
                    var sb = new StringBuilder();

                    if ((sig_options.Type & PgpSignatureType.NonExportable) == PgpSignatureType.NonExportable) {
                        sb.Append("l");
                    }

                    if ((sig_options.Type & PgpSignatureType.Trust) == PgpSignatureType.Trust) {
                        sb.Append("t");
                    }

                    if ((sig_options.Type & PgpSignatureType.NonRevocable) == PgpSignatureType.NonRevocable) {
                        sb.Append("nr");
                    }

                    output = sb + "sign";

                    // mark that the operation command has been sent
                    sig_options.cmdSend = true;

                    return ToU8(output);
                }

                if (args.Equals("sign_uid.class")) {
                    output = ((int) sig_options.Class).ToString(CultureInfo.InvariantCulture);
                    return ToU8(output);
                }

                if (args.Equals("sign_uid.expire")) {
                    output = sig_options.IsInfinitely ? "N" : "Y";
                    return ToU8(output);
                }

                if (args.Equals("siggen.valid")) {
                    output = sig_options.GetExpirationDate();
                    return ToU8(output);
                }

                if (args.Equals("trustsig_prompt.trust_value")) {
                    output = ((int) sig_options.TrustLevel).ToString(CultureInfo.InvariantCulture);
                    return ToU8(output);
                }

                if (args.Equals("trustsig_prompt.trust_depth")) {
                    output = sig_options.TrustDepth.ToString(CultureInfo.InvariantCulture);
                    return ToU8(output);
                }

                if (args.Equals("trustsig_prompt.trust_regexp")) {
                    output = sig_options.TrustRegexp;
                    if (output == null) {
                        output = "";
                    }
                    return Gpgme.ConvertCharArrayAnsi(output.ToCharArray());
                }

                if (args.Equals("sign_uid.local_promote_okay")) {
                    output = sig_options.LocalPromoteOkay ? "Y" : "N";
                    return ToU8(output);
                }

                if (args.Equals("sign_uid.okay")) {
                    output = "Y"; // Really sign? (y/N)
                    return ToU8(output);
                }

                if (args.Equals("keyedit.sign_all.okay")) {
                    // Really sign all user IDs? (y/N)
                    output = sig_options.signAllUids 
                        ? "Y" 
                        : "N";
                    return ToU8(output);
                }

                if (args.Equals("keyedit.prompt")) {
                    if (!sig_options.forceQuit) {
                        output = "save";
                        sig_options.forceQuit = true;
                    } else {
                        output = "quit";
                    }
                    return ToU8(output);
                }

                if (args.Equals("keyedit.save.okay")) {
                    output = "Y"; // Save changes? (y/N) 
                    return ToU8(output);
                }

                // .. unknown question
            }

            return new byte[0];
        }

        public void Sign(Context ctx, PgpSignatureOptions options) {
            if (ctx == null) {
                throw new ArgumentNullException("ctx", "No context object supplied.");
            }
            if (!ctx.IsValid) {
                throw new InvalidContextException("An invalid context has been supplied.");
            }

            if (options == null) {
                throw new ArgumentNullException("options", "No PgpSignatureOptions object specified.");
            }

            lock (_settings.passLock) {
                lock (_settings.sigLock) {
                    _settings.sigOptions = options;

                    // reset object
                    options.cmdSend = false;
                    options.nUid = 0;
                    options.forceQuit = false;
                    options.signAllUids = true;

                    // specify key edit operation;
                    const KeyEditOp OP = KeyEditOp.Signature;

                    // output data
                    GpgmeData data = new GpgmeMemoryData();

                    int err = StartEdit(ctx, (IntPtr) OP, data);
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());

                        default:
                            throw new GpgmeException("An unknown error occurred. Error: "
                                + err.ToString(CultureInfo.InvariantCulture), err);
                    }
                }
            }
        }


        private byte[] ExpireHandler(string args) {
            PgpExpirationOptions expire_options = _settings.expireOptions;

            if (args != null) {
                string output;
                if (args.Equals("keyedit.prompt") && !expire_options.cmdSend) {
                    if (expire_options.SelectedSubkeys != null
                        && (expire_options.nsubkey < expire_options.SelectedSubkeys.Length)) {
                        output = "key " + expire_options.SelectedSubkeys[expire_options.nsubkey++].ToString(CultureInfo.InvariantCulture);
                    } else {
                        expire_options.cmdSend = true;
                        output = "expire";
                    }
                    return ToU8(output);
                }

                // Expire date in days
                if (args.Equals("keygen.valid")) {
                    if (expire_options.IsInfinitely || (expire_options.ExpirationDate.CompareTo(DateTime.Now) < 0)) {
                        output = "0";
                    } else {
                        output = (expire_options.ExpirationDate - DateTime.Now).Days.ToString(CultureInfo.InvariantCulture);
                    }

                    return ToU8(output);
                }

                if (args.Equals("keyedit.save.okay")) {
                    if (!expire_options.forceQuit) {
                        output = "Y";
                    } else {
                        // maybe an other gnupg process is editing this key at the same time
                        output = "N";
                    }

                    return ToU8(output);
                }

                if (args.Equals("keyedit.prompt") && expire_options.forceQuit) {
                    output = "quit";
                    return ToU8(output);
                }

                if (args.Equals("keyedit.prompt")) {
                    expire_options.forceQuit = true;
                    output = "save";
                    return ToU8(output);
                }
            }

            return new byte[0];
        }

        public void SetExpirationDate(Context ctx, PgpExpirationOptions options) {
            if (ctx == null) {
                throw new ArgumentNullException("ctx", "No context object supplied.");
            }
            if (!ctx.IsValid) {
                throw new InvalidContextException("An invalid context has been supplied.");
            }

            if (options == null) {
                throw new ArgumentNullException("options", "No PgpExpireOptions object specified.");
            }

            lock (_settings.passLock) {
                lock (_settings.expireLock) {
                    _settings.expireOptions = options;

                    // reset object
                    options.cmdSend = false;
                    // reset subkey index
                    options.nsubkey = 0;
                    // reset enforced quit
                    options.forceQuit = false;

                    const KeyEditOp OP = KeyEditOp.Expire;
                    GpgmeData data = new GpgmeMemoryData();

                    int err = StartEdit(ctx, (IntPtr) OP, data);
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());
                        default:
                            throw new GpgmeException("An unknown error occurred. Error: "
                                + err.ToString(CultureInfo.InvariantCulture), err);
                    }
                }
            }
        }

        private byte[] AddSubkeyHandler(string args) {
#if (VERBOSE_DEBUG)
			DebugOutput("Inside AddSubkeyHandler(..)");
#endif

            PgpSubkeyOptions subkey_options = _settings.subkeyOptions;

            if (args != null) {
                string output;
                if (args.Equals("keyedit.prompt") && !subkey_options.cmdSend) {
                    // send command
                    subkey_options.cmdSend = true;
                    output = "addkey";

#if (VERBOSE_DEBUG)
					DebugOutput("End keygen.algo.");
#endif
                    return ToU8(output);
                }
                if (args.Equals("keygen.algo")) {
                    /* Gnupg's configuration mode needs be set to "expert" 
                     * in order to specify customized DSA or RSA subkeys.
                     */

                    _settings.subkeyalgoquestion++;

                    output = ((int) subkey_options.Algorithm).ToString(CultureInfo.InvariantCulture);

                    // GPG IS NOT IN EXPERT MODE!
                    if (_settings.subkeyalgoquestion > 1) {
#if (VERBOSE_DEBUG)
						DebugOutput("End keygen.algo.");
#endif
                        return ToU8("2");
                    }

#if (VERBOSE_DEBUG)
					DebugOutput("End keygen.algo.");
#endif
                    return ToU8(output);
                }
                if (args.Equals("keygen.flags")) {
                    // Auth is NOT enabled by default
                    if ((subkey_options.Capability & AlgorithmCapability.CanAuth) == AlgorithmCapability.CanAuth &&
                        ((_settings.subkeycapability & AlgorithmCapability.CanAuth) != AlgorithmCapability.CanAuth)) {
                        _settings.subkeycapability |= AlgorithmCapability.CanAuth;
                        output = "A";
                    }
                        // Sign is enabled by default!
                    else if ((subkey_options.Capability & AlgorithmCapability.CanSign) != AlgorithmCapability.CanSign &&
                        ((_settings.subkeycapability & AlgorithmCapability.CanSign) != AlgorithmCapability.CanSign)) {
                        _settings.subkeycapability |= AlgorithmCapability.CanSign;
                        // save, that we have checked "Sign" flag
                        output = "S";
                    }
                        // Encrypt is enabled by default!
                    else if ((subkey_options.Capability & AlgorithmCapability.CanEncrypt) !=
                        AlgorithmCapability.CanEncrypt &&
                            ((_settings.subkeycapability & AlgorithmCapability.CanEncrypt) !=
                                AlgorithmCapability.CanEncrypt)) {
                        _settings.subkeycapability |= AlgorithmCapability.CanEncrypt;
                        // save, that we have checked "Encrypt" flag
                        output = "E";
                    } else {
                        // all flags specified
                        output = "Q";
                    }

#if (VERBOSE_DEBUG)
					DebugOutput("End keygen.flags.");
#endif
                    return ToU8(output);
                }
                if (args.Equals("keygen.size")) {
                    output = subkey_options.KeyLength.ToString(CultureInfo.InvariantCulture);

#if (VERBOSE_DEBUG)
					DebugOutput("End keygen.size.");
#endif
                    return ToU8(output);
                }
                if (args.Equals("keygen.valid")) {
                    if (subkey_options.IsInfinitely || (subkey_options.ExpirationDate.CompareTo(DateTime.Now) < 0)) {
                        output = "0";
                    } else {
                        output = (subkey_options.ExpirationDate - DateTime.Now).Days.ToString(CultureInfo.InvariantCulture);
                    }

#if (VERBOSE_DEBUG)
					DebugOutput("End keygen.valid.");
#endif
                    return ToU8(output);
                }
                if (args.Equals("keyedit.prompt")) {
                    if (_settings.subkeyalgoquestion > 1) {
                        /* Do not save the new created subkey because the user 
                         * requested a customized subkey but GnuPG was not in 
                         * "Expert" mode.
                         */
                        output = "quit";
                    } else {
                        output = "save";
                    }

#if (VERBOSE_DEBUG)
					DebugOutput("End --edit-key session.");
#endif
                    return ToU8(output);
                }
            }

#if (VERBOSE_DEBUG)
			DebugOutput("End - WITH BYTE[0]");
#endif
            return new byte[0];
        }


        public void AddSubkey(Context ctx, PgpSubkeyOptions options) {
            if (ctx == null) {
                throw new ArgumentNullException("ctx", "No context object supplied.");
            }
            if (!ctx.IsValid) {
                throw new InvalidContextException("An invalid context has been supplied.");
            }

            if (options == null) {
                throw new ArgumentNullException("options", "No PgpSubkeyOptions object specified.");
            }

            lock (_settings.passLock) {
                lock (_settings.subkeyLock) {
                    _settings.subkeyOptions = options;
                    _settings.subkeycapability = AlgorithmCapability.CanNothing;
                    _settings.subkeyalgoquestion = 0;
                    options.cmdSend = false;

                    const KeyEditOp OP = KeyEditOp.AddSubkey;
                    GpgmeData data = new GpgmeMemoryData();

                    int err = StartEdit(ctx, (IntPtr) OP, data);
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);
                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            if (_settings.subkeyalgoquestion > 1) {
                                throw new NotSupportedException(
                                    "GnuPG was not in expert mode. Customized subkeys are not supported.");
                            }
                            break;
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());
                        default:
                            throw new GpgmeException("An unknown error occurred. Error: "
                                + err.ToString(CultureInfo.InvariantCulture), err);
                    }
                }
            }
        }

        private byte[] PassphraseHandler(KeyEditStatusCode status, string args, int fd) {
            PgpPassphraseOptions pass_options = _settings.passOptions;

            switch (status) {
                case KeyEditStatusCode.MissingPassphrase:
                    pass_options.missingpasswd = true; // empty passphrase
                    pass_options.emptypasswdcount++;

                    if (pass_options.missingpasswd &&
                        pass_options.emptypasswdcount >= PgpPassphraseOptions.MAX_PASSWD_COUNT) {
                        pass_options.aborthandler = true;
                        return null;
                    }
                    break;

                case KeyEditStatusCode.GoodPassphrase:
                    if (pass_options.needoldpw) // old password has been entered correctly
                    {
                        pass_options.needoldpw = false;
                    }

                    return null;


                case KeyEditStatusCode.NeedPassphraseSym:
                    if (pass_options.needoldpw) // old password has been entered already
                    {
                        pass_options.needoldpw = false;
                    }

                    return null;
            }


            if (args != null) {
                string output;
                if (args.Equals("keyedit.prompt") && !pass_options.passphraseSendCmd) {
                    pass_options.passphraseSendCmd = true;

                    output = "passwd";
                    return ToU8(output);
                }

                switch (status) {
                    case KeyEditStatusCode.UserIdHint:
                        _settings.passSettings.PassphraseUserIdHint = args;
                        return new byte[0];

                    case KeyEditStatusCode.NeedPassphrase:
                        _settings.passSettings.PassphraseInfo = args;
                        return new byte[0];

                    case KeyEditStatusCode.MissingPassphrase:
                    case KeyEditStatusCode.BadPassphrase:
                        _settings.passSettings.PassphrasePrevWasBad = true;
                        return new byte[0];

                    case KeyEditStatusCode.GetHidden:
                        if (args.Equals("passphrase.enter")) {
                            char[] passphrase = null;
                            PassphraseDelegate passphrase_func;

                            if (pass_options.needoldpw) {
                                // ask for old password
                                passphrase_func = pass_options.OldPassphraseCallback;
                                if (pass_options.OldPassphrase != null) {
                                    passphrase = new char[pass_options.OldPassphrase.Length];
                                    // TODO: can we trust Array.Copy?
                                    Array.Copy(pass_options.OldPassphrase,
                                        passphrase,
                                        pass_options.OldPassphrase.Length);
                                }
                            } else {
                                // ask for new password
                                passphrase_func = pass_options.NewPassphraseCallback;
                                if (pass_options.NewPassphrase != null) {
                                    passphrase = new char[pass_options.NewPassphrase.Length];
                                    // TODO: can we trust Array.Copy?
                                    Array.Copy(pass_options.OldPassphrase,
                                        passphrase,
                                        pass_options.NewPassphrase.Length);
                                }
                            }

                            if (passphrase_func != null && _settings.passSettings.PassphraseLastResult != PassphraseResult.Canceled) {
#if (VERBOSE_DEBUG)
								    DebugOutput("Calling passphrase callback function.. ");
#endif
                                // run callback function
                                _settings.passSettings.PassphraseLastResult =
                                    passphrase_func(
                                        null,
                                        new PassphraseInfo(IntPtr.Zero,
                                            _settings.passSettings.PassphraseUserIdHint,
                                            _settings.passSettings.PassphraseInfo,
                                            _settings.passSettings.PassphrasePrevWasBad),
                                        ref passphrase);
                            }

                            if (passphrase != null) {
                                byte[] p = Gpgme.ConvertCharArrayToUTF8(passphrase, 0);
                                libgpgme.gpgme_io_write(fd, p, (UIntPtr) p.Length);

                                // try to clear passphrase in memory
                                int i;
                                for (i = 0; i < p.Length; i++) {
                                    p[i] = 0;
                                }

                                // try to clear
                                for (i = 0; i < passphrase.Length; i++) {
                                    passphrase[i] = '\0';
                                }
                            }
                        }
                        break;
                }
                if (args.Equals("change_passwd.empty.okay")) {
                    output = (pass_options.EmptyOkay) ? "Y" : "N";
                    return ToU8(output);
                }

                if (args.Equals("keyedit.prompt")) {
                    output = "save";
                    return ToU8(output);
                }
            }

            return new byte[0];
        }

        public void ChangePassphrase(Context ctx, PgpPassphraseOptions options) {
            if (ctx == null) {
                throw new ArgumentNullException("ctx", "No context object supplied.");
            }
            if (!ctx.IsValid) {
                throw new InvalidContextException("An invalid context has been supplied.");
            }

            if (ctx.HasPassphraseFunction) {
                throw new InvalidContextException("The context must not have a passphrase callback function.");
            }

            if (options == null) {
                throw new ArgumentNullException("options", "PgpPassphraseOptions object required.");
            }

            lock (_settings.passLock) {
                lock (_settings.newpassLock) {
                    // specify key edit operation;
                    const KeyEditOp OP = KeyEditOp.Passphrase;
                    _settings.passOptions = options;

                    // reset object
                    options.passphraseSendCmd = false;
                    options.needoldpw = true;
                    options.missingpasswd = false;
                    options.aborthandler = false;
                    options.emptypasswdcount = 0;

                    // output data
                    GpgmeData data = new GpgmeMemoryData();

                    int err = StartEdit(ctx, (IntPtr) OP, data);
                    gpg_err_code_t errcode = libgpgerror.gpg_err_code(err);

                    switch (errcode) {
                        case gpg_err_code_t.GPG_ERR_NO_ERROR:
                            break;
                        case gpg_err_code_t.GPG_ERR_BAD_PASSPHRASE:
                            // This could be thrown if the user has chosen an empty password.
                            if (options.missingpasswd
                                && options.EmptyOkay == false
                                && options.aborthandler == false
                                && (options.emptypasswdcount < PgpPassphraseOptions.MAX_PASSWD_COUNT)) 
                            {
                                break;
                            }
                            throw new BadPassphraseException(_settings.passSettings.GetPassphraseInfo());
                        default:
                            if (options.missingpasswd && options.aborthandler) {
                                throw new EmptyPassphraseException(_settings.passSettings.GetPassphraseInfo());
                            }
                            throw new GpgmeException("An unknown error occurred. Error:"
                                + err.ToString(CultureInfo.InvariantCulture), err);
                    }
                }
            }
        }

        private byte[] ToU8(string text) {
            return Gpgme.ConvertCharArrayToUTF8(text.ToCharArray(), 0);
        }

        public class PassphraseSettings
        {
            // variables used to save the last passphrase information (keyid etc.)
            public char[] Passphrase;
            public PassphraseDelegate PassphraseFunction;
            internal string PassphraseInfo = "";

            // give the user the opportunity to cancel the passphrase dialogs completely
            internal PassphraseResult PassphraseLastResult = PassphraseResult.Success;
            internal bool PassphrasePrevWasBad;
            internal string PassphraseUserIdHint = "";

            internal PassphraseInfo GetPassphraseInfo() {
                return new PassphraseInfo(IntPtr.Zero,
                    PassphraseUserIdHint,
                    PassphraseInfo,
                    PassphrasePrevWasBad);
            }
        }

        public class Settings
        {
            internal object delsigLock = new object();
            internal PgpDeleteSignatureOptions delsigOptions;

            // enable/disable key
            internal object endisLock = new object();
            internal PgpEnableDisableOptions endisOptions;
            internal object expireLock = new object();
            internal PgpExpirationOptions expireOptions;
            private PgpKey key;

            // passphrase queries during key editing

            // change passphrase
            internal object newpassLock = new object();
            internal object passLock = new object();
            internal PgpPassphraseOptions passOptions;
            internal PassphraseSettings passSettings = new PassphraseSettings();

            // revoke signature
            internal object revsigLock = new object();
            internal PgpRevokeSignatureOptions revsigOptions;
            internal object sigLock = new object();
            internal PgpSignatureOptions sigOptions;

            // delete signature

            // addkey
            internal object subkeyLock = new object();
            internal PgpSubkeyOptions subkeyOptions;
            internal int subkeyalgoquestion;
            internal AlgorithmCapability subkeycapability;
            internal object trustLock = new object();
            internal PgpTrustOptions trustOptions;

            // expire

            internal Settings(PgpKey key) {
                this.key = key;
            }

            public PassphraseDelegate PassphraseFunction {
                get {
                    lock (passLock) {
                        return passSettings.PassphraseFunction;
                    }
                }
                set {
                    lock (passLock) {
                        passSettings.PassphraseFunction = value;
                    }
                }
            }
            public char[] Passphrase {
                get {
                    lock (passLock) {
                        return passSettings.Passphrase;
                    }
                }
                set {
                    lock (passLock) {
                        passSettings.Passphrase = value;
                    }
                }
            }
        }
    }
}