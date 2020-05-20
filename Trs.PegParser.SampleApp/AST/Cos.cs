namespace Trs.PegParser.SampleApp.AST
{
    namespace Trs.PegParser.SampleApp.AST
    {
        public class Cos : FunctionBase
        {
            public object ArgumentException { get; }

            public Cos(FunctionBase arg) => ArgumentException = arg;
        }
    }
}
