using System;

namespace Trs.PegParser.Grammer
{
    /// <summary>
    /// Defines a non-terminal.
    /// </summary>
    /// <typeparam name="TNonTerminalName">An enumeration used to identity non-terminals across the PEG.</typeparam>
    class NonTerminal<TNonTerminalName>
        where TNonTerminalName : Enum
    {
    }
}
