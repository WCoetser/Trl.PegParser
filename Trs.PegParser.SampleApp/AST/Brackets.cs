namespace Trs.PegParser.SampleApp.AST
{
    public class Brackets : FunctionBase
    {
        public FunctionBase Argument { get; }

        public Brackets(FunctionBase arg) => Argument = arg;
    }
}
