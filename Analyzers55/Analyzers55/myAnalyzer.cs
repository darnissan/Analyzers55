
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sample.Analyzers
{
    /// <summary>
    /// Analyzer for reporting syntax node diagnostics.
    /// It reports diagnostics for implicitly typed local variables, recommending explicit type specification.
    /// </summary>
    /// <remarks>
    /// For analyzers that requires analyzing symbols or syntax nodes across compilation, see <see cref="CompilationStartedAnalyzer"/> and <see cref="CompilationStartedAnalyzerWithCompilationWideAnalysis"/>.
    /// For analyzers that requires analyzing symbols or syntax nodes across a code block, see <see cref="CodeBlockStartedAnalyzer"/>.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALERRRRRRRTTTTTTTTTTTTTTTTTTTTTTTTTTTT";
        private const string Title = "Declare explicit type for local declarations.";
        private static readonly Regex UpperCamelCaseRegex = new Regex(@"^([A-Z][a-z]*[0-9]*)+$", RegexOptions.Compiled);
        private static readonly Regex lowerCamelCaseRegex = new Regex(@"^[a-z]+[0-9]*([A-Z][a-z]*[0-9]*)*$", RegexOptions.Compiled);
        private static readonly Regex SNAKE_CASE_REGEX = new Regex(@"^[A-Z]+([_][A-Z]+)*$", RegexOptions.Compiled);
        public const string MessageFormat =
            "Local '{0}' is implicitly typed. Consider specifying its type explicitly in the declaration.";

        private const string Description = "Declare explicit type for local declarations.";
        
        
        
        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                MessageFormat,
                "Naming",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: Description);
        

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            //context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.VariableDeclaration);
            //context.RegisterSyntaxNodeAction(AnalyzeSyntaxNodeMethodDeclaration, SyntaxKind.MethodDeclaration);
            //context.RegisterSyntaxNodeAction(AnalyzeSyntaxNodeClassDeclaration, SyntaxKind.ClassDeclaration);
            //context.RegisterSymbolAction(AnalyzeClassSymbol, SymbolKind.NamedType);
            //context.RegisterSyntaxNodeAction(AnalyzeObjCreation,SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeIdentifierName, SyntaxKind.IdentifierName);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeLocalDeclaration, SyntaxKind.LocalDeclarationStatement);
            context.RegisterSyntaxNodeAction(AnalyzeGlobaclConstDeclaration,SyntaxKind.FieldDeclaration);
        }

      
        private static void AnalyzeGlobaclConstDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            // Check for 'const' modifier
            if (!fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                return;

            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                var name = variable.Identifier.Text;

                if (!SNAKE_CASE_REGEX.IsMatch(name))
                {
                    var diagnostic = Diagnostic.Create(Rule, variable.Identifier.GetLocation(), name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        private static void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;
            foreach (var variable in localDeclaration.Declaration.Variables)
            {
                // issue rule upon every local variable that its identifier doesnt follow low camel case
                if (!lowerCamelCaseRegex.IsMatch(variable.Identifier.ValueText))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variable.Identifier.ValueText));
                }
            }
        }
        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var expression = invocationExpr.Expression;

            ISymbol symbol = null;

            if (expression is IdentifierNameSyntax identifierNameSyntax)
            {
                symbol = context.SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;
                if (symbol is IMethodSymbol methodSymbol &&
                    identifierNameSyntax.Identifier.Text == methodSymbol.Name && UpperCamelCaseRegex.IsMatch(identifierNameSyntax.Identifier.Text)==false)
                {
                    var methodName = identifierNameSyntax.Identifier.Text;
                    var diagnostic = Diagnostic.Create(Rule, identifierNameSyntax.Identifier.GetLocation(), methodName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else if (expression is MemberAccessExpressionSyntax memberAccessExpr)
            {
                var nameSyntax = memberAccessExpr.Name;
                if (nameSyntax is IdentifierNameSyntax nameIdentifier)
                {
                    symbol = context.SemanticModel.GetSymbolInfo(nameIdentifier).Symbol;
                    if (symbol is IMethodSymbol methodSymbol &&
                        nameIdentifier.Identifier.Text == methodSymbol.Name 
                        && UpperCamelCaseRegex.IsMatch(nameIdentifier.Identifier.Text)==false)
                    {
                        var methodName = nameIdentifier.Identifier.Text;
                        var diagnostic = Diagnostic.Create(Rule, nameIdentifier.Identifier.GetLocation(), methodName);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    
        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var methodName = methodDeclaration.Identifier.Text;
            if (UpperCamelCaseRegex.IsMatch(methodName) == false)
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }
        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            // Report a diagnostic on the class identifier in class declarations
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var className = classDeclaration.Identifier.Text;
            if (!UpperCamelCaseRegex.IsMatch(className))
            {
                var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), className);
                context.ReportDiagnostic(diagnostic);
            }
        }
    
        private static void AnalyzeIdentifierName(SyntaxNodeAnalysisContext context)
        {
            var identifierNameSyntax = (IdentifierNameSyntax)context.Node;

            // Use the semantic model to get symbol information
            var symbolInfo = context.SemanticModel.GetSymbolInfo(identifierNameSyntax);
            var symbol = symbolInfo.Symbol;

            // Check if the symbol is a named type symbol (class, interface, etc.)
            if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind == TypeKind.Class )
            {
                // Check if the identifier text matches the symbol name
                if (identifierNameSyntax.Identifier.Text == namedTypeSymbol.Name && UpperCamelCaseRegex.IsMatch(identifierNameSyntax.Identifier.Text)==false )
                {
                    var className = identifierNameSyntax.Identifier.Text;
                    var diagnostic = Diagnostic.Create(Rule, identifierNameSyntax.Identifier.GetLocation(), className);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        
        
        
        private static void AnalyzeObjCreation(SyntaxNodeAnalysisContext context)
        {
            var creation = (ObjectCreationExpressionSyntax)context.Node;
            var identifierName = creation.ToString();
        }
        private static void AnalyzeClassSymbol(SymbolAnalysisContext context)
        {
            ISymbol symbol = context.Symbol;

            if (symbol.Kind is (SymbolKind.Method or SymbolKind.NamedType))
            {
                // bool isComforting = UpperCamelCaseRegex.IsMatch(symbol.Name);
                // if (isComforting)
                //     return;

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rule,
                        symbol.Locations[0],
                        symbol.Name));
                return;
            }
         
        }

        private static bool  IsValidChars(string text)
        {
            foreach (var character in text)
            {
                if (!((character>='A' && character<='Z') || 
                      (character>='a' && character<='z') ||
                      (character>='0' && character<='9')))
                    return false;
            }
            return true;
        }
        private static void AnalyzeSyntaxNodeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (ClassDeclarationSyntax)context.Node;
            var declarationText = declaration.Identifier.Text;
            if (!IsValidChars(declarationText))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), declarationText));
            }
            
         
            if (!Regex.IsMatch(declarationText, UpperCamelCaseRegex.ToString()))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), declarationText));
            }
        }
        
        
        
        
        
        private void SymbolAction(SymbolAnalysisContext context)
        {
            ISymbol symbol = context.Symbol;

            if (symbol.Kind is (SymbolKind.Method or SymbolKind.NamedType))
            {
                bool isComforting = UpperCamelCaseRegex.IsMatch(symbol.Name);
                if (isComforting)
                    return;

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rule,
                        symbol.Locations[0],
                        symbol.Name));
                return;
            }

            if (symbol.Kind is (SymbolKind.Parameter or SymbolKind.Property))
            {
                bool isComforting = lowerCamelCaseRegex.IsMatch(symbol.Name);
                if (isComforting)
                    return;

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rule,
                        symbol.Locations[0],
                        symbol.Name));
                return;
            }

          

            
        }
    
        private static void AnalyzeSyntaxNodeMethodDeclaration(SyntaxNodeAnalysisContext context) 
        {
                var declaration = (MethodDeclarationSyntax)context.Node;
                var declarationText = declaration.Identifier.ValueText;
                if (!IsValidChars(declarationText))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), declarationText));
                }

                if (!Regex.IsMatch(declarationText, UpperCamelCaseRegex.ToString()))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), declarationText));
                }

             
        }
        

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            // Find implicitly typed variable declarations.
            VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)context.Node;

            foreach (VariableDeclaratorSyntax variable in declaration.Variables)
            {
                foreach (char character in variable.Identifier.ValueText)
                {

                    if (!((character>='A' && character<='Z') || 
                        (character>='a' && character<='z') ||
                        (character>='0' && character<='9')))
                    {
                        // For all such locals, report a diagnostic.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Rule,
                                variable.GetLocation(),
                                variable.Identifier.ValueText));
                    }

                }
            }
        }
    }
}