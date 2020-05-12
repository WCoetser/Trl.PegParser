using Trs.PegParser.Grammer;

namespace Trs.PegParser.SampleApp
{
    public enum ParsingRuleNames
    {
        Start,
        Statement,
        Function,
        InputDomain,
        Brackets,
        Sin,
        Cos,
        Add,
        Subtract,
        Multiply,
        Divide,
        Variable_X,
        Number
    }
    
    public static class ParsingRuleDefinitions
    {       
        public static Parser<TokensNames, ParsingRuleNames, IParseResult> GetParser(PegFacade<TokensNames, ParsingRuleNames, IParseResult> pegFacade)
        {
            // Operators are seperated into a seperate "mini facade" to avoid too many 
            // method definitions in one class
            var op = pegFacade.Operators;

            // Semantics are defined through callback functions. These could be on operators themselfes,
            // or could be default actions defined per input token type, operator, or parsing rule name.
            // The idea with default actions is to clean up the parsing rule definitions as much as possible.
            // It is possible to first define and test the parser, and then add the semantics later.


            // A parser is defined by the start symbol and a collection of parsing rules
            return pegFacade.Parser(ParsingRuleNames.Start, new[] { 
                
                // Parse input into semicolon seperated statements
                pegFacade.Rule(ParsingRuleNames.Start, op.OneOrMore(op.NonTerminal(ParsingRuleNames.Statement))),

                // A statement is a function or a domain (of input values)
                pegFacade.Rule(ParsingRuleNames.Statement,
                    op.Sequence(op.OrderedChoice(op.NonTerminal(ParsingRuleNames.Function),
                                                 op.NonTerminal(ParsingRuleNames.InputDomain)),
                                op.Terminal(TokensNames.Semicolon))),

                // A function domain gives the input values to be graphed
                pegFacade.Rule(ParsingRuleNames.InputDomain,
                    op.Sequence(op.Terminal(TokensNames.Domain),
                                op.Terminal(TokensNames.OpenRoundBracket),
                                op.Terminal(TokensNames.Number),
                                op.Terminal(TokensNames.Comma),
                                op.Terminal(TokensNames.Number),
                                op.Terminal(TokensNames.CloseRoundBracket))),

                // A function consists of operators on functions and numbers
                pegFacade.Rule(ParsingRuleNames.Function,
                    op.OrderedChoice(
                        // Order is important here 
                        op.NonTerminal(ParsingRuleNames.Number),                        
                        op.NonTerminal(ParsingRuleNames.Variable_X),
                        op.NonTerminal(ParsingRuleNames.Sin),
                        op.NonTerminal(ParsingRuleNames.Cos),
                        op.NonTerminal(ParsingRuleNames.Multiply),
                        op.NonTerminal(ParsingRuleNames.Divide),
                        op.NonTerminal(ParsingRuleNames.Add),
                        op.NonTerminal(ParsingRuleNames.Subtract),
                        op.NonTerminal(ParsingRuleNames.Brackets))),

                // Brackets for grouping
                pegFacade.Rule(ParsingRuleNames.Brackets,
                    op.Sequence(op.Terminal(TokensNames.OpenRoundBracket),
                                op.NonTerminal(ParsingRuleNames.Function),
                                op.Terminal(TokensNames.CloseRoundBracket))),

                // Sine
                pegFacade.Rule(ParsingRuleNames.Sin,
                    op.Sequence(op.Terminal(TokensNames.Sin),
                                op.Terminal(TokensNames.OpenRoundBracket),
                                op.NonTerminal(ParsingRuleNames.Function),
                                op.Terminal(TokensNames.CloseRoundBracket))),

                // Cosine
                pegFacade.Rule(ParsingRuleNames.Cos,
                    op.Sequence(op.Terminal(TokensNames.Cos),
                                op.Terminal(TokensNames.OpenRoundBracket),
                                op.NonTerminal(ParsingRuleNames.Function),
                                op.Terminal(TokensNames.CloseRoundBracket))),

                // Multiply
                pegFacade.Rule(ParsingRuleNames.Multiply,
                    op.Sequence(op.NonTerminal(ParsingRuleNames.Function),
                                op.Terminal(TokensNames.Multiply),
                                op.NonTerminal(ParsingRuleNames.Function))),

                // Divide
                pegFacade.Rule(ParsingRuleNames.Divide,
                    op.Sequence(op.NonTerminal(ParsingRuleNames.Function),
                                op.Terminal(TokensNames.Divide),
                                op.NonTerminal(ParsingRuleNames.Function))),

                // Add
                pegFacade.Rule(ParsingRuleNames.Add,
                    op.Sequence(op.NonTerminal(ParsingRuleNames.Function),
                                op.Terminal(TokensNames.Plus),
                                op.NonTerminal(ParsingRuleNames.Function))),

                // Subtract
                pegFacade.Rule(ParsingRuleNames.Subtract,
                    op.Sequence(op.NonTerminal(ParsingRuleNames.Function),
                                op.Terminal(TokensNames.Minus),
                                op.NonTerminal(ParsingRuleNames.Function))),

                // Subtract
                pegFacade.Rule(ParsingRuleNames.Variable_X, op.Terminal(TokensNames.X)),

                // Number
                pegFacade.Rule(ParsingRuleNames.Number, op.Terminal(TokensNames.Number))

            });
        }
    }
}
