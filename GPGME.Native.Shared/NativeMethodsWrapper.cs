using System;
using Libgpgme.Interop;

namespace GPGME.Native.Shared
{
    /// <summary>
    /// Wraps all the native methods used by gpgme-sharp.
    ///
    /// This is required as the library name differs on Windows (libgpgme-11.dll) vs on
    /// Linux (libgpgme.so.11). .NET Core does not support a feature like "DllMap" in Mono, so we
    /// need to have two sets of DllImports: One for Windows, and one for every platform.
    ///
    /// Unfortunately, DllImports must be static, so we can't put them in a class with an interface.
    /// Instead, we have this messy wrapper class that contains delegates for all the native
    /// methods.
    /// </summary>
    public class NativeMethodsWrapper
    {
        public Func<IntPtr, IntPtr> gpgme_check_version { get; set; }
        public Func<IntPtr, IntPtr> gpgme_ctx_get_engine_info { get; set; }
        public Func<IntPtr, gpgme_protocol_t, IntPtr, IntPtr, int> gpgme_ctx_set_engine_info { get; set; }
        public Func<IntPtr, gpgme_data_encoding_t> gpgme_data_get_encoding { get; set; }
        public Func<IntPtr, IntPtr> gpgme_data_get_file_name { get; set; }
        public GPGDataNew gpgme_data_new { get; set; }
        public GPGDataNewFromCBS gpgme_data_new_from_cbs { get; set; }
        public GPGDataNewFromMem gpgme_data_new_from_mem { get; set; }
        public Action<IntPtr> gpgme_data_release { get; set; }
        public Func<IntPtr, gpgme_data_encoding_t, int> gpgme_data_set_encoding { get; set; }
        public Func<IntPtr, IntPtr, int> gpgme_data_set_file_name { get; set; }
        public Func<gpgme_protocol_t, int> gpgme_engine_check_version { get; set; }
        public Func<IntPtr, int> gpgme_get_armor { get; set; }
        public GPGGetEngineInfo gpgme_get_engine_info { get; set; }
        public Func<IntPtr, int> gpgme_get_include_certs { get; set; }
        public GPGGetKey gpgme_get_key { get; set; }
        public Func<IntPtr, gpgme_keylist_mode_t> gpgme_get_keylist_mode { get; set; }
        public Func<gpgme_protocol_t, IntPtr> gpgme_get_protocol_name { get; set; }
        public Func<IntPtr, int> gpgme_get_textmode { get; set; }
        public Func<IntPtr, gpgme_protocol_t> gpgme_get_protocol { get; set; }
        public Func<gpgme_hash_algo_t, IntPtr> gpgme_hash_algo_name { get; set; }
        public Func<int, byte[], UIntPtr, IntPtr> gpgme_io_write { get; set; }
        public Action<IntPtr> gpgme_key_release { get; set; }
        public GPGNew gpgme_new { get; set; }
        public Func<IntPtr, IntPtr, IntPtr, int> gpgme_op_decrypt { get; set; }
        public Func<IntPtr, IntPtr> gpgme_op_decrypt_result { get; set; }
        public Func<IntPtr, IntPtr, IntPtr, int> gpgme_op_decrypt_verify { get; set; }
        public Func<IntPtr, IntPtr, int, int> gpgme_op_delete { get; set; }
        public Func<IntPtr, IntPtr, gpgme_edit_cb_t, IntPtr, IntPtr, int> gpgme_op_edit { get; set; }
        public Func<IntPtr, IntPtr[], gpgme_encrypt_flags_t, IntPtr, IntPtr, int> gpgme_op_encrypt { get; set; }
        public Func<IntPtr, IntPtr> gpgme_op_encrypt_result { get; set; }
        public Func<IntPtr, IntPtr[], gpgme_encrypt_flags_t, IntPtr, IntPtr, int> gpgme_op_encrypt_sign { get; set; }
        public Func<IntPtr, IntPtr, uint, IntPtr, int> gpgme_op_export { get; set; }
        public Func<IntPtr, IntPtr[], uint, IntPtr, int> gpgme_op_export_ext { get; set; }
        public Func<IntPtr, IntPtr, IntPtr, IntPtr, int> gpgme_op_genkey { get; set; }
        public Func<IntPtr, IntPtr> gpgme_op_genkey_result { get; set; }
        public Func<IntPtr, IntPtr, int> gpgme_op_import { get; set; }
        public Func<IntPtr, IntPtr> gpgme_op_import_result { get; set; }
        public Func<IntPtr, int> gpgme_op_keylist_end { get; set; }
        public Func<IntPtr, IntPtr[], int, int, int> gpgme_op_keylist_ext_start { get; set; }
        public GPGOpKeylistNext gpgme_op_keylist_next { get; set; }
        public Func<IntPtr, IntPtr, int, int> gpgme_op_keylist_start { get; set; }
        public Func<IntPtr, IntPtr, IntPtr, gpgme_sig_mode_t, int> gpgme_op_sign { get; set; }
        public Func<IntPtr, IntPtr> gpgme_op_sign_result { get; set; }
        public Func<IntPtr, int> gpgme_op_trustlist_end { get; set; }
        public GPGOpTrustlistNext gpgme_op_trustlist_next { get; set; }
        public Func<IntPtr, IntPtr, int, int> gpgme_op_trustlist_start { get; set; }
        public Func<IntPtr, IntPtr, IntPtr, IntPtr, int> gpgme_op_verify { get; set; }
        public Func<IntPtr, IntPtr> gpgme_op_verify_result { get; set; }
        public Func<gpgme_pubkey_algo_t, IntPtr> gpgme_pubkey_algo_name { get; set; }
        public Action<IntPtr> gpgme_release { get; set; }
        public Action<IntPtr, int> gpgme_set_armor { get; set; }
        public Action<IntPtr, int> gpgme_set_include_certs { get; set; }
        public Func<IntPtr, gpgme_keylist_mode_t, int> gpgme_set_keylist_mode { get; set; }
        public Action<IntPtr, gpgme_passphrase_cb_t, IntPtr> gpgme_set_passphrase_cb { get; set; }
        public Action<IntPtr, int> gpgme_set_textmode { get; set; }
        public Func<IntPtr, gpgme_protocol_t, int> gpgme_set_protocol { get; set; }
        public Func<IntPtr, IntPtr, IntPtr, gpgme_sig_notation_flags_t, int> gpgme_sig_notation_add { get; set; }
        public Action<IntPtr> gpgme_sig_notation_clear { get; set; }
        public Func<IntPtr, IntPtr> gpgme_sig_notation_get { get; set; }
        public Func<IntPtr, IntPtr, int> gpgme_signers_add { get; set; }
        public Action<IntPtr> gpgme_signers_clear { get; set; }
        public Func<IntPtr, int, IntPtr> gpgme_signers_enum { get; set; }
        public Func<int, IntPtr> gpgme_strerror { get; set; }
        public Func<int, IntPtr, UIntPtr, int> gpgme_strerror_r { get; set; }
        public Func<int, IntPtr> gpgme_strsource { get; set; }
        public Action<IntPtr> gpgme_trust_item_unref { get; set; }

