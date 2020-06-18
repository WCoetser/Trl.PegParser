using System;
using System.Collections.Generic;

namespace Trl.PegParser.Grammer.ParserGenerator
{
    public class RuleCollectionResult<TTokenTypeName, TNonTerminalName, TActionResult> : IParserGeneratorResult
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        public IEnumerable<ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>> Rules { get; set; }
    }
}
