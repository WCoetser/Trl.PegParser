using System;

namespace Trs.PegParser.Grammer
{
    public interface IParsingOperator<TTokenTypeName, TNoneTerminalName, TActionResult>
        :   IParsingOperatorExecution<TTokenTypeName, TActionResult>, 
            IParsingOperatorValidation<TNoneTerminalName>
        where TTokenTypeName : Enum
        where TNoneTerminalName : Enum
    {
    }
}
