using Microsoft.CodeAnalysis;

namespace Generated.Builders;

internal class InitMember
{
    public string Name { get; set; }
    public string TypeNamespace { get; set; }
    public string TypeName { get; set; }
    public bool IsCollection { get; set; }
    public bool HasSetter { get; set; }
    public bool IsArray { get; set; }
    public int? CtorIndex { get; set; }
    public INamedTypeSymbol CollectionType { get; internal set; }
    public bool SkipDefaultValue { get; internal set; }

    internal string ValueMemberName => "_" + Name.Substring(0, 1).ToLower() + Name.Substring(1);
}
