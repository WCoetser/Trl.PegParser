# Overview

_Trs.PegParser_ contains a tokenizer and a parser. The tokenizer uses regular expressions to define tokens, and exposes both matched and unmatched character ranges. The PEG Parser uses parsing expression grammers with tokens produced by the tokenizer. _Trs.PegParser_ is build on .NET Standard 2.1 for cross-platform compatibility.

# Simple Example - Parsing only

This is the input that needs to be parsed:

```C#
const string input = "1 + 2 + 3";
```

Start by defining enumerations to name tokens and parsing rules.

```C#
public enum Tokens { Plus, Number, Whitespace }
public enum RuleNames { Add }
```

Then define the PEG facade. This is a class that ties everything together in a type-safe manner. The output type for the parser is `double`, however it is not used in this example. In a more complex system this could be an AST (abstract syntax tree) base type or an interface.

```C#
var peg = new PegFacade<Tokens, RuleNames, double>();
```

Next define a tokenizer using regular expressions and the token names from the `Tokens` enumeration.

```C#
var tokenizer = peg.Tokenizer(new[] {
    peg.Token(Tokens.Plus, new Regex(@"\+")),
    peg.Token(Tokens.Number, new Regex(@"[-+]?[0-9]+\.?[0-9]*")),
    peg.Token(Tokens.Whitespace, new Regex(@"\s+"))
});
```

Now the parsing rules can be defined. The values in square brackets refer to the token names from the `Tokens` enumeration. The value on the left hand side of the arrow refers to the rule names defined in the `RuleNames` enumeration.

```C#
var grammer = @"Add => (Add [Plus] Add) | [Number]";
var rules = peg.ParserGenerator.GetParsingRules(grammer);
var parser = peg.Parser(RuleNames.Add, rules); // RuleNames.Add is the start symbol
```

The tokenizer can now be used to break the input strings into tokens for later consumption by the parser.

```C#
var tokenResult = tokenizer.Tokenize(input);
if (!tokenResult.Succeed)
{
    Console.WriteLine("Invalid input");
    return;
}
```

Next, whitespace is stripped from the input stream (resulting from tokenization) using LINQ.

```C#
var inputNoWhitespace = tokenResult.MatchedRanges
    .Where(t => t.TokenName != Tokens.Whitespace)
    .ToList().AsReadOnly();
```

The input can now be parsed.

```C#
var output = parser.Parse(inputNoWhitespace);
if (!output.Succeed)
{
    Console.WriteLine("Invalid input");
}
else
{
    Console.WriteLine("Parse succeeded");
}
```

# PEG Operator Syntax

The parser generator uses the following operators for specifying parsing rules:

- And predicate: `&(E)` - the brackets are required
- Empty string: `[]`
- Non-terminal: `E` - `E` is a member of the parsing rule names enumeration.
- Not predicate: `!(E)` - the brackets are required.
- One or more: `E+`
- Optional: `E?`
- Ordered Choice: `C1 | ... | Cn`
- Sequence: `S1 S2 ... Sn`
- Terminal: `[T]` - `T` is a member of the terminal names enumeration.
- Zero or more: `E*`
- Brackets for grouping elements, ex. `(A | B) C` vs. `A | (B C)`

# Installation via Nuget

See [https://www.nuget.org/packages/Trs.PegParser/](https://www.nuget.org/packages/Trs.PegParser/) for nuget package.

# Unit Test Code Coverage

Unit tests can be run using the `.\test.ps1` script. This will generate a code coverage report in the `.\UnitTestCoverageReport` folder using [Coverlet](https://github.com/tonerdo/coverlethttps://github.com/tonerdo/coverlet) and [ReportGenerator](https://github.com/danielpalme/ReportGenerator).

![Code Coverage](code_coverage.PNG)

# Licence

Trs.PegParser is released under the MIT open source licence. See LICENCE.txt in this repository for the full text.
