namespace Libgpgme
{
    public interface IKeyGenerator
    {
        GenkeyResult GenerateKey(Protocol protocoltype, KeyParameters keyparms);
    }
}