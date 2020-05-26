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
            //// Semantics are defined through callback functions. These could be on operators themselfes,
            //// or could be default actions defined per input token type, operator, or parsing rule name.
            //// The idea with default actions is to clean up the parsing rule definitions as much as possible.
            //// It is possible to first define and test the parser, and then add the semantics later.
            //var defaultSemantics = pegFacade.DefaultSemanticActions;

            //// Set generic passthrough type to avoid coding boilerplate
            //defaultSemantics.SetDefaultGenericPassthroughAction<GenericResult>();

            //// Function
            //defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Function, (_, subResult) => subResult.First());

            //// Number
            //defaultSemantics.SetTerminalAction(TokensNames.Number, (matchResult, _) => new Number(matchResult.GetMatchedString()));            
            //defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Number, (_, subResult) => subResult.First());
            
            //// Variable
            //defaultSemantics.SetTerminalAction(TokensNames.X, (matchResult, _) => new Variable());
            
            //// Binary expression
            //defaultSemantics.SetNonTerminalAction(ParsingRuleNames.BinaryExpression, (matchResult, subActionResults) =>
            //{
            //    var subResultsList = subActionResults.ToList();
            //    var op = (GenericResult)subResultsList[1];
            //    var lhs = (FunctionBase)subResultsList[0];
            //    var rhs = (FunctionBase)subResultsList[2];
            //    return new BinaryExpression(op.MatchedTokens.GetMatchedString(), lhs, rhs);
            //});

            //// Cosine
            //defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Cos, 
            //    (matchResult, subActionResults) => new Cos((FunctionBase)subActionResults.Skip(2).First()));

            //// Sine
            //defaultSemantics.SetNonTerminalAction(ParsingRuleNames.Sin,
            //    (matchResult, subActionResults) => new Sin((FunctionBase)subActionResults.Skip(2).First()));

            //// Brackets

            //// Input domain

            //// Statement

            //// Start

            const string rulesStr = @"
Start => Statement+;
Statement => (Function | InputDomain) [Semicolon];
InputDomain => [Domain] [OpenRoundBracket] [Number] [Comma] [Number] [CloseRoundBracket];
Function => BinaryExpression | Number | Variable_X | Sin | Cos | Brackets;
Brackets => [OpenRoundBracket] Function [CloseRoundBracket];
Sin => [Sin] [OpenRoundBracket] Function [CloseRoundBracket];
Cos => [Cos] [OpenRoundBracket] Function [CloseRoundBracket];
BinaryExpression => Function ([Multiply] | [Divide] | [Plus] | [Minus]) Function;
Variable_X => [X];
Number => [Number];
";
            var rules = pegFacade.ParserGenerator.GetParsingRules(rulesStr);
            return pegFacade.Parser(ParsingRuleNames.Start, rules);
        }
    }
}
