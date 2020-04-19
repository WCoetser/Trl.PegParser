using System;
using System.Collections.Generic;
using System.Text;

namespace Trs.PegParser.Grammer
{
    public interface IParsingOperatorValidation<TNoneTerminalName>
        where TNoneTerminalName: Enum
    {
        IEnumerable<TNoneTerminalName> GetNonTerminalNames();
    }
}
