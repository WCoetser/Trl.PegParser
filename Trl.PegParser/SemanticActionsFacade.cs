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

        private SemanticAction<TActionResult, TTokenTypeName> _passthroughFunction;
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
            _passthroughFunction = null;
        }

        public void SetDefaultGenericPassthroughAction<TGenericResult>()
            where TGenericResult : GenericPassthroughResult<TActionResult, TTokenTypeName>, TActionResult, new()
        {
            _passthroughFunction = 
                (tokensMatch, subResults, matchedPeg) => 
                    new TGenericResult 
                    { 
                        MatchedTokens = tokensMatch, 
                        SubResults = subResults.ToList().AsReadOnly(),
                        MatchedPeg = matchedPeg
                    };
        }
        
        public SemanticAction<TActionResult, TTokenTypeName> SequenceAction
        {
            get => _sequenceAction ?? _passthroughFunction;
            set => _sequenceAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> EmptyStringAction
        {
            get => _emptyStringAction ?? _passthroughFunction;
            set => _emptyStringAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> OptionalAction
        {
            get => _optionalAction ?? _passthroughFunction;
            set => _optionalAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> OrderedChoiceAction
        {
            get => _orderedChoiceAction ?? _passthroughFunction;
            set => _orderedChoiceAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> ZeroOrMoreAction
        {
            get => _zeroOrMoreAction ?? _passthroughFunction;
            set => _zeroOrMoreAction = value;
        }

        public SemanticAction<TActionResult, TTokenTypeName> OneOrMoreAction
        { 
            get => _oneOrMoreAction ?? _passthroughFunction; 
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
            => _actionByTerminalToken.TryGetValue(tokenType, out SemanticAction<TActionResult, TTokenTypeName> action) ? action : _passthroughFunction;

        public void SetNonTerminalAction(TNonTerminalName nonTerminalA, SemanticAction<TActionResult, TTokenTypeName> matchAction)
        {
            if (_actionByNonTerminal.ContainsKey(nonTerminalA))
            {
                throw new Exception($"Nonterminal action already assigned for {nonTerminalA}");
            }
            _actionByNonTerminal[nonTerminalA] = matchAction;
        }

        public SemanticAction<TActionResult, TTokenTypeName> GetNonTerminalAction(TNonTerminalName nonTerminal)
        => _actionByNonTerminal.TryGetValue(nonTerminal, out SemanticAction<TActionResult, TTokenTypeName> action) ? action : _passthroughFunction;
    }
}
