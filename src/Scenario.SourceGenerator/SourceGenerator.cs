using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Scenario.SourceGenerator
{
    [Generator]
    public class ScenarioExtensionsGenerator : ISourceGenerator
    {
        private const string AttributeText = @"
namespace Scenario
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class ScenarioDependencyAttribute : System.Attribute
    {
        public string? ExtensionName { get; set; }
        public bool CreateNonGenericExtension { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method)]
    public sealed class ScenarioResourceAttribute : System.Attribute
    {
        public string? ExtensionName { get; set; }
        public bool CreateNonGenericExtension { get; set; }
    }
}
";
        
        // TODO: optional IServiceCollection parameter

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(i => i.AddSource("ScenarioExtensionsAttributes", AttributeText));

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;
            
            var dependencyAttributeSymbol =
                context.Compilation.GetTypeByMetadataName("Scenario.ScenarioDependencyAttribute");
            var resourceAttributeSymbol =
                context.Compilation.GetTypeByMetadataName("Scenario.ScenarioResourceAttribute");

            foreach (var namespaceMethodGroup in receiver.DependencyMethods.GroupBy(m => m.MethodSymbol.ContainingNamespace))
            {
                var containingNamespace = namespaceMethodGroup.Key;
                var methods = namespaceMethodGroup.ToList();
                var dependencyExtensionsSource = GenerateForMethodsInNamespace(containingNamespace, methods,
                    dependencyAttributeSymbol, context, false);
                context.AddSource($"{containingNamespace.Name}_scenarioDependencyExtensions.cs", SourceText.From(dependencyExtensionsSource, Encoding.UTF8));
            }

            foreach (var namespaceMethodGroup in receiver.ResourceMethods.GroupBy(m =>
                m.MethodSymbol.ContainingNamespace))
            {
                var containingNamespace = namespaceMethodGroup.Key;
                var methods = namespaceMethodGroup.ToList();
                var dependencyExtensionsSource = GenerateForMethodsInNamespace(containingNamespace, methods,
                    resourceAttributeSymbol, context, true);
                context.AddSource($"{containingNamespace.Name}_scenarioResourceExtensions.cs", SourceText.From(dependencyExtensionsSource, Encoding.UTF8));
            }
        }

        private string GenerateForMethodsInNamespace(INamespaceSymbol containingNamespace, List<DependencyMethodInfo> methods, INamedTypeSymbol? dependencyAttributeSymbol, GeneratorExecutionContext context, bool resource)
        {
            var namespaceName = containingNamespace.ToDisplayString();

            var source = new StringBuilder($@"
namespace {namespaceName}
{{
    public static class {namespaceName.Replace(".", "__")}_scenario{(resource ? "Resource" : "Dependency")}Extensions
    {{
");

            foreach (var methodInfo in methods)
                source.Append(GenerateMethodSource(methodInfo, dependencyAttributeSymbol, resource));

            source.Append("    }\n}");
            return source.ToString();
        }

        private string GenerateMethodSource(DependencyMethodInfo methodInfo, INamedTypeSymbol? dependencyAttributeSymbol, bool resource)
        {
            var (method, fullParameters, fullConstraints) = methodInfo;
            var name = method.Name;
            var parameters = method.Parameters;
            var typeParameters = method.TypeParameters;

            var attributeData = method.GetAttributes().Single(a =>
                a.AttributeClass?.Equals(dependencyAttributeSymbol, SymbolEqualityComparer.Default) ?? false);
            var overridenNameOption =
                attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "ExtensionName").Value;
            var generateNonGenericForm =
                attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "CreateNonGenericExtension").Value.Value as bool?;

            var extensionName = ChooseName(name, overridenNameOption);
            if (string.IsNullOrWhiteSpace(extensionName))
                return null!; // TODO: issue diagnostic warning about an invalid name

            if (!resource && !method.ReturnsVoid)
                return null!; // TODO: issue diagnostic warning about an invalid return type - must be void

            if (typeParameters.Any(t => t.Name == "__TScenarioBuilder"))
                return null!; // TODO: issue diagnostic warning about an invalid type parameter

            if (parameters.Any(t => t.Name is "__scenarioBuilder" or "__services" or "__scope"))
                return null!; // TODO: issue diagnostic warning about an invalid parameter name

            if (parameters.Any(t => t.RefKind != RefKind.None))
                return null!; // TODO: issue diagnostic warning that ref and out parameters are invalid

            var typeParams = "";
            var funcCallTypeParams = "";
            if (typeParameters.Length > 0)
            {
                var joinedParams = string.Join(", ", typeParameters.Select(t => t.Name));
                typeParams = $", {joinedParams}";
                funcCallTypeParams = $"<{joinedParams}>";
            }

            var @params = "";
            var paramInjection = "";
            if (parameters.Length > 1)
            {
                @params = $", {string.Join(", ", fullParameters.Skip(1))}";
                paramInjection = $", {string.Join(",", parameters.Skip(1).Select(p => $"{(p.RefKind == RefKind.Out ? "out " : "")}{(p.RefKind == RefKind.Ref ? "ref " : "")}{p.Name}"))}";
            }

            var typeConstraints = new StringBuilder();
            foreach (var typeParam in typeParameters)
            {
                typeConstraints.AppendLine();
                var paramConstraints = new List<string>();
                switch (typeParam.ReferenceTypeConstraintNullableAnnotation)
                {
                    case NullableAnnotation.None:
                        break;
                    case NullableAnnotation.NotAnnotated:
                        paramConstraints.Add("class");
                        break;
                    case NullableAnnotation.Annotated:
                        paramConstraints.Add("class?");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (typeParam.HasConstructorConstraint) paramConstraints.Add("new()");
                if (typeParam.HasNotNullConstraint) paramConstraints.Add("notnull");
                if (typeParam.HasUnmanagedTypeConstraint) paramConstraints.Add("unmanaged");
                if (typeParam.HasValueTypeConstraint) paramConstraints.Add("struct");
                paramConstraints.AddRange(typeParam.ConstraintTypes.Select(constrainType => constrainType.ToDisplayString()));
                typeConstraints.Append($"            where {typeParam.Name} : {string.Join(", ", paramConstraints)}");
            }

            if (resource)
            {
                var body = $@"
        public static __TScenarioBuilder {extensionName}<__TScenarioBuilder{typeParams}>(this __TScenarioBuilder __scenarioBuilder{@params})
            where __TScenarioBuilder : IScenarioBuilder{typeConstraints}
            => (__TScenarioBuilder) __scenarioBuilder.With(__scope => {method.ContainingType.Name}.{method.Name}{funcCallTypeParams}(__scope{paramInjection}));
";
                if (generateNonGenericForm ?? false)
                {
                    body += $@"
        public static IScenarioBuilder {extensionName}{funcCallTypeParams}(this IScenarioBuilder __scenarioBuilder{@params}){typeConstraints}
            => __scenarioBuilder.With(__scope => {method.ContainingType.Name}.{method.Name}{funcCallTypeParams}(__scope{paramInjection}));
";
                }

                return body;
            }
            else
            {
                var body = $@"
        public static __TScenarioBuilder {extensionName}<__TScenarioBuilder{typeParams}>(this __TScenarioBuilder __scenarioBuilder{@params})
            where __TScenarioBuilder : IScenarioBuilder{typeConstraints}
            => (__TScenarioBuilder) __scenarioBuilder.Use(__services => {method.ContainingType.Name}.{method.Name}{funcCallTypeParams}(__services{paramInjection}));
";
                if (generateNonGenericForm ?? false)
                {
                    body += $@"
        public static IScenarioBuilder {extensionName}{funcCallTypeParams}(this IScenarioBuilder __scenarioBuilder{@params}){typeConstraints}
            => __scenarioBuilder.Use(__services => {method.ContainingType.Name}.{method.Name}{funcCallTypeParams}(__services{paramInjection}));
";
                }

                return body;
            }

            string ChooseName(string methodName, TypedConstant overridenNameOpt)
            {
                if (!overridenNameOpt.IsNull)
                    return overridenNameOpt.Value?.ToString()!;

                methodName = methodName.TrimStart('_');
                return methodName.Length switch
                {
                    0 => string.Empty,
                    1 => methodName.ToUpper(),
                    _ => methodName[..1].ToUpper() + methodName[1..]
                };
            }
        }
    }

    internal record DependencyMethodInfo(IMethodSymbol MethodSymbol, IImmutableList<string> Parameters,
        IImmutableList<string> TypeConstraints);

    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<DependencyMethodInfo> DependencyMethods { get; } = new();
        public List<DependencyMethodInfo> ResourceMethods { get; } = new();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax ||
                methodDeclarationSyntax.AttributeLists.Count == 0)
                return;

            var fullParameterStrings = methodDeclarationSyntax.ParameterList.Parameters.Select(p => p.ToFullString()).ToImmutableList();
            var typeConstraints = methodDeclarationSyntax.ConstraintClauses.Select(c => c.ToFullString()).ToImmutableList();
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;

            if (methodSymbol == null) return;
            if (methodSymbol.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == "Scenario.ScenarioDependencyAttribute"))
                DependencyMethods.Add(new DependencyMethodInfo(methodSymbol, fullParameterStrings,
                    typeConstraints));
            else if (methodSymbol.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == "Scenario.ScenarioResourceAttribute"))
                ResourceMethods.Add(new DependencyMethodInfo(methodSymbol, fullParameterStrings,
                    typeConstraints));
        }
    }
}