using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.SampleApp.AST;
using Trs.PegParser.SampleApp.AST.Trs.PegParser.SampleApp.AST;

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
        BinaryExpression,
        Variable_X,
        Number
    }
    
    public static class ParsingRuleDefinitions
    {       
        public static Parser<TokensNames, ParsingRuleNames, ICalculatorAstNode> GetParser(PegFacade<TokensNames, ParsingRuleNames, ICalculatorAstNode> pegFacade)
        {
            // Operators are seperated into a seperate "mini facade" to avoid too many 
            // method definitions in one class
            var op = pegFacade.Operators;

            // Semantics are defined through callback functions. These could be on operators themselfes,
            // or could be default actions defined per input token type, operator, or parsing rule name.
            // The idea with default actions is to clean up the parsing rule definitions as much as possible.
            // It is possible to first define and test the parser, and then add the semantics later.
            var defaultSemantics = pegFacade.DefaultSemanticActions;

            // Set generic passthrough type to avoid coding boilerplate
            defaultSemantics.SetDefaultGenericPassthroughAction<GenericResult>();

            // Function
            defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Function, (_, subResult) => subResult.First());

            // Number
            defaultSemantics.SetTerminalAction(TokensNames.Number, (matchResult, _) => new Number(matchResult.GetMatchedString()));
            defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Number, (_, subResult) => subResult.First());
            
            // Variable
            defaultSemantics.SetTerminalAction(TokensNames.X, (matchResult, _) => new Variable());
            defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Number, (_, subResult) => subResult.First());
            
            // Binary expression
            defaultSemantics.SetNonTerminalAction(ParsingRuleNames.BinaryExpression, (matchResult, subActionResults) =>
            {
                var subResultsList = subActionResults.ToList();
                var op = (GenericResult)subResultsList[1];
                var lhs = (FunctionBase)subResultsList[0];
                var rhs = (FunctionBase)subResultsList[2];
                return new BinaryExpression(op.MatchedTokens.GetMatchedString(), lhs, rhs);
            });

            // Cosine
            defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Cos, 
                (matchResult, subActionResults) => new Cos((FunctionBase)subActionResults.Skip(2).First()));

            // Sine
            defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Sin,
                (matchResult, subActionResults) => new Sin((FunctionBase)subActionResults.Skip(2).First()));

            // Brackets

            // Input domain

            // Statement

            // Start

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
                        op.NonTerminal(ParsingRuleNames.BinaryExpression),
                        // If these are specified first, parsing will stop after (for example) 
                        // matching a number
                        op.NonTerminal(ParsingRuleNames.Number),
                        op.NonTerminal(ParsingRuleNames.Variable_X),
                        op.NonTerminal(ParsingRuleNames.Sin),
                        op.NonTerminal(ParsingRuleNames.Cos),                        
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

                // Binary expression
                pegFacade.Rule(ParsingRuleNames.BinaryExpression,
                    op.Sequence(op.NonTerminal(ParsingRuleNames.Function),
                                op.OrderedChoice(op.Terminal(TokensNames.Multiply), 
                                                 op.Terminal(TokensNames.Divide), 
                                                 op.Terminal(TokensNames.Plus), 
                                                 op.Terminal(TokensNames.Minus)),
                                op.NonTerminal(ParsingRuleNames.Function))),

                // Variable
                pegFacade.Rule(ParsingRuleNames.Variable_X, op.Terminal(TokensNames.X)),

                // Number
                pegFacade.Rule(ParsingRuleNames.Number, op.Terminal(TokensNames.Number))

            });
        }
    }
}
