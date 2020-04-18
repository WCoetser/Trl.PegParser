using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Trs.PegParser.Tokenization
{
    public class Tokenizer<TTokenName>
        where TTokenName: Enum
    {        
        /// <summary>
        /// Tokenizes the input string based on the given definitions.
        /// </summary>
        /// <param name="inputString">String to be tokenized.</param>
        /// <param name="prioritizedTokenDefinitions">The token definitions in order of importance. It is possible to
        /// create overlapping definitions with regular expression. The tokens are mapped first to last to get arround this.</param>
        /// <returns>The result of tokenization. Tokens are assaigned names from <see cref="TTokenName"/></returns>
        public TokenizationResult<TTokenName> Tokenize(string inputString, 
            IEnumerable<TokenDefinition<TTokenName>> prioritizedTokenDefinitions)
        {            
            _ = inputString ?? throw new ArgumentException("Expected input string", nameof(inputString));

            if (prioritizedTokenDefinitions == null || !prioritizedTokenDefinitions.Any())
            {
                throw new ArgumentException("Expected at least one token definition.", nameof(prioritizedTokenDefinitions));
            }

            var matches = GetMatches(inputString, prioritizedTokenDefinitions);
            var mismatches = GetMismatches(matches, inputString.Length);
            return new TokenizationResult<TTokenName>(matches.Select(m => m.Value).ToList().AsReadOnly(), mismatches.AsReadOnly());
        }

        private static List<StringMatchRange> GetMismatches(SortedDictionary<int, TokenMatch<TTokenName>> matches, int inputStringLength)
        {
            if (matches.Count == 0)
            {
                return new List<StringMatchRange>
                {
                    new StringMatchRange(0, inputStringLength)
                };
            }

            var mismatches = new List<StringMatchRange>();
            int currentIndex = 0;
            foreach (var match in matches.Select(m => m.Value))
            {
                if (match.MatchedCharacterRange.StartIndex != currentIndex)
                {
                    mismatches.Add(new StringMatchRange(currentIndex, match.MatchedCharacterRange.StartIndex - currentIndex));
                    currentIndex = match.MatchedCharacterRange.StartIndex + match.MatchedCharacterRange.Length;
                }
                else
                {
                    currentIndex += match.MatchedCharacterRange.Length;
                }
            }            
            if (currentIndex != inputStringLength)
            {
                mismatches.Add(new StringMatchRange(currentIndex, inputStringLength - currentIndex));
            }
            return mismatches;
        }

        private static SortedDictionary<int, TokenMatch<TTokenName>> GetMatches(string inputString, IEnumerable<TokenDefinition<TTokenName>> prioritizedTokenDefinitions)
        {
            var orderedMatches = new SortedDictionary<int, TokenMatch<TTokenName>>(); // mapping from match start index to match
            foreach (var definition in prioritizedTokenDefinitions)
            {
                var regexMatches = definition.DefiningRegex.Matches(inputString);
                foreach (Match match in regexMatches)
                {
                    // Contains check in case a higher priority regex/token def has been matched before this one
                    if (match.Success && !orderedMatches.ContainsKey(match.Index))
                    {
                        var tokenMatch = new TokenMatch<TTokenName>(definition.Name, new StringMatchRange(match.Index, match.Length));
                        orderedMatches.Add(match.Index, tokenMatch);
                    }
                }
            }
            return orderedMatches;
        }
    }
}
