namespace Trl.PegParser.Grammer.Semantics
{
    public enum MatchedPegOperator
    {
        None,
        Sequence,
        EmptyString,
        Optional,
        OrderedChoice,
        ZeroOrMore,
        OneOrMore,
        Terminal,
        NonTerminal,
        NotPredicate,
        AndPredicate
    }
}
