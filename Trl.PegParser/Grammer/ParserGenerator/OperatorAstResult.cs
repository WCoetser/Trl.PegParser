using System;

namespace Trl.PegParser.Grammer.ParserGenerator
{
    public class OperatorAstResult<TTokenTypeName, TNonTerminalName, TActionResult> : IParserGeneratorResult
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        public IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> Operator { get; set; }
    }
}
