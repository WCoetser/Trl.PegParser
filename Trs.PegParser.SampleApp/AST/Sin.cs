namespace Trs.PegParser.SampleApp.AST
{
    public class Sin : FunctionBase
    {
        public object ArgumentException { get; }

        public Sin(FunctionBase arg) => ArgumentException = arg;
    }
}
