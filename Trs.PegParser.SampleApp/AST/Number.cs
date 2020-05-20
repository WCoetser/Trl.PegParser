namespace Trs.PegParser.SampleApp.AST
{
    public class Number : FunctionBase
    {
        public double Value { get; }

        public Number(string inputValue) 
        => Value = double.Parse(inputValue);
    }
}
