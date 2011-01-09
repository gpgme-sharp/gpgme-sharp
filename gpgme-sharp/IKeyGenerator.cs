using System;
using System.Collections.Generic;
using System.Text;

namespace Libgpgme
{
    public interface IKeyGenerator
    {
        GenkeyResult GenerateKey(Protocol protocoltype, KeyParameters keyparms);
    }
}