        // Delegates that have "out" params
        public delegate int GPGDataNew(out IntPtr r_dh);
        public delegate int GPGDataNewFromCBS(out IntPtr dh, IntPtr cbs, IntPtr handle);
        public delegate int GPGDataNewFromMem(out IntPtr r_dh, IntPtr buffer, UIntPtr size, int copy);
        public delegate int GPGNew(out IntPtr ctd);
        public delegate int GPGGetEngineInfo(out IntPtr engineInfo);
        public delegate int GPGGetKey(IntPtr ctx, IntPtr fpr, out IntPtr r_key, int secret);
        public delegate int GPGOpKeylistNext(IntPtr ctx, out IntPtr r_key);
        public delegate int GPGOpTrustlistNext(IntPtr ctx, out IntPtr r_item);
        public delegate int GPGDataNewFromFilepart1(out IntPtr r_dh, IntPtr fname, IntPtr fp, IntPtr offset, UIntPtr length);
        public delegate int GPGDataNewFromFilepart2(out IntPtr r_dh, IntPtr fname, IntPtr fp, long offset, UIntPtr length);

        // Overloaded functions
        public GPGDataNewFromFilepart1 gpgme_data_new_from_filepart_1 { get; set; }
        public GPGDataNewFromFilepart2 gpgme_data_new_from_filepart_2 { get; set; }
        public Func<IntPtr, byte[], UIntPtr, IntPtr> gpgme_data_read_1 { get; set; }
        public Func<IntPtr, IntPtr, UIntPtr, IntPtr> gpgme_data_read_2 { get; set; }
        public Func<IntPtr, IntPtr, int, IntPtr> gpgme_data_seek_1 { get; set; }
        public Func<IntPtr, long, int, long> gpgme_data_seek_2 { get; set; }
        public Func<IntPtr, byte[], UIntPtr, IntPtr> gpgme_data_write_1 { get; set; }
        public Func<IntPtr, IntPtr, UIntPtr, IntPtr> gpgme_data_write_2 { get; set; }
    }
}
