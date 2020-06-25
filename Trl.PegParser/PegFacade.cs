using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Trl.PegParser.Grammer;
using Trl.PegParser.Grammer.ParserGenerator;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser
{
    /// <summary>
    /// Facade for creating PEG parser, tying everything together.
    /// 
    /// NB: The purpose of this class is to avoid typing all the generic constraints repeatedly over and over.
    /// 
    /// </summary>
    /// <typeparam name="TTokenTypeName">Enum to name tokens.</typeparam>
    /// <typeparam name="TNonTerminalName">Enum used to name non-terminals and parsing rule heads.</typeparam>
    /// <typeparam name="TActionResult">Result of evaluation a match, ex. numbers for a calculator or AST node type for a compiler.</typeparam>
    public class PegFacade<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        public SemanticActionsFacade<TTokenTypeName, TNonTerminalName, TActionResult> DefaultSemanticActions { get; }
        public OperatorFacade<TTokenTypeName, TNonTerminalName, TActionResult> Operators { get; }
                       
        public PegFacade()
        {
            DefaultSemanticActions = new SemanticActionsFacade<TTokenTypeName, TNonTerminalName, TActionResult>();
            Operators = new OperatorFacade<TTokenTypeName, TNonTerminalName, TActionResult>(DefaultSemanticActions);
        }
        
        public Generator<TTokenTypeName, TNonTerminalName, TActionResult> ParserGenerator
            => new Generator<TTokenTypeName, TNonTerminalName, TActionResult>(this);

        public TokenDefinition<TTokenTypeName> Token(TTokenTypeName tokenName, Regex tokenDefinition)
        => new TokenDefinition<TTokenTypeName>(tokenName, tokenDefinition);

        public Tokenizer<TTokenTypeName> Tokenizer(IEnumerable<TokenDefinition<TTokenTypeName>> prioritizedTokenDefinitions)
        => new Tokenizer<TTokenTypeName>(prioritizedTokenDefinitions);

        public Parser<TTokenTypeName, TNonTerminalName, TActionResult> Parser(TNonTerminalName startSymbol,
            IEnumerable<ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>> grammerRules)
        {
            var semanticAction = DefaultSemanticActions.GetNonTerminalAction(startSymbol);
            if (semanticAction == default)
            {
                semanticAction = DefaultSemanticActions.SemanticAction((_, subResults, matchedPeg) => subResults.FirstOrDefault());
            }
            return new Parser<TTokenTypeName, TNonTerminalName, TActionResult>(Operators.NonTerminal(startSymbol, semanticAction), grammerRules);
        }

        public ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult> Rule(TNonTerminalName ruleHead,
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> ruleBody)
        => new ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>(ruleHead, ruleBody);
    }
}
