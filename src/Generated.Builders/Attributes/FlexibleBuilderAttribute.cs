using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Generated.Builders;

[AttributeUsage(AttributeTargets.Class)]
public class FlexibleBuilderAttribute : Attribute
{
    public FlexibleBuilderAttribute(Type type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public Type Type { get; set; }

    public static SourceText GetSourceCode()
    {
        return SourceText.From(@"using System;

namespace Generated.Builders
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FlexibleBuilderAttribute : Attribute
    {
        public FlexibleBuilderAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}
", Encoding.UTF8, SourceHashAlgorithm.Sha256);
    }
}
