using System;

namespace Trl.PegParser.Grammer.ParserGenerator
{
    public class RuleAstResult<TTokenTypeName, TNonTerminalName, TActionResult> : IParserGeneratorResult
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        public TNonTerminalName RuleName { get; set; }

        public IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> Operator { get; set; }
    }

}
