using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Trl.PegParser.Grammer.Semantics;

namespace Trl.PegParser
{
    public class SemanticActionsFacade<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly Dictionary<TTokenTypeName, SemanticAction<TActionResult, TTokenTypeName>> _actionByTerminalToken;

        private readonly Dictionary<TNonTerminalName, SemanticAction<TActionResult, TTokenTypeName>> _actionByNonTerminal;

        private Func<MatchedPegOperator, SemanticAction<TActionResult, TTokenTypeName>> _passthroughFunctionConstructor;

        private SemanticAction<TActionResult, TTokenTypeName> _sequenceAction;
        private SemanticAction<TActionResult, TTokenTypeName> _emptyStringAction;
        private SemanticAction<TActionResult, TTokenTypeName> _optionalAction;
        private SemanticAction<TActionResult, TTokenTypeName> _orderedChoiceAction;
        private SemanticAction<TActionResult, TTokenTypeName> _zeroOrMoreAction;
        private SemanticAction<TActionResult, TTokenTypeName> _oneOrMoreAction;

        public SemanticActionsFacade()
        {
            _actionByTerminalToken = new Dictionary<TTokenTypeName, SemanticAction<TActionResult, TTokenTypeName>>();
            _actionByNonTerminal = new Dictionary<TNonTerminalName, SemanticAction<TActionResult, TTokenTypeName>>();
            _passthroughFunctionConstructor = null;
        }

        public void SetDefaultGenericPassthroughAction<TGenericResult>()
            where TGenericResult : GenericPassthroughResult<TActionResult, TTokenTypeName>, TActionResult, new()
        {
            _passthroughFunctionConstructor = (operatorType) =>
            {
                return (tokensMatch, subResults, matchedPeg) =>
                    new TGenericResult
                    {
                        MatchedTokens = tokensMatch,
                        SubResults = subResults.ToList().AsReadOnly(),
                        MatchedPeg = matchedPeg,
                        MatchedOperator = operatorType
                    };
            };
        }
        
        public SemanticAction<TActionResult, TTokenTypeName> SequenceAction
        {
            get => _sequenceAction ?? _passthroughFunctionConstructor?.Invoke(MatchedPegOperator.Sequence);
            set => _sequenceAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> EmptyStringAction
        {
            get => _emptyStringAction ?? _passthroughFunctionConstructor?.Invoke(MatchedPegOperator.EmptyString);
            set => _emptyStringAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> OptionalAction
        {
            get => _optionalAction ?? _passthroughFunctionConstructor?.Invoke(MatchedPegOperator.Optional);
            set => _optionalAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> OrderedChoiceAction
        {
            get => _orderedChoiceAction ?? _passthroughFunctionConstructor?.Invoke(MatchedPegOperator.OrderedChoice);
            set => _orderedChoiceAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> ZeroOrMoreAction
        {
            get => _zeroOrMoreAction ?? _passthroughFunctionConstructor?.Invoke(MatchedPegOperator.ZeroOrMore);
            set => _zeroOrMoreAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> OneOrMoreAction
        { 
            get => _oneOrMoreAction ?? _passthroughFunctionConstructor?.Invoke(MatchedPegOperator.OneOrMore);
            set => _oneOrMoreAction = value; 
        }

        /// <summary>
        /// This method exists to avoid re-typing all the generic constraints in relation to the 
        /// reset of the PEG parser in this facade. Therefore it just passes the input to the output.
        /// </summary>
        public SemanticAction<TActionResult, TTokenTypeName> SemanticAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => semanticAction;

        public void SetTerminalAction(TTokenTypeName tokenType, SemanticAction<TActionResult, TTokenTypeName> semanticAction)
        {
            if (_actionByTerminalToken.ContainsKey(tokenType))
            {
                throw new Exception($"Terminal action already assigned for {tokenType}");
            }
            _actionByTerminalToken[tokenType] = semanticAction;
        }

        public SemanticAction<TActionResult, TTokenTypeName> GetTerminalAction(TTokenTypeName tokenType)
        {
            return _actionByTerminalToken.TryGetValue(tokenType, out SemanticAction<TActionResult, TTokenTypeName> action) switch {
                true => action,
                _ => _passthroughFunctionConstructor?.Invoke(MatchedPegOperator.Terminal)
            };
        }

        public void SetNonTerminalAction(TNonTerminalName nonTerminalA, SemanticAction<TActionResult, TTokenTypeName> matchAction)
        {
            if (_actionByNonTerminal.ContainsKey(nonTerminalA))
            {
                throw new Exception($"Nonterminal action already assigned for {nonTerminalA}");
            }
            _actionByNonTerminal[nonTerminalA] = matchAction;
        }

        public SemanticAction<TActionResult, TTokenTypeName> GetNonTerminalAction(TNonTerminalName nonTerminal)
        {
            return _actionByNonTerminal.TryGetValue(nonTerminal, out SemanticAction<TActionResult, TTokenTypeName> action) switch
            {
                true => action,
                _ => _passthroughFunctionConstructor?.Invoke(MatchedPegOperator.NonTerminal)
            };
        }
    }
}
