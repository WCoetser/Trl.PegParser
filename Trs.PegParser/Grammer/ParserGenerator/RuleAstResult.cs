using System;

namespace Trs.PegParser.Grammer.ParserGenerator
{
    public class RuleAstResult<TTokenTypeName, TNoneTerminalName, TActionResult> : IParserGeneratorResult
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        public TNoneTerminalName RuleName { get; set; }

        public IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> Operator { get; set; }
    }

}
