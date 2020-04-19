namespace Trs.PegParser.Grammer
{
    public class ParseResult<TActionResult>
    {
        /// <summary>
        /// Specifies whether parsing succeeded.
        /// </summary>
        public bool Succeed;

        /// <summary>
        /// Where to parse from next.
        /// </summary>
        public int? NextParsePosition;
        
        /// <summary>
        /// The result of the semantic actions carried out when this match took place.
        /// </summary>
        public TActionResult SemanticActionResult;
    }
}
