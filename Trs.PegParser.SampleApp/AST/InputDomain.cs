namespace Trs.PegParser.SampleApp.AST
{
    public class InputDomain : ICalculatorAstNode
    {
        public Number From { get; }
        public Number To { get; }

        public InputDomain(Number from, Number to)
        => (From, To) = (from, to);
    }
}
