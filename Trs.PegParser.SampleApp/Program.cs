using System;
using System.Linq;

namespace Trs.PegParser.SampleApp
{
    class Program
    {
        //        
        // How this works:
        //
        // 1. The tokenizer is used to label input character
        //    ranges and outputs a stream of tokens.
        //
        // 2. The parser is used to parse the tokens
        //    calling callback functions. These functions are refered 
        //    to as "Semantic actions". In this program it is used to 
        //    populate a data structure representing the test input.
        // 
        // 3. The data structure is used to render a graph of the input
        //    function.
        //

        // Test innput explanation
        // line 1: The function to be graphed
        // line 2: The function domain (i.e. input value range)

        const string TestInput = "1 + 1;";
//@"
//        ((sin(x) + cos(x)) * 3 - x) / 2;
//        domain(0, 2); 
//        ";
        

        // Output width in px - the height will be calculated from the "output" values,
        // preserving the aspect ratio.
        const int OutputImageWidth = 1000;
        const string OutputFileName = "output.bmp";

        static void Main(string[] args)
        {
            // All functionality should be accessed via the PegFacade class.
            // This ties the Tokenizer, Parser, and semantic functions together.
            var pegFacade = new PegFacade<TokensNames, ParsingRuleNames, IParseResult>();
            
            // First define the tokens and create the tokenizer
            var tokenizer = pegFacade.Tokenizer(TokenDefinitions.GetTokenDefinitions());

            // Create the parser
            var parser = ParsingRuleDefinitions.GetParser(pegFacade);

            // Tokenize
            var tokens = tokenizer.Tokenize(TestInput);
            if (!tokens.Succeed)
            {
                Console.WriteLine("Error - Unknown characters");
                return;
            }

            // Strip whitespace
            var inputTokens = tokens.MatchedRanges
                                .Where(m => m.TokenName != TokensNames.Whitespace)
                                .ToList();

            // Parse
            var parseResult = parser.Parse(inputTokens);
            if (!parseResult.Succeed)
            {
                Console.WriteLine("Error - Parsing failed");
                return;
            }

            Console.WriteLine("Parsing succeeded");
        }
    }
}
