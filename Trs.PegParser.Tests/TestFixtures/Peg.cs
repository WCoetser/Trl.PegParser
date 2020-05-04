namespace Trs.PegParser.Tests.TestFixtures
{
    public static class Peg
    {
        public static PegFacade<TokenNames, ParsingRuleNames, string> Facade()
            => new PegFacade<TokenNames, ParsingRuleNames, string>();
    }
}
