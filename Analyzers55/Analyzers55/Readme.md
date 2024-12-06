# HW1 - C# Linter


This project is simple C# linter that follows the policy provided in CS236651 course in the Technion
It focuses on identifier names and providing code fixes.

written by Dar Nissan - dar.nissan@campus.technion.ac.il


## Policy
-	No identifier shall contain a character that is not a lowercase letter, an uppercase letter, a number or an underscore.
-	Class names and method names should be written in UpperCamelCase, that is, multiple words must be concatenated while having the first letter as uppercase and the rest of the letters as lowercase. They must not contain underscores.
-	Local variable names should be written in lowerCamelCase, that is, multiple words are concatenated as in UpperCamelCase, except the first word is all lowercase. They must not contain underscores.
-	Global constant names should be written in SNAKE_CASE, that is, multiple words are each in all-uppercase and connected using underscores. They should not contain numbers or lowercase letters.


# Linter's Approach

## Capturing desired identifiers
The linters uses the fact the c# and Roslyn is using [SymbolKind](https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.symbolkind?view=roslyn-dotnet-4.9.0) to all of it's identifiers. this way the desired identifiers are captures with the analyzer.

It should be mentions that SymbolKind.Local IS NOT supported with Roslyn analyzers and so to follow the policy the linter uses [LocalDeclarationStatementSyntax](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/declarations) and goes over all of it's variables to apply policy

## Dealing with invalid characters
As it list below to check if we match the naming conventions we can use regular expressions but unfortunatly that doesnt help us fix invalid characters. Also C# built in -IsLetter is letting ANY valid unicode character.Thus, it was decided to implement a simple is english characters to avoid identifiers that are sytaxly valid but mix up several different languges in the same identifier

## Regular Expressions
The linter uses the following regular expressions to follow the policy

```cs
UpperCamelCaseRegex = new Regex(@"^([A-Z][a-z]*[0-9]*)+$"
lowerCamelCaseRegex = new Regex(@"^[a-z]+[0-9]*([A-Z][a-z]*[0-9]*)*$"
SNAKE_CASE_REGEX = new Regex(@"^[A-Z]+([_][A-Z]+)*$"
```


## Fallback names


[MIT](https://choosealicense.com/licenses/mit/)