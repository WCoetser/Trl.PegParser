using System;
using System.Collections.Generic;
using Trs.PegParser.Grammer;
using Trs.PegParser.Grammer.Operators;

namespace Trs.PegParser
{
    public class OperatorFacade<TTokenTypeName, TNonTerminalName, TActionResult>
        where TTokenTypeName : Enum
        where TNonTerminalName : Enum
    {
        private readonly SemanticActions<TTokenTypeName, TNonTerminalName, TActionResult> _defaultSemanticActions;

        internal OperatorFacade(SemanticActions<TTokenTypeName, TNonTerminalName, TActionResult> defaultSemanticActions)
        {
            _defaultSemanticActions = defaultSemanticActions;
        }

        public OneOrMore<TTokenTypeName, TNonTerminalName, TActionResult> OneOrMore(
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression, SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultSemanticActions.OneOrMoreAction;
            return new OneOrMore<TTokenTypeName, TNonTerminalName, TActionResult>(subExpression, matchAction);
        }

        public NotPredicate<TTokenTypeName, TNonTerminalName, TActionResult> NotPredicate(
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression)
        => new NotPredicate<TTokenTypeName, TNonTerminalName, TActionResult>(subExpression);

        public AndPredicate<TTokenTypeName, TNonTerminalName, TActionResult> AndPredicate(
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression)
        => new AndPredicate<TTokenTypeName, TNonTerminalName, TActionResult>(subExpression);

        public ZeroOrMore<TTokenTypeName, TNonTerminalName, TActionResult> ZeroOrMore(
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression, SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultSemanticActions.ZeroOrMoreAction;
            return new ZeroOrMore<TTokenTypeName, TNonTerminalName, TActionResult>(subExpression, matchAction);
        }

        public Sequence<TTokenTypeName, TNonTerminalName, TActionResult> Sequence(
            params IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>[] sequenceDefinitions)
        => Sequence(sequenceDefinitions, null);

        public Sequence<TTokenTypeName, TNonTerminalName, TActionResult> Sequence(
            IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> sequenceDefinitions,
            SemanticAction<TActionResult, TTokenTypeName> matchAction = null)
        => new Sequence<TTokenTypeName, TNonTerminalName, TActionResult>(sequenceDefinitions, matchAction ?? _defaultSemanticActions.SequenceAction);

        public NonTerminal<TTokenTypeName, TNonTerminalName, TActionResult> NonTerminal(TNonTerminalName head,
            SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var action = semanticAction ?? _defaultSemanticActions.GetNonTerminalAction(head);
            return new NonTerminal<TTokenTypeName, TNonTerminalName, TActionResult>(head, action);
        }

        public EmptyString<TTokenTypeName, TNonTerminalName, TActionResult> EmptyString(
            SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultSemanticActions.EmptyStringAction;
            return new EmptyString<TTokenTypeName, TNonTerminalName, TActionResult>(matchAction);
        }

        public Terminal<TTokenTypeName, TNonTerminalName, TActionResult> Terminal(
            TTokenTypeName expectedToken, SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultSemanticActions.GetTerminalAction(expectedToken);
            return new Terminal<TTokenTypeName, TNonTerminalName, TActionResult>(expectedToken, matchAction);
        }

        public Optional<TTokenTypeName, TNonTerminalName, TActionResult> Optional(
            IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult> subExpression,
            SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultSemanticActions.OptionalAction;
            return new Optional<TTokenTypeName, TNonTerminalName, TActionResult>(subExpression, matchAction);
        }

        public OrderedChoice<TTokenTypeName, TNonTerminalName, TActionResult> OrderedChoice(
            IEnumerable<IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>> orderedChoices,
            SemanticAction<TActionResult, TTokenTypeName> semanticAction = null)
        {
            var matchAction = semanticAction ?? _defaultSemanticActions.OrderedChoiceAction;
            return new OrderedChoice<TTokenTypeName, TNonTerminalName, TActionResult>(orderedChoices, matchAction);
        }

        public OrderedChoice<TTokenTypeName, TNonTerminalName, TActionResult> OrderedChoice(
            params IParsingOperator<TTokenTypeName, TNonTerminalName, TActionResult>[] orderedChoices)
        => OrderedChoice(orderedChoices, null);

    }
}
