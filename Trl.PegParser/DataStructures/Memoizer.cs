using System.Collections.Generic;

namespace Trl.PegParser.DataStructures
{
    public class Memoizer<TInput, TOutput>
    {
        private readonly Dictionary<TInput, TOutput> _existingMappings;

        public Memoizer(IEqualityComparer<TInput> inputComparer)
        {
            _existingMappings = new Dictionary<TInput, TOutput>(inputComparer);
        }

        /// <summary>
        /// Caches related input and output. If input is already present,
        /// existing values are overwritten.
        /// </summary>
        public void Memoize(TInput input, TOutput output) => _existingMappings[input] = output;

        /// <summary>
        /// Gets the previously saved output for a given input.
        /// If there is no 
        /// </summary>
        public TOutput GetOutput(TInput input)
        => _existingMappings.TryGetValue(input, out TOutput existingOutput) switch
        {
            true => existingOutput,
            _ => default
        };

        /// <summary>
        /// Clears all memoized values.
        /// Also clears associated integer mappers.
        /// </summary>
        public void ClearAll()
        {
            _existingMappings.Clear();
        }
    }
}
