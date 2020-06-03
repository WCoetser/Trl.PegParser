using System;
using System.Collections.Generic;

namespace Trl.PegParser.Grammer.Semantics
{
    /// <summary>
    /// This class is used to build a generic AST for parse results. It simply builds an expression
    /// tree of the input and exists to be consumed by other custom semantic actions. Without this
    /// class you will have to create patch through functions for semantic actions that do not have "clean"
    /// AST mappings and serves as intermediate results.
    /// 
    /// Note: <see cref="TActionResult"/> here should inherit from <see cref="GenericPassthroughResult"/>
    /// and should be tagged with an interface of type <see cref="TActionResult"/>.
    /// </summary>
    public abstract class GenericPassthroughResult<TActionResult, TTokenTypeName>
        where TTokenTypeName : Enum
    {
        public TokensMatch<TTokenTypeName> MatchedTokens { get; set; }
        public IReadOnlyList<TActionResult> SubResults { get; set; }
    }
}
