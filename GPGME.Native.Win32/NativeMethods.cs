using Libgpgme.Interop;
using System;
using System.Runtime.InteropServices;
using GPGME.Native.Shared;

#if WINDOWS
namespace GPGME.Native.Win32
#else
namespace GPGME.Native.Unix
#endif
{
	public static class NativeMethods
	{
#if WINDOWS
		private const string LIBRARY_NAME = "libgpgme-11.dll";
#else
		private const string LIBRARY_NAME = "libgpgme.so.11";
#endif

        /* Check that the library fulfills the version requirement.  Note:
           This is here only for the case where a user takes a pointer from
           the old version of this function.  The new version and macro for
           run-time checks are below.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_check_version(
            [In] IntPtr req_version);

        // const char*

        /* Check that the library fulfills the version requirement and check
           for struct layout mismatch involving bitfields.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_check_version_internal(
            [In] IntPtr req_version, // const char *
            [In] IntPtr offset_sig_validity);

        // size_t

        /* Return a pointer to a string containing a description of the error
           code in the error value ERR.  This function is not thread safe.  
           
           const char *gpgme_strerror (gpgme_error_t err) */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_strerror(
            [In] int err);


        /* Return the error string for ERR in the user-supplied buffer BUF of
           size BUFLEN.  This function is, in contrast to gpg_strerror,
           thread-safe if a thread-safe strerror_r() function is provided by
           the system.  If the function succeeds, 0 is returned and BUF
           contains the string describing the error.  If the buffer was not
           large enough, ERANGE is returned and BUF contains as much of the
           beginning of the error string as fits into the buffer. 
         
           int gpgme_strerror_r(gpg_error_t err, char* buf, size_t buflen); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_strerror_r(
            [In] int err,
            [In] IntPtr buf,
            [In] UIntPtr buflen);

        /* Return a pointer to a string containing a description of the error
           source in the error value ERR.  
           
           const char *gpgme_strsource (gpgme_error_t err); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_strsource(
            [In] int err);


        /* Retrieve the error code for the system error ERR.  This returns
           GPG_ERR_UNKNOWN_ERRNO if the system error is not mapped (report
           this).  
           
           gpgme_err_code_t gpgme_err_code_from_errno(int err); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern gpg_err_code_t gpgme_err_code_from_errno(
            [In] int err);

        /* Retrieve the system error for the error code CODE.  This returns 0
           if CODE is not a system error code.  
           
           int gpgme_err_code_to_errno(gpgme_err_code_t code); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_err_code_to_errno(
            [In] gpg_err_code_t code);

        /* Return an error value with the error source SOURCE and the system
           error ERR.  
           
           gpgme_error_t gpgme_err_make_from_errno(gpgme_err_source_t source, int err); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_err_make_from_errno(
            [In] gpg_err_source_t source,
            [In] int err);

        /* Return an error value with the system error ERR.  
        
           gpgme_err_code_t gpgme_error_from_errno(int err); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern gpg_err_code_t gpgme_error_from_errno(
            [In] int err);

        /* Get the string describing protocol PROTO, or NULL if invalid.  
        const char *gpgme_get_protocol_name (gpgme_protocol_t proto); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_get_protocol_name(
            [In] gpgme_protocol_t proto);


        /* Verify that the engine implementing PROTO is installed and
           available.  
        gpgme_error_t gpgme_engine_check_version(gpgme_protocol_t proto); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_engine_check_version(
            [In] gpgme_protocol_t proto);

        /* Get the information about the configured and installed engines.  A
           pointer to the first engine in the statically allocated linked list
           is returned in *INFO.  If an error occurs, it is returned.  The
           returned data is valid until the next gpgme_set_engine_info.  
        gpgme_error_t gpgme_get_engine_info(gpgme_engine_info_t* engine_info); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_get_engine_info(
            [Out] out IntPtr engine_info);

        /* Return a statically allocated string with the name of the public
           key algorithm ALGO, or NULL if that name is not known.  
        const char *gpgme_pubkey_algo_name (gpgme_pubkey_algo_t algo); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_pubkey_algo_name(
            [In] gpgme_pubkey_algo_t algo);

        /* Return a statically allocated string with the name of the hash
           algorithm ALGO, or NULL if that name is not known.  
         const char *gpgme_hash_algo_name (gpgme_hash_algo_t algo); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_hash_algo_name(
            [In] gpgme_hash_algo_t algo);

        /* Create a new context and return it in CTX.  
        gpgme_error_t gpgme_new(gpgme_ctx_t* ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_new(
            [Out] out IntPtr ctx);

        /* Release the context CTX.  
        void gpgme_release(gpgme_ctx_t ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_release(
            [In] IntPtr ctx);

        /* Set the protocol to be used by CTX to PROTO.  
        gpgme_error_t gpgme_set_protocol(gpgme_ctx_t ctx, gpgme_protocol_t proto); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_set_protocol(
            [In] IntPtr ctx,
            [In] gpgme_protocol_t proto);

        /* Get the protocol used with CTX 
        gpgme_protocol_t gpgme_get_protocol(gpgme_ctx_t ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern gpgme_protocol_t gpgme_get_protocol(
            [In] IntPtr ctx);

        /* Specifies the pinentry mode to be used.
           For GnuPG >= 2.1 this option is required to be set to GPGME_PINENTRY_MODE_LOOPBACK 
           to enable the passphrase callback mechanism in GPGME through gpgme_set_passphrase_cb.
         
         gpgme_error_t gpgme_set_pinentry_mode (gpgme_ctx_t ctx, gpgme_pinentry_mode_t mode) 
         */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_set_pinentry_mode(
            [In] IntPtr ctx,
            gpgme_pinentry_mode_t mode);

        /* Returns the mode set for the context. 
        gpgme_pinentry_mode_t gpgme_get_pinentry_mode(gpgme_ctx_t ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern gpgme_pinentry_mode_t gpgme_get_pinentry_mode(
            [In] IntPtr ctx);

        /* Get the information about the configured engines.  A pointer to the
           first engine in the statically allocated linked list is returned.
           The returned data is valid until the next gpgme_ctx_set_engine_info.  
        gpgme_engine_info_t gpgme_ctx_get_engine_info(gpgme_ctx_t ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_ctx_get_engine_info(
            [In] IntPtr ctx);

        /* Set the engine info for the context CTX, protocol PROTO, to the
           file name FILE_NAME and the home directory HOME_DIR.  
         gpgme_error_t gpgme_ctx_set_engine_info (gpgme_ctx_t ctx,
					 gpgme_protocol_t proto,
					 const char *file_name,
					 const char *home_dir); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_ctx_set_engine_info(
            [In] IntPtr ctx,
            [In] gpgme_protocol_t proto,
            [In] IntPtr file_name,
            [In] IntPtr home_dir);


        /* If YES is non-zero, enable armor mode in CTX, disable it otherwise.  
        void gpgme_set_armor(gpgme_ctx_t ctx, int yes) */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_set_armor(
            [In] IntPtr ctx,
            [In] int yes);

        /* Return non-zero if armor mode is set in CTX.  
        int gpgme_get_armor(gpgme_ctx_t ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_get_armor(
            [In] IntPtr ctx);


        /* If YES is non-zero, enable text mode in CTX, disable it otherwise.  
        void gpgme_set_textmode(gpgme_ctx_t ctx, int yes); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_set_textmode(
            [In] IntPtr ctx,
            [In] int yes);

        /* Return non-zero if text mode is set in CTX.  
        int gpgme_get_textmode(gpgme_ctx_t ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_get_textmode(
            [In] IntPtr ctx);

        /* Include up to NR_OF_CERTS certificates in an S/MIME message.  
        void gpgme_set_include_certs(gpgme_ctx_t ctx, int nr_of_certs); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_set_include_certs(
            [In] IntPtr ctx,
            [In] int nr_of_certs);

        /* Return the number of certs to include in an S/MIME message.  
        int gpgme_get_include_certs(gpgme_ctx_t ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_get_include_certs(
            [In] IntPtr ctx);

        /* Set keylist mode in CTX to MODE.  
        gpgme_error_t gpgme_set_keylist_mode(gpgme_ctx_t ctx,
                              gpgme_keylist_mode_t mode); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_set_keylist_mode(
            [In] IntPtr ctx,
            gpgme_keylist_mode_t mode);

        /* Get keylist mode in CTX.  
        gpgme_keylist_mode_t gpgme_get_keylist_mode(gpgme_ctx_t ctx); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern gpgme_keylist_mode_t gpgme_get_keylist_mode(
            [In] IntPtr ctx);

        /* Set the passphrase callback function in CTX to CB.  HOOK_VALUE is
           passed as first argument to the passphrase callback function.  
        void gpgme_set_passphrase_cb(gpgme_ctx_t ctx,
                                     gpgme_passphrase_cb_t cb, void* hook_value); */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_set_passphrase_cb(
            [In] IntPtr ctx,
            [In] gpgme_passphrase_cb_t cb,
            [In] IntPtr hook_value);

        /* Start a keylist operation within CTX, searching for keys which
           match PATTERN.  If SECRET_ONLY is true, only secret keys are
           returned.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_keylist_start(
            [In] IntPtr ctx,
            [In] IntPtr pattern,
            [In] int secret_only);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_keylist_ext_start(
            [In] IntPtr ctx,
            [In] IntPtr[] pattern,
            [In] int secret_only,
            [In] int reserved);

        /* Return the next key from the keylist in R_KEY.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_keylist_next(
            [In] IntPtr ctx,
            [Out] out IntPtr r_key);

        /* Terminate a pending keylist operation within CTX.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_keylist_end(
            [In] IntPtr ctx);

        /* Release a reference to KEY.  If this was the last one the key is
           destroyed.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_key_unref(
            [In] IntPtr key);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_key_release(
            [In] IntPtr key);

        /* Get the key with the fingerprint FPR from the crypto backend.  If
          SECRET is true, get the secret key.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_get_key(
            [In] IntPtr ctx,
            [In] IntPtr fpr,
            [Out] out IntPtr r_key,
            [In] int secret);

        /* Generate a new keypair and add it to the keyring.  PUBKEY and
           SECKEY should be null for now.  PARMS specifies what keys should be
           generated.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_genkey_start(
            [In] IntPtr ctx,
            [In] IntPtr parms, // const char*
            [In] IntPtr pubkey, // gpgme_data_t
            [In] IntPtr seckey //gpgme_data_t
            );

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_genkey(
            [In] IntPtr ctx,
            [In] IntPtr parms, // const char*
            [In] IntPtr pubkey, // gpgme_data_t
            [In] IntPtr seckey //gpgme_data_t
            );

        /* Retrieve a pointer to the result of the genkey operation.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_op_genkey_result(
            [In] IntPtr ctx);


        /* Export the keys found by PATTERN into KEYDATA.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_export(
            [In] IntPtr ctx,
            [In] IntPtr pattern, // const char*
            [In] uint reserved,
            [In] IntPtr keydata);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_export_ext(
            [In] IntPtr ctx,
            [In] IntPtr[] pattern, //const char *pattern[]
            [In] uint reserved,
            [In] IntPtr keydata);

        /* Create a new data buffer and return it in R_DH.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_new(
            [Out] out IntPtr r_dh);

        /* Destroy the data buffer DH.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_data_release(
            [In] IntPtr dh);

        /* Create a new data buffer filled with SIZE bytes starting from
           BUFFER.  If COPY is zero, copying is delayed until necessary, and
           the data is taken from the original location when needed.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_new_from_mem(
            [Out] out IntPtr r_dh,
            [In] IntPtr buffer,
            [In] UIntPtr size, //size_t
            [In] int copy);

        /* Create a new data buffer filled with LENGTH bytes starting from
           OFFSET within the file FNAME or stream FP (exactly one must be
           non-zero).  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_new_from_filepart(
            [Out] out IntPtr r_dh,
            [In] IntPtr fname, // const char*
            [In] IntPtr fp, //FILE *
            [In] IntPtr offset, // off_t
            [In] UIntPtr length);

        //size_t
        /* Create a new data buffer filled with LENGTH bytes starting from
           OFFSET within the file FNAME or stream FP (exactly one must be
           non-zero).  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_new_from_filepart(
            [Out] out IntPtr r_dh,
            [In] IntPtr fname, // const char*
            [In] IntPtr fp, //FILE *
            [In] long offset, // off_t
            [In] UIntPtr length);

        //size_t

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_new_from_fd(
            [Out] out IntPtr dh,
            int fd);

        /*
        [DllImport(LIBRARY_NAME, CharSet=CharSet.Ansi)]
        internal extern static int gpgme_data_new_from_cbs(
            [Out] out IntPtr dh,
            [In] _gpgme_data_cbs cbs, //gpgme_data_cbs_t
            [In] IntPtr handle);

        */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_new_from_cbs(
            [Out] out IntPtr dh,
            [In] [MarshalAs(UnmanagedType.FunctionPtr)] _gpgme_data_cbs cbs, //gpgme_data_cbs_t 
            [In] IntPtr handle);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_new_from_cbs(
            [Out] out IntPtr dh,
            [In] [MarshalAs(UnmanagedType.FunctionPtr)] _gpgme_data_cbs_lfs cbs, //gpgme_data_cbs_t_lfs
            [In] IntPtr handle);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_new_from_cbs(
            [Out] out IntPtr dh,
            [In] IntPtr cbs, //gpgme_data_cbs_t
            [In] IntPtr handle);


        /* Read up to SIZE bytes into buffer BUFFER from the data object with
           the handle DH.  Return the number of characters read, 0 on EOF and
           -1 on error.  If an error occurs, errno is set.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_data_read(
            [In] IntPtr dh,
            [In] byte[] buffer,
            [In] UIntPtr size);

        //size_t

        /* Read up to SIZE bytes into buffer BUFFER from the data object with
           the handle DH.  Return the number of characters read, 0 on EOF and
           -1 on error.  If an error occurs, errno is set.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_data_read(
            [In] IntPtr dh,
            [In] IntPtr buffer,
            [In] UIntPtr size);

        //size_t

        /* Set the current position from where the next read or write starts
           in the data object with the handle DH to OFFSET, relativ to
           WHENCE.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_data_seek(
            [In] IntPtr dh,
            [In] IntPtr offset, // off_t
            [In] int whence);

        /* Set the current position from where the next read or write starts
           in the data object with the handle DH to OFFSET, relativ to
           WHENCE.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long gpgme_data_seek(
            [In] IntPtr dh,
            [In] long offset, // off_t
            [In] int whence);

        /* Write up to SIZE bytes from buffer BUFFER to the data object with
           the handle DH.  Return the number of characters written, or -1 on
           error.  If an error occurs, errno is set.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_data_write(
            [In] IntPtr dh,
            [In] byte[] buffer,
            [In] UIntPtr size);

        /* Write up to SIZE bytes from buffer BUFFER to the data object with
           the handle DH.  Return the number of characters written, or -1 on
           error.  If an error occurs, errno is set.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_data_write(
            [In] IntPtr dh,
            [In] IntPtr buffer,
            [In] UIntPtr size);


        /* Get the file name associated with the data object with handle DH, or
           NULL if there is none.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_data_get_file_name(
            [In] IntPtr dh);

        /* Set the file name associated with the data object with handle DH to
           FILE_NAME.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_set_file_name(
            [In] IntPtr dh,
            [In] IntPtr file_name);

        /* Return the encoding attribute of the data buffer DH */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern gpgme_data_encoding_t gpgme_data_get_encoding(
            [In] IntPtr dh);

        /* Set the encoding attribute of data buffer DH to ENC */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_data_set_encoding(
            [In] IntPtr dh,
            [In] gpgme_data_encoding_t enc);


        /* Retrieve a pointer to the result of the import operation.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_op_import_result(
            [In] IntPtr ctx);

        // returns gpgme_import_result_t

        /* Import the key in KEYDATA into the keyring.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_import_start(
            [In] IntPtr ctx,
            [In] IntPtr keydata);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_import(
            [In] IntPtr ctx,
            [In] IntPtr keydata);


        /* Delete KEY from the keyring.  If ALLOW_SECRET is non-zero, secret
           keys are also deleted.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_delete_start(
            [In] IntPtr ctx,
            [In] IntPtr key,
            [In] int allow_secret);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_delete(
            [In] IntPtr ctx,
            [In] IntPtr key,
            [In] int allow_secret);

        /* Start a trustlist operation within CTX, searching for trust items
           which match PATTERN.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_trustlist_start(
            [In] IntPtr ctx,
            [In] IntPtr pattern, // const char * 
            [In] int max_level);

        /* Return the next trust item from the trustlist in R_ITEM.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_trustlist_next(
            [In] IntPtr ctx,
            [Out] out IntPtr r_item //gpgme_trust_item_t *
            );

        /* Terminate a pending trustlist operation within CTX.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_trustlist_end(
            [In] IntPtr ctx);

        /* Acquire a reference to ITEM.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_trust_item_ref(
            [In] IntPtr item //gpgme_trust_item_t
            );

        /* Release a reference to ITEM.  If this was the last one the trust
           item is destroyed.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_trust_item_unref(
            [In] IntPtr item //gpgme_trust_item_t
            );

        /* Encrypt plaintext PLAIN within CTX for the recipients RECP and
           store the resulting ciphertext in CIPHER.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_encrypt_start(
            [In] IntPtr ctx,
            [In] IntPtr[] recp, // gpgme_key_t []
            [In] gpgme_encrypt_flags_t flags,
            [In] IntPtr plain, // gpgme_data_t
            [In] IntPtr cipher //gpgme_data_t
            );

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_encrypt(
            [In] IntPtr ctx,
            [In] IntPtr[] recp, // gpgme_key_t []
            [In] gpgme_encrypt_flags_t flags,
            [In] IntPtr plain, //gpgme_data_t
            [In] IntPtr cipher //gpgme_data_t
            );

        /* Retrieve a pointer to the result of the encrypt operation.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_op_encrypt_result(
            [In] IntPtr ctx);

        // returns gpgme_encrypt_result_t


        /* Encrypt plaintext PLAIN within CTX for the recipients RECP and
           store the resulting ciphertext in CIPHER.  Also sign the ciphertext
           with the signers in CTX.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_encrypt_sign_start(
            [In] IntPtr ctx,
            [In] IntPtr[] recp, //gpgme_key_t[]
            [In] gpgme_encrypt_flags_t flags,
            [In] IntPtr plain,
            [In] IntPtr cipher);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_encrypt_sign(
            [In] IntPtr ctx,
            [In] IntPtr[] recp, //gpgme_key_t[]
            [In] gpgme_encrypt_flags_t flags,
            [In] IntPtr plain,
            [In] IntPtr cipher);

        /* Delete all signers from CTX.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_signers_clear(
            [In] IntPtr ctx);

        /* Add KEY to list of signers in CTX.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_signers_add(
            [In] IntPtr ctx,
            [In] IntPtr key // const gpgme_key_t
            );

        /* Return the SEQth signer's key in CTX.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_signers_enum(
            [In] IntPtr ctx,
            [In] int seq);

        // returns gpgme_key_t


        /* Retrieve a pointer to the result of the signing operation.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_op_sign_result(
            [In] IntPtr ctx);

        // returns gpgme_sign_result_t 

        /* Sign the plaintext PLAIN and store the signature in SIG.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_sign_start(
            [In] IntPtr ctx,
            [In] IntPtr plain,
            [In] IntPtr sig,
            [In] gpgme_sig_mode_t mode);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_sign(
            [In] IntPtr ctx,
            [In] IntPtr plain,
            [In] IntPtr sig,
            [In] gpgme_sig_mode_t mode);

        /* Clear all notation data from the context.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void gpgme_sig_notation_clear(
            [In] IntPtr ctx);

        /* Add the human-readable notation data with name NAME and value VALUE
           to the context CTX, using the flags FLAGS.  If NAME is NULL, then
           VALUE should be a policy URL.  The flag
           GPGME_SIG_NOTATION_HUMAN_READABLE is forced to be true for notation
           data, and false for policy URLs.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_sig_notation_add(
            [In] IntPtr ctx,
            [In] IntPtr name, // const char *
            [In] IntPtr value, // const char *
            [In] gpgme_sig_notation_flags_t flags);

        /* Get the sig notations for this context.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_sig_notation_get(
            [In] IntPtr ctx);

        // returns gpgme_sig_notation_t


        /* Retrieve a pointer to the result of the decrypt operation.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_op_decrypt_result(
            [In] IntPtr ctx);

        // returns gpgme_decrypt_result_t

        /* Decrypt ciphertext CIPHER within CTX and store the resulting
           plaintext in PLAIN.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_decrypt_start(
            [In] IntPtr ctx,
            [In] IntPtr cipher,
            [In] IntPtr plain);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_decrypt(
            [In] IntPtr ctx,
            [In] IntPtr cipher,
            [In] IntPtr plain);

        /* Decrypt ciphertext CIPHER and make a signature verification within
           CTX and store the resulting plaintext in PLAIN.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_decrypt_verify_start(
            [In] IntPtr ctx,
            [In] IntPtr cipher,
            [In] IntPtr plain);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_decrypt_verify(
            [In] IntPtr ctx,
            [In] IntPtr cipher,
            [In] IntPtr plain);


        /* Retrieve a pointer to the result of the verify operation.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_op_verify_result(
            [In] IntPtr ctx);

        // returns gpgme_verify_result_t

        /* Verify within CTX that SIG is a valid signature for TEXT.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_verify_start(
            [In] IntPtr ctx,
            [In] IntPtr sig,
            [In] IntPtr signed_text,
            [In] IntPtr plaintext);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_verify(
            [In] IntPtr ctx,
            [In] IntPtr sig,
            [In] IntPtr signed_text,
            [In] IntPtr plaintext);

        /* Edit the key KEY.  Send status and command requests to FNC and
           output of edit commands to OUT.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_edit_start(
            [In] IntPtr ctx,
            [In] IntPtr key, // gpgme_key_t
            [In] gpgme_edit_cb_t fnc, // gpgme_edit_cb_t 
            [In] IntPtr fnc_value, // void *
            [In] IntPtr outdata // gpgme_data_t
            );

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_edit(
            [In] IntPtr ctx,
            [In] IntPtr key, // gpgme_key_t
            [In] gpgme_edit_cb_t fnc, // gpgme_edit_cb_t
            [In] IntPtr fnc_value, // void *
            [In] IntPtr outdata // gpgme_data_t
            );

        /* Edit the card for the key KEY.  Send status and command requests to
           FNC and output of edit commands to OUT.  */

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_card_edit_start(
            [In] IntPtr ctx,
            [In] IntPtr key, //gpgme_key_t
            [In] gpgme_edit_cb_t fnc, //gpgme_edit_cb_t 
            [In] IntPtr fnc_value, //void *
            [In] IntPtr outdata //gpgme_data_t
            );

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_op_card_edit(
            [In] IntPtr ctx,
            [In] IntPtr key, //gpgme_key_t
            [In] gpgme_edit_cb_t fnc, //gpgme_edit_cb_t 
            [In] IntPtr fnc_value, //void *
            [In] IntPtr outdata //gpgme_data_t
            );


        /* Wrappers around the internal I/O functions for use with
   gpgme_passphrase_cb_t and gpgme_edit_cb_t.  */
        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr  gpgme_io_read(
            [In] int fd, 
            [In,Out] byte[] buffer, 
            [In] UIntPtr count);
        
        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr gpgme_io_write (
            [In] int fd, 
            [In] byte[] buffer, 
            [In] UIntPtr count);

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int gpgme_io_writen(
            [In] int fd,
            [In] byte[] buffer,
            [In] UIntPtr count);

        public static NativeMethodsWrapper CreateWrapper()
        {
            return new NativeMethodsWrapper
            {
                gpgme_check_version = gpgme_check_version,
                gpgme_ctx_get_engine_info = gpgme_ctx_get_engine_info,
                gpgme_ctx_set_engine_info = gpgme_ctx_set_engine_info,
                gpgme_data_get_encoding = gpgme_data_get_encoding,
                gpgme_data_get_file_name = gpgme_data_get_file_name,
                gpgme_data_new = gpgme_data_new,
                gpgme_data_new_from_cbs = gpgme_data_new_from_cbs,
                gpgme_data_new_from_filepart_1 = gpgme_data_new_from_filepart,
                gpgme_data_new_from_filepart_2 = gpgme_data_new_from_filepart,
                gpgme_data_new_from_mem = gpgme_data_new_from_mem,
                gpgme_data_read_1 = gpgme_data_read,
                gpgme_data_read_2 = gpgme_data_read,
                gpgme_data_release = gpgme_data_release,
                gpgme_data_seek_1 = gpgme_data_seek,
                gpgme_data_seek_2 = gpgme_data_seek,
                gpgme_data_set_encoding = gpgme_data_set_encoding,
                gpgme_data_set_file_name = gpgme_data_set_file_name,
                gpgme_data_write_1 = gpgme_data_write,
                gpgme_data_write_2 = gpgme_data_write,
                gpgme_engine_check_version = gpgme_engine_check_version,
                gpgme_get_armor = gpgme_get_armor,
                gpgme_get_engine_info = gpgme_get_engine_info,
                gpgme_get_include_certs = gpgme_get_include_certs,
                gpgme_get_key = gpgme_get_key,
                gpgme_get_keylist_mode = gpgme_get_keylist_mode,
                gpgme_get_pinentry_mode = gpgme_get_pinentry_mode,
                gpgme_get_protocol_name = gpgme_get_protocol_name,
                gpgme_get_textmode = gpgme_get_textmode,
                gpgme_get_protocol = gpgme_get_protocol,
                gpgme_hash_algo_name = gpgme_hash_algo_name,
                gpgme_io_write = gpgme_io_write,
                gpgme_io_writen = gpgme_io_writen,
                gpgme_key_release = gpgme_key_release,
                gpgme_new = gpgme_new,
                gpgme_op_decrypt = gpgme_op_decrypt,
                gpgme_op_decrypt_result = gpgme_op_decrypt_result,
                gpgme_op_decrypt_verify = gpgme_op_decrypt_verify,
                gpgme_op_delete = gpgme_op_delete,
                gpgme_op_edit = gpgme_op_edit,
                gpgme_op_encrypt = gpgme_op_encrypt,
                gpgme_op_encrypt_result = gpgme_op_encrypt_result,
                gpgme_op_encrypt_sign = gpgme_op_encrypt_sign,
                gpgme_op_export = gpgme_op_export,
                gpgme_op_export_ext = gpgme_op_export_ext,
                gpgme_op_genkey = gpgme_op_genkey,
                gpgme_op_genkey_result = gpgme_op_genkey_result,
                gpgme_op_import = gpgme_op_import,
                gpgme_op_import_result = gpgme_op_import_result,
                gpgme_op_keylist_end = gpgme_op_keylist_end,
                gpgme_op_keylist_ext_start = gpgme_op_keylist_ext_start,
                gpgme_op_keylist_next = gpgme_op_keylist_next,
                gpgme_op_keylist_start = gpgme_op_keylist_start,
                gpgme_op_sign = gpgme_op_sign,
                gpgme_op_sign_result = gpgme_op_sign_result,
                gpgme_op_trustlist_end = gpgme_op_trustlist_end,
                gpgme_op_trustlist_next = gpgme_op_trustlist_next,
                gpgme_op_trustlist_start = gpgme_op_trustlist_start,
                gpgme_op_verify = gpgme_op_verify,
                gpgme_op_verify_result = gpgme_op_verify_result,
                gpgme_pubkey_algo_name = gpgme_pubkey_algo_name,
                gpgme_release = gpgme_release,
                gpgme_set_armor = gpgme_set_armor,
                gpgme_set_include_certs = gpgme_set_include_certs,
                gpgme_set_keylist_mode = gpgme_set_keylist_mode,
                gpgme_set_passphrase_cb = gpgme_set_passphrase_cb,
                gpgme_set_textmode = gpgme_set_textmode,
                gpgme_set_pinentry_mode = gpgme_set_pinentry_mode,
                gpgme_set_protocol = gpgme_set_protocol,
                gpgme_sig_notation_add = gpgme_sig_notation_add,
                gpgme_sig_notation_clear = gpgme_sig_notation_clear,
                gpgme_sig_notation_get = gpgme_sig_notation_get,
                gpgme_signers_add = gpgme_signers_add,
                gpgme_signers_clear = gpgme_signers_clear,
                gpgme_signers_enum = gpgme_signers_enum,
                gpgme_strerror = gpgme_strerror,
                gpgme_strerror_r = gpgme_strerror_r,
                gpgme_strsource = gpgme_strsource,
                gpgme_trust_item_unref = gpgme_trust_item_unref,
            };
        }
    }
}
