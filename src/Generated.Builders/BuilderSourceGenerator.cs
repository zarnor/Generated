using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

            var attribute = classToAugment.AttributeLists
                .SelectMany(list => list.Attributes)
                .Where(a => a.Name.ToString() == "FlexibleBuilder" ||
                            a.Name.ToString() == "Generated.Builders.FlexibleBuilder")
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

                var ctorMember = builder.InitMembers.FirstOrDefault(im => im.Name == member.Name);

                var walker = new PropertyHasDefaultValueSyntaxWalker(member.Name);
                walker.Visit(classToAugment.Parent);
                bool hasDefaultValue = walker.HasDefaultValue;

                if (ctorMember != null)
                {
                    // Skip properties that have already been added (as constructor parameters)

                    if (isCollection)
                    {
                        ctorMember.TypeNamespace = elementNamespace;
                        ctorMember.TypeName = elementName;
                        ctorMember.CollectionType = member.Type as INamedTypeSymbol;
                    }

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
                        HasSetter = !member.IsReadOnly,
                        SkipDefaultValue = hasDefaultValue
                    });
                }
                else
                {
                    builder.InitMembers.Add(new InitMember()
                    {
                        Name = member.Name,
                        TypeNamespace = member.Type.ContainingNamespace.ToString(),
                        TypeName = member.Type.ToSimplifiedString(),
                        SkipDefaultValue = hasDefaultValue
                    });
                }
            }

            var source = builder.Build();
            context.AddSource($"{symbol.Name}.g.cs", SourceText.From(source, Encoding.UTF8, SourceHashAlgorithm.Sha256));
        }
    }

    private void AddAttributeSource(GeneratorExecutionContext context)
    {
        // Add the attribute code because the user should not need to reference the Generator in output assembly.
        context.AddSource("FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode());
    }
}

class PropertyHasDefaultValueSyntaxWalker : CSharpSyntaxWalker
{
    private readonly string _propertyName;

    public PropertyHasDefaultValueSyntaxWalker(string propertyName)
    {
        _propertyName = propertyName;
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (HasDefaultValue)
        {
            return;
        }

        if (node.Identifier.ValueText == _propertyName)
        {
            var equals = node.ChildNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();
            HasDefaultValue = equals != null;
            return;
        }

        base.VisitPropertyDeclaration(node);
    }

    public bool HasDefaultValue { get; set; }
}
