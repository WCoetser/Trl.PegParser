namespace Trs.PegParser.SampleApp.AST
{
    public class Calculator : ICalculatorAstNode
    {
        public Calculator(FunctionBase function, InputDomain domain)
            => (Function, Domain) = (function, domain);

        public FunctionBase Function { get; }
        public InputDomain Domain { get; }
    }
}
