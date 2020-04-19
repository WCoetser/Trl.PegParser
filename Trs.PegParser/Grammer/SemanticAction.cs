using System;
using System.Collections.Generic;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer
{
    /// <summary>
    /// This is the definition of a function type for functions that are called from PEG operators to pass parsing 
    /// results back to the program using the parser.
    /// It could for example be used to specify a function creating an abstract syntax tree for a 
    /// compiler, or a the implementation of operators in a calculator.
    /// </summary>
    /// <typeparam name="TActionResult">The type used to specify the results of semantic actions. This could for example be 
    /// abstract syntax tree nodes or numbers for a calculator.</typeparam>
    /// <param name="matchedTokenRange">The index range of tokens matched.</param>
    /// <param name="inputTokens">The input tokens from the tokenizer.</param>
    /// <param name="subActionResults">Subresults from child semantic actions.</param>
    /// <returns>Results that can be used in parent semantic actions.</returns>
    public delegate TActionResult SemanticAction<TActionResult, TTokenTypeName>(MatchRange matchedTokenRange, 
            IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens,
            TActionResult[] subActionResults)
        where TTokenTypeName: Enum;
}
