namespace Trl.PegParser.Tests.TestFixtures
{
    public static class Peg
    {
        public static PegFacade<TokenNames, ParsingRuleNames, string> Facade()
            => new PegFacade<TokenNames, ParsingRuleNames, string>();

        public static PegFacade<TokenNames, ParsingRuleNames, IAstResult> GenericPassthroughTest()
            => new PegFacade<TokenNames, ParsingRuleNames, IAstResult>();
    }
}
