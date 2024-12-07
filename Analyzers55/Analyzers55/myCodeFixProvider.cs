
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

        
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var identifierNode = root?.FindNode(diagnosticSpan);
        if (identifierNode == null )
            return;
       
     
        context.RegisterCodeFix(
            CodeAction.Create(
                title: string.Format(Resources.CS236651CodeFixTitle),
                createChangedSolution: c => FixNamingAsync(context.Document, identifierNode, c),
                equivalenceKey: "FixNamingConvention"),
            diagnostic);
        
 
    }
    private async Task<Solution> FixNamingAsync(Document document, SyntaxNode identifierNode, CancellationToken cancellationToken)
    {
  
        
   
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var symbol = semanticModel?.GetDeclaredSymbol(identifierNode, cancellationToken);

        if (symbol == null)
        {
            return document.Project.Solution;
        }

 
        var newName = GenerateCorrectName(symbol);
        
       
  
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


    var validChars = originalName.Where(c => isEnglishLetterOrDigit(c) || c == '_').ToArray();
    var cleanedName = new string(validChars);
    

    string newName;
    switch (symbol)
    {
        case IMethodSymbol _:
            newName = SuitableClassMethodName(originalName);
            break;
        case INamedTypeSymbol namedSymbol when namedSymbol.TypeKind == TypeKind.Class:
   
            newName = SuitableClassMethodName(originalName);
            break;
        case ILocalSymbol _:

            newName = SuitableLocalVarName(originalName);
            break;
        case IFieldSymbol fieldSymbol when fieldSymbol.IsConst:

            newName = SuitableGlobalConstVarName(originalName);
            break;
        default:
            newName = cleanedName; 
            break;
    }



    return newName;
}

private string SuitableClassMethodName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "FixMe"; 
    }
    
    
    var validChars = originalName.Where(c => isEnglishLetterOrDigit(c)).ToArray();
    var cleanedName = new string(validChars);
    
  
    char[] chars = new char[cleanedName.Length];

    
    chars[0] = isEnglishLetter(cleanedName[0]) ? char.ToUpper(cleanedName[0]) : cleanedName[0];

  
    for (int i = 1; i < cleanedName.Length; i++)
    {
        char currentChar = cleanedName[i];
        char previousChar = cleanedName[i - 1];

        if (isEnglishLetter(currentChar) && char.IsDigit(previousChar))
        {
    
            chars[i] = char.ToUpper(currentChar);
        }
        else
        {
       
            chars[i] = currentChar;
        }
    }


    string transformedName = new string(chars);


    return transformedName;
}


private string SuitableLocalVarName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "fixMe"; 
    }
    

    var validChars = originalName.Where(c => isEnglishLetterOrDigit(c)).ToArray();
    var cleanedName = new string(validChars);
    

    char[] chars = new char[cleanedName.Length];

 
    chars[0] = isEnglishLetter(cleanedName[0]) ? char.ToLower(cleanedName[0]) : cleanedName[0];

 
    for (int i = 1; i < cleanedName.Length; i++)
    {
        char currentChar = cleanedName[i];
        char previousChar = cleanedName[i - 1];

        if (isEnglishLetter(currentChar) && char.IsDigit(previousChar))
        {
   
            chars[i] = char.ToUpper(currentChar);
        }
        else
        {
        
            chars[i] = currentChar;
        }
    }


    string transformedName = new string(chars);


    return transformedName;
}


private string SuitableGlobalConstVarName(string originalName)
{
    if (string.IsNullOrEmpty(originalName))
    {
        return "FIX_ME"; 
    }
    

    var validChars = originalName.Where(c => char.IsLetter(c) || c== '_').ToArray();
    
    var cleanedName = new string(validChars);
    cleanedName=cleanedName.TrimStart('_');
    cleanedName=cleanedName.TrimEnd('_');
     cleanedName = Regex.Replace(cleanedName, "_+", "_");
     cleanedName = cleanedName.ToUpper();
     return cleanedName;
    }
}