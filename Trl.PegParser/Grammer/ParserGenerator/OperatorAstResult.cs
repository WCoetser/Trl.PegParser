using System;

namespace Trl.PegParser.Grammer.ParserGenerator
{
    public class OperatorAstResult<TTokenTypeName, TNoneTerminalName, TActionResult> : IParserGeneratorResult
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
        public IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult> Operator { get; set; }
    }
}
