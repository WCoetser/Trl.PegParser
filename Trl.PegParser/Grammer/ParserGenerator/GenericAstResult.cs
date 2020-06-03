using Trl.PegParser.Grammer.Semantics;

namespace Trl.PegParser.Grammer.ParserGenerator
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
