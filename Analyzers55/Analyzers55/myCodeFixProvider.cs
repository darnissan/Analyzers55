
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Rename;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;


namespace Analyzers55;


[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MyCodeFixProvider)), Shared]
public class MyCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(MyAnalyzer.DiagnosticId);
    public override FixAllProvider? GetFixAllProvider() => null;
    
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.Single();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the syntax node representing the identifier that needs fixing
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var identifierNode = root?.FindNode(diagnosticSpan);
        if (identifierNode == null )
            return;

        // Register a code action to fix the naming violation
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Fix naming convention",
                createChangedSolution: c => FixNamingAsync(context.Document, identifierNode, c),
                equivalenceKey: "FixNamingConvention"),
            diagnostic);
        
 
    }
    private async Task<Solution> FixNamingAsync(Document document, SyntaxNode identifierNode, CancellationToken cancellationToken)
    {
  
        
        // Obtain the symbol associated with the identifier
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var symbol = semanticModel?.GetDeclaredSymbol(identifierNode, cancellationToken);

        if (symbol == null)
        {
            return document.Project.Solution;
        }

        // Generate the correct name based on your naming policy
        var newName = GenerateCorrectName(symbol);
        
       
        // Use Roslyn's Renamer to apply the new name across the solution
        var solution = document.Project.Solution;
        var optionSet = solution.Workspace.Options;
        var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

        return newSolution;
    }
    public bool isEnglishLetter(char letter)
    {
        if( Regex.IsMatch(letter.ToString(), "^[a-zA-Z]$") )
            return true;
        return false;
    }
    public bool isEnglishLetterOrDigit(char letter)
    {
        if( Regex.IsMatch(letter.ToString(), "^[a-zA-Z0-9]$") )
            return true;
        return false;
    }
    
  

private string GenerateCorrectName(ISymbol symbol)
{
    var originalName = symbol.Name;

    // Remove invalid characters (characters that are not letters, numbers, or underscores)
    var validChars = originalName.Where(c => isEnglishLetterOrDigit(c) || c == '_').ToArray();
    var cleanedName = new string(validChars);
    
    // Apply naming conventions based on symbol kind
    string newName;
    switch (symbol)
    {
        case IMethodSymbol _:
            newName = SuitableClassMethodName(originalName);
            break;
        case INamedTypeSymbol _:
            // UpperCamelCase
            newName = SuitableClassMethodName(originalName);
            break;
        case ILocalSymbol _:
            // lowerCamelCase
            newName = SuitableLocalVarName(originalName);
            break;
        case IFieldSymbol fieldSymbol when fieldSymbol.IsConst:
            // SNAKE_CASE
            newName = SuitableGlobalConstVarName(originalName);
            break;
        default:
            newName = cleanedName; // Fallback to cleaned name if symbol kind is unhandled
            break;
    }



    return newName;
}

private string SuitableClassMethodName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "FixMe"; // Or you can throw an exception if preferred
    }
    
    // First, drop all invalid characters
    var validChars = originalName.Where(c => isEnglishLetterOrDigit(c)).ToArray();
    var cleanedName = new string(validChars);
    
    // Capitalize the first letter and every letter after a digit
    char[] chars = new char[cleanedName.Length];

    // Capitalize the first character if it's a letter
    chars[0] = isEnglishLetter(cleanedName[0]) ? char.ToUpper(cleanedName[0]) : cleanedName[0];

    // Process the rest of the characters
    for (int i = 1; i < cleanedName.Length; i++)
    {
        char currentChar = cleanedName[i];
        char previousChar = cleanedName[i - 1];

        if (isEnglishLetter(currentChar) && char.IsDigit(previousChar))
        {
            // Capitalize if the previous character is a digit
            chars[i] = char.ToUpper(currentChar);
        }
        else
        {
            // Keep the character as is
            chars[i] = currentChar;
        }
    }

    // Create the transformed string
    string transformedName = new string(chars);

    // Return the transformed name
    return transformedName;
}


private string SuitableLocalVarName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "fixMe"; // Or you can throw an exception if preferred
    }
    
    // First, drop all invalid characters
    var validChars = originalName.Where(c => isEnglishLetterOrDigit(c)).ToArray();
    var cleanedName = new string(validChars);
    
    // Capitalize the first letter and every letter after a digit
    char[] chars = new char[cleanedName.Length];

    // Capitalize the first character if it's a letter
    chars[0] = isEnglishLetter(cleanedName[0]) ? char.ToLower(cleanedName[0]) : cleanedName[0];

    // Process the rest of the characters
    for (int i = 1; i < cleanedName.Length; i++)
    {
        char currentChar = cleanedName[i];
        char previousChar = cleanedName[i - 1];

        if (isEnglishLetter(currentChar) && char.IsDigit(previousChar))
        {
            // Capitalize if the previous character is a digit
            chars[i] = char.ToUpper(currentChar);
        }
        else
        {
            // Keep the character as is
            chars[i] = currentChar;
        }
    }

    // Create the transformed string
    string transformedName = new string(chars);

    // Return the transformed name
    return transformedName;
}


private string SuitableGlobalConstVarName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "FIX_ME"; // Or you can throw an exception if preferred
    }
    
    // First, drop all invalid characters
    var validChars = originalName.Where(c => char.IsLetter(c) || c== '_').ToArray();
    
    var cleanedName = new string(validChars);
    cleanedName=cleanedName.TrimStart('_');
    cleanedName=cleanedName.TrimEnd('_');
     cleanedName = Regex.Replace(cleanedName, "_+", "_");
     cleanedName = cleanedName.ToUpper();
     return cleanedName;
    }

}