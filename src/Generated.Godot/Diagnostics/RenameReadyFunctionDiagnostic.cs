using Microsoft.CodeAnalysis;

namespace Generated.Godot;

internal static class RenameReadyFunctionDiagnostic
{
    public static Diagnostic Create(Location location)
        => Diagnostic.Create(new DiagnosticDescriptor(Id, Title, Message, "Usage", DiagnosticSeverity.Error, true, null, null), location);

    public const string Id = "GG1";
    public const string Message = "The \"_EnterTree\" function must be renamed to \"EnterTree\".";
    public const string Title = "Duplicate _EnterTree method definition.";
}
