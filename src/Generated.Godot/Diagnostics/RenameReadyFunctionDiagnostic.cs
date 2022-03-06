using Microsoft.CodeAnalysis;

namespace Generated.Godot;

internal static class RenameReadyFunctionDiagnostic
{
    public static Diagnostic Create(Location location)
        => Diagnostic.Create(new DiagnosticDescriptor(Id, Title, Message, "Usage", DiagnosticSeverity.Error, true, null, null), location);

    public const string Id = "GG1";
    public const string Message = "The \"_Ready\" function must be renamed to \"Ready\".";
    public const string Title = "Duplicate _Ready method definition.";
}
