# Overview

_Trl.PegParser_ contains a tokenizer and a parser. The tokenizer uses regular expressions to define tokens, and exposes both matched and unmatched character ranges. The PEG Parser uses parsing expression grammers with tokens produced by the tokenizer. _Trl.PegParser_ is built on .NET Standard 2.1 for cross-platform compatibility.

_Trl.PegParser_ supports left recursion and contains safeguards against infinite loops in grammers due to cyclical non-terminal definitions.

_Trl.PegParser_ prevents exponential parsing times based on parsing rules with multiple left recursion non-terminals through the use of memoization.

An example of a simple parser for adding up numbers can be found in the `Trl.PegParser.SampleApp` folder of this repository.

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

Then define the PEG facade. This is a class that ties everything together in a type-safe manner. The output type for the parser is `double?`, however it is not used in this section. In a more complex system this could be an AST (abstract syntax tree) base type or an interface.

```C#
var peg = new PegFacade<Tokens, RuleNames, double?>();
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

# Simple Example - Adding semantics with callbacks

Semantics are implemented as callback functions. These functions are attached to the `PegFacade` class (see previous section) and are copied over to parsing operators when they are created via the parser generator.

Therefore, these functions _must_ be defined before the parsing operators and rules. I refer to these functions as 'semantic actions' in the code.

Semantic actions are assigned per parsing operator:

```C#
var semanticActions = peg.DefaultSemanticActions;

semanticActions.OrderedChoiceAction =
    (matchedTokens, subresults) => subresults.First();

semanticActions.SequenceAction =
    (matchedTokens, subresults) =>
    {
        return subresults.Where(s => s.HasValue).Sum(s => s.Value);
    };
```

It is also possible to define actions per terminal and non-terminal symbol name. This makes it easier to implement targeted semanic actions:

```C
semanticActions.SetTerminalAction(Tokens.Number,
    (matchedTokens, subresults) => double.Parse(matchedTokens.GetMatchedString()));

semanticActions.SetNonTerminalAction(RuleNames.Add,
    (matchedTokens, subresults) => subresults.First());
```

The second argument for semantic actions, `subresults`, is of type `IEnumerable`. This is to accomodate scenarios where there may be more than one sub result, for example when the _Sequence_ or _One or More_ operator matched a sequence of substring.

In some situations, an operator may not have a semantic action assigned. In this scenario, the default value for the semantic action return type will be returned. In this case it is `null` for `double?`.

Semantic action results can be retrieved via the parse result.

```C
Console.WriteLine($"Sum = {output.SemanticActionResult}");
```

# PEG Operator Syntax

The parser generator uses the following operators for specifying parsing rules:

- Parsing rules have this form: `Head => Body;` - `Head` is a non-terminal symbol form the enumeration passed into the `PegFacade` class. `Body` is any combination of PEG operators. The semicolon is optional. Multiple rules can be passed into the parser generator.
- And Predicate: `&(E)` - the brackets are required
- Empty String: `[]`
- Non-Terminal: `E` - `E` is a member of the parsing rule names enumeration.
- Not Predicate: `!(E)` - the brackets are required.
- One or More: `E+`
- Optional: `E?`
- Ordered Choice: `C1 | ... | Cn`
- Sequence: `S1 S2 ... Sn`
- Terminal: `[T]` - `T` is a member of the terminal names enumeration.
- Zero or More: `E*`
- Brackets for grouping elements, ex. `(A | B) C` vs. `A | (B C)`

# Complex parsing scenarios

In some scenarios, you may need to parse based on larger grammers. At this point it becomes tedious to work with default parsing rules and you end up coding a lot of boilerplate code to pass results from one operator to another.

To deal with this, a method was created to automatically return sub results on a tree structure, where each node corresponds to a PEG operator.

When using this method, an interface needs to be defined that acts as the return value in the PEG facade. The definition for the facade may become something along these lines:

```C#
var pegFacade = new PegFacade<TokensNames, ParsingRuleNames, ICalculatorAstNode>();
```

Here `ICalculatorAstNode` is the semantic result. This can now be used with the afore mentioned base type (`GenericPassthroughResult`) to create a class like this:

```C#
public class GenericResult
    : GenericPassthroughResult<ICalculatorAstNode, TokensNames>, ICalculatorAstNode
{
}
```

This can now be used to create a default semantic action method by passing it to the facade.

```C#
var defaultSemantics = pegFacade.DefaultSemanticActions;
defaultSemantics.SetDefaultGenericPassthroughAction<GenericResult>();
```

All operators created after this method call will use the passthrough function and return subresults of type `GenericResult` unless otherwise specified.

After this `SetTerminalAction` and `SetNonTerminalAction` can be used to set semantic actions for specific terminals and non-terminals. For example, when parsing a binary operator, it should be possible to write something like this:

```C#
defaultSemantics.SetNonTerminalAction(ParsingRuleNames.BinaryExpression, (matchResult, subActionResults) =>
{
    var subResultsList = subActionResults.ToList();
    var op = (GenericResult)subResultsList[1];
    var lhs = (FunctionBase)subResultsList[0];
    var rhs = (FunctionBase)subResultsList[2];
    return new BinaryExpression(op.MatchedTokens.GetMatchedString(), lhs, rhs);
});
```

# Installation via Nuget

See [https://www.nuget.org/packages/Trl.PegParser/](https://www.nuget.org/packages/Trl.PegParser/) for nuget package.

# Unit Test Code Coverage

Unit tests can be run using the `.\test.ps1` script. This will generate a code coverage report in the `.\UnitTestCoverageReport` folder using [Coverlet](https://github.com/tonerdo/coverlethttps://github.com/tonerdo/coverlet) and [ReportGenerator](https://github.com/danielpalme/ReportGenerator).

![Code Coverage](code_coverage.PNG)

# Licence

Trl.PegParser is released under the MIT open source licence. See LICENCE.txt in this repository for the full text.
