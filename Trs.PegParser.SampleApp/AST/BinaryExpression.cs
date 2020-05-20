namespace Trs.PegParser.SampleApp.AST
{
    public class BinaryExpression : FunctionBase
    {
        public string Opperator { get; }
        public FunctionBase LeftHandSide { get; }
        public FunctionBase RightHandSide { get; }

        public BinaryExpression(string op, FunctionBase lhs, FunctionBase rhs)
        => (Opperator, LeftHandSide, RightHandSide) = (op, lhs, rhs);
    }
}
