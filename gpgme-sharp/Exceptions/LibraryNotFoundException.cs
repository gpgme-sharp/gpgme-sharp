using System.Collections.Generic;

namespace Libgpgme;

public class LibraryNotFoundException : GpgmeException
{
    public LibraryNotFoundException(string dllName, IEnumerable<string> potentialPaths)
    : base(BuildMessage(dllName, potentialPaths)) {}

    private static string BuildMessage(string dllName, IEnumerable<string> potentialPaths)
    {
        var paths = string.Join("\n", potentialPaths);
        return $"Could not find {dllName}. Tried the following paths:\n\n{paths}";
    }
}