using System;
using System.Collections.Generic;
using System.Linq;

namespace Trl.PegParser.Tokenization
{
    public class Tokenizer<TTokenName>
        where TTokenName: Enum
    {
        private readonly IReadOnlyList<TokenDefinition<TTokenName>> prioritizedTokenDefinitions;

        public Tokenizer(IEnumerable<TokenDefinition<TTokenName>> prioritizedTokenDefinitions)
        {
            if (prioritizedTokenDefinitions == null || !prioritizedTokenDefinitions.Any())
            {
                throw new ArgumentException("Expected at least one token definition.", nameof(prioritizedTokenDefinitions));
            }

            this.prioritizedTokenDefinitions = prioritizedTokenDefinitions.ToList().AsReadOnly();
        }

        /// <summary>
        /// Tokenizes the input string based on the given definitions.
        /// </summary>
        /// <param name="inputString">String to be tokenized.</param>
        /// <param name="prioritizedTokenDefinitions">The token definitions in order of importance. It is possible to
        /// create overlapping definitions with regular expression. The tokens are mapped first to last to get arround this.</param>
        /// <returns>The result of tokenization. Tokens are assaigned names from <see cref="TTokenName"/></returns>
        public TokenizationResult<TTokenName> Tokenize(string inputString)
        {            
            _ = inputString ?? throw new ArgumentException("Expected input string", nameof(inputString));

            var matches = GetMatches(inputString, prioritizedTokenDefinitions);
            var mismatches = GetMismatches(matches, inputString.Length);
            return new TokenizationResult<TTokenName>(matches.AsReadOnly(), mismatches.AsReadOnly());
        }

        private List<MatchRange> GetMismatches(List<TokenMatch<TTokenName>> matches, int inputStringLength)
        {
            if (inputStringLength == 0)
            {
                return new List<MatchRange>();
            }

            if (matches.Count == 0)
            {
                return new List<MatchRange>
                {
                    new MatchRange(0, inputStringLength)
                };
            }

            var mismatches = new List<MatchRange>();
            int currentIndex = 0;
            foreach (var match in matches)
            {
                if (match.MatchedCharacterRange.StartIndex != currentIndex)
                {
                    mismatches.Add(new MatchRange(currentIndex, match.MatchedCharacterRange.StartIndex - currentIndex));
                    currentIndex = match.MatchedCharacterRange.StartIndex + match.MatchedCharacterRange.Length;
                }
                else
                {
                    currentIndex += match.MatchedCharacterRange.Length;
                }
            }            
            if (currentIndex != inputStringLength)
            {
                mismatches.Add(new MatchRange(currentIndex, inputStringLength - currentIndex));
            }
            return mismatches;
        }

        private List<TokenMatch<TTokenName>> GetMatches(string inputString, 
            IEnumerable<TokenDefinition<TTokenName>> prioritizedTokenDefinitions)
        {
            bool foundMatch = true;
            var nextStartIndex = 0;
            var matches = new List<TokenMatch<TTokenName>>();
            var nextMatches = new List<TokenMatch<TTokenName>>(prioritizedTokenDefinitions.Count());

            do
            {
                foundMatch = false;
                nextMatches.Clear();
                foreach (var definition in prioritizedTokenDefinitions)
                {
                    var match = definition.DefiningRegex.Match(inputString, nextStartIndex);
                    if (match.Success)
                    {
                        var tokenMatch = new TokenMatch<TTokenName>(definition.Name,
                                new MatchRange(match.Index, match.Length), inputString);
                        nextMatches.Add(tokenMatch);
                    }
                }
                if (nextMatches.Any())
                {
                    var minStart = nextMatches.Min(m => m.MatchedCharacterRange.StartIndex);
                    var bestMatch = nextMatches.First(nm => nm.MatchedCharacterRange.StartIndex == minStart);
                    matches.Add(bestMatch);
                    nextStartIndex = bestMatch.MatchedCharacterRange.Length switch
                    {
                        0 => bestMatch.MatchedCharacterRange.StartIndex + 1,
                        _ => bestMatch.MatchedCharacterRange.StartIndex + bestMatch.MatchedCharacterRange.Length,
                    };
                    foundMatch = true;
                }
            }
            while (foundMatch && nextStartIndex < inputString.Length);

            return matches;
        }
    }
}
