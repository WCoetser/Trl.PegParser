using System;
using System.Collections.Generic;

namespace Trs.PegParser.Grammer.ParserGenerator
{
    public class RuleCollectionResult<TTokenTypeName, TNoneTerminalName, TActionResult> : IParserGeneratorResult
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        public IEnumerable<ParsingRule<TTokenTypeName, TNoneTerminalName, TActionResult>> Rules { get; set; }
    }
}
