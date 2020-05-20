namespace Trs.PegParser.SampleApp.AST
{
    public class BinaryOperator : ICalculatorAstNode
    {
        public BinaryOperator(string op)
            => Op = op;

        public string Op { get; }
    }
}
