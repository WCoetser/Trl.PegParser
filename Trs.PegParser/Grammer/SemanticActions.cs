using System;
using System.Collections.Generic;
using Trs.PegParser.Grammer;

namespace Trs.PegParser.Grammer
{
    public class SemanticActions<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private Dictionary<TTokenTypeName, SemanticAction<TActionResult, TTokenTypeName>> _actionByTerminalToken;            

        private Dictionary<TNonTerminalName, SemanticAction<TActionResult, TTokenTypeName>> _actionByNonTerminal;            

        public SemanticActions()
        {
            _actionByTerminalToken = new Dictionary<TTokenTypeName, SemanticAction<TActionResult, TTokenTypeName>>();
            _actionByNonTerminal  = new Dictionary<TNonTerminalName, SemanticAction<TActionResult, TTokenTypeName>>();
        }

        public SemanticAction<TActionResult, TTokenTypeName> SequenceAction { get; set; }
        public SemanticAction<TActionResult, TTokenTypeName> EmptyStringAction { get; set; }
        public SemanticAction<TActionResult, TTokenTypeName> OptionalAction { get; set; }
        public SemanticAction<TActionResult, TTokenTypeName> OrderedChoiceAction { get; set; }
        public SemanticAction<TActionResult, TTokenTypeName> ZeroOrMoreAction { get; set; }
        public SemanticAction<TActionResult, TTokenTypeName> OneOrMoreAction { get; set; }

        /// <summary>
        /// This method exists to avoid re-typing all the generic constraints in relation to the 
        /// reset of the PEG parser in this facade. Therefore it just passes the input to the output.
        /// </summary>
        public SemanticAction<TActionResult, TTokenTypeName> SemanticAction(SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => semanticAction;

        public void SetTerminalAction(TTokenTypeName tokenType, SemanticAction<TActionResult, TTokenTypeName> semanticAction)
            => _actionByTerminalToken[tokenType] = semanticAction;

        public SemanticAction<TActionResult, TTokenTypeName> GetTerminalAction(TTokenTypeName tokenType)
            => _actionByTerminalToken.TryGetValue(tokenType, out SemanticAction<TActionResult, TTokenTypeName> action) ? action : null;

        public void SetNonTerminalAction(TNonTerminalName nonTerminalA, SemanticAction<TActionResult, TTokenTypeName> matchAction)
        => _actionByNonTerminal[nonTerminalA] = matchAction;

        public SemanticAction<TActionResult, TTokenTypeName> GetNonTerminalAction(TNonTerminalName nonTerminal)
        => _actionByNonTerminal.TryGetValue(nonTerminal, out SemanticAction<TActionResult, TTokenTypeName> action) ? action : null;
    }
}
