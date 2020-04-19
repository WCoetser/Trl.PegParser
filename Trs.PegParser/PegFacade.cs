using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trs.PegParser.Grammer;
using Trs.PegParser.Grammer.Operators;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser
{
    /// <summary>
    /// Facade for creating PEG parser.
    /// 
    /// NB: The purpose of this class is to avoid typing all the generic constraints repeatedly over and over.
    /// 
    /// </summary>
    public class PegFacade<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        public Tokenizer<TTokenTypeName> Tokenizer(IEnumerable<TokenDefinition<TTokenTypeName>> prioritizedTokenDefinitions)
        => new Tokenizer<TTokenTypeName>(prioritizedTokenDefinitions);

        public Parser<TTokenTypeName, TNonTerminalName, TActionResult> Parser(TNonTerminalName startSymbol,
            IEnumerable<ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>> grammerRules)
        => new Parser<TTokenTypeName, TNonTerminalName, TActionResult>(startSymbol, grammerRules);

        public string GetStringValue(MatchRange matchedTokenRange, IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, string inputString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = matchedTokenRange.StartIndex; i < matchedTokenRange.Length; i++)
            {
                var substringRange = inputTokens[i].MatchedCharacterRange;
                sb.Append(inputString.Substring(substringRange.StartIndex, substringRange.Length));
            }
            return sb.ToString();
        }

        public SemanticAction<TActionResult, TTokenTypeName> SemanticAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => semanticAction;

        public ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult> Rule(TNonTerminalName ruleHead,
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> ruleBody)
        => new ParsingRule<TTokenTypeName, TNonTerminalName, TActionResult>(ruleHead, ruleBody);

        public Terminal<TTokenTypeName, TNonTerminalName, TActionResult> Terminal(
            TTokenTypeName expectedToken, SemanticAction<TActionResult, TTokenTypeName> matchAction)
        => new Terminal<TTokenTypeName, TNonTerminalName, TActionResult>(expectedToken, matchAction);
    }
}
