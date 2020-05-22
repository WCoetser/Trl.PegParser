using Trs.PegParser.Grammer.Semantics;

namespace Trs.PegParser.SampleApp.AST
{
    public class GenericResult 
        : GenericPassthroughResult<ICalculatorAstNode, TokensNames>, ICalculatorAstNode
    {
    }
}
