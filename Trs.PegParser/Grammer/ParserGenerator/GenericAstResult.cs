using Trs.PegParser.Grammer.Semantics;

namespace Trs.PegParser.Grammer.ParserGenerator
{
    public class GenericAstResult :
        GenericPassthroughResult<IParserGeneratorResult, TokenNames>, IParserGeneratorResult
    {
        public override string ToString()
        {
            return MatchedTokens.GetMatchedString();
        }
    }
}
