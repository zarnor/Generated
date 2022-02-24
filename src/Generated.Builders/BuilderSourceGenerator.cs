using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Generated.Builders;

[Generator]
internal class BuilderSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new BuilderSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        AddAttributeSource(context);

        // the generator infrastructure will create a receiver and populate it
        // we can retrieve the populated instance via the context
        var syntaxReceiver = (BuilderSyntaxReceiver)context.SyntaxContextReceiver;

        foreach (var classToAugment in syntaxReceiver.ClassesToAugment)
        {
            var model = context.Compilation.GetSemanticModel(classToAugment.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classToAugment, context.CancellationToken);
            var attr = symbol.GetAttributes().Where(attr => attr.AttributeClass.Name == "FlexibleBuilder").Single();

            var attribute = classToAugment.AttributeLists
                .SelectMany(list => list.Attributes)
                .Where(a => a.Name.ToString() == "FlexibleBuilder")
                .SingleOrDefault();

            var typeArg = attribute.ArgumentList.Arguments[0];
            var typeOfExpression = typeArg.Expression as Microsoft.CodeAnalysis.CSharp.Syntax.TypeOfExpressionSyntax;
            var implementedType = (INamedTypeSymbol)model.GetTypeInfo(typeOfExpression.Type).Type;

            var builder = new BuilderGenerator()
            {
                NewLine = "\n",
                Namespace = symbol.ContainingNamespace.IsGlobalNamespace ?
                    null : symbol.ContainingNamespace.ToString(),
                ClassName = symbol.Name,
                TargetClassName = implementedType.Name,
                IsPublic = true
            };

            if (implementedType.Constructors.Length == 1)
            {
                // TODO: Diagnostic error when multiple constructors
                var constructorParameters = implementedType.Constructors.Single().Parameters;
                int index = 0;
                foreach (var ctorParam in constructorParameters)
                {
                    bool isCollection = ctorParam.Type.IsCollection(out string elementNamespace, out string elementName);

                    builder.InitMembers.Add(new InitMember()
                    {
                        Name = ctorParam.Name.Capitalize(),
                        TypeNamespace = elementNamespace ?? ctorParam.Type.ContainingNamespace.Name,
                        TypeName = elementName ?? ctorParam.Type.ToSimplifiedString(),
                        IsCollection = isCollection,
                        IsArray = ctorParam.Type.Kind == SymbolKind.ArrayType,
                        HasSetter = false,
                        CtorIndex = index++
                    });
                }
            }

            foreach (var member in implementedType.GetMembers().OfType<IPropertySymbol>())
            {
                bool isCollection = member.Type.IsCollection(out string elementNamespace, out string elementName);

                if (member.IsReadOnly && !isCollection)
                {
                    // Skip readonly properties if they're not collections
                    continue;
                }

                if (builder.InitMembers.Any(im => im.Name == member.Name))
                {
                    // Skip properties that have already been added (as constructor parameters)
                    continue;
                }

                if (isCollection)
                {
                    builder.InitMembers.Add(new InitMember()
                    {
                        Name = member.Name,
                        TypeNamespace = elementNamespace,
                        TypeName = elementName,
                        IsCollection = true,
                        CollectionType = member.Type as INamedTypeSymbol,
                        IsArray = member.Type.Kind == SymbolKind.ArrayType,
                        HasSetter = !member.IsReadOnly
                    });
                }
                else
                {
                    builder.InitMembers.Add(new InitMember()
                    {
                        Name = member.Name,
                        TypeNamespace = member.Type.ContainingNamespace.Name,
                        TypeName = member.Type.ToSimplifiedString(),
                    });
                }
            }

            var source = builder.Build();
            context.AddSource("Generated.Builders.cs", SourceText.From(source, Encoding.UTF8, SourceHashAlgorithm.Sha256));
        }
    }

    private void AddAttributeSource(GeneratorExecutionContext context)
    {
        // Add the attribute code because the user should not need to reference the Generator in output assembly.
        context.AddSource("FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode());
    }
}
