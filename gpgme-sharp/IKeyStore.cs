namespace Libgpgme
{
    public interface IKeyStore
    {
        ImportResult Import(GpgmeData keydata);

        void Export(string pattern, GpgmeData keydata, ExportMode? mode = null);
        void Export(string[] pattern, GpgmeData keydata, ExportMode? mode = null);

        Key GetKey(string fpr, bool secretOnly);
        Key[] GetKeyList(string pattern, bool secretOnly);
        Key[] GetKeyList(string[] pattern, bool secretOnly);
        void DeleteKey(Key key, bool deleteSecret);
    }
}