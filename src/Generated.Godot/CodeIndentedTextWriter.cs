using System;
using System.CodeDom.Compiler;
using System.IO;

namespace Generated.Godot;

class CodeIndentedTextWriter : IndentedTextWriter
{
    private bool _noSeparatorNeeded = false;

    public CodeIndentedTextWriter(TextWriter writer) : base(writer)
    {
    }

    public void BeginScope()
    {
        WriteLine("{");
        Indent++;
        _noSeparatorNeeded = true;
    }

    public void EndScope(string? text = null)
    {
        Indent--;
        WriteLine("}" + text);
    }

    public void WriteSeparatorLine()
    {
        if (!_noSeparatorNeeded)
        {
            WriteLineNoTabs(string.Empty);
            _noSeparatorNeeded = true;
        }
    }

    public IDisposable Class(string name, bool isPublic)
    {
        WriteLine($"{(isPublic ? "public" : "internal")} partial class {name}");
        BeginScope();
        return new EndScopeDisposable(this);
    }

    public IDisposable Namespace(string? @namespace)
    {
        if (string.IsNullOrEmpty(@namespace))
        {
            return new NoopDisposable();
        }

        WriteLine($"namespace {@namespace}");
        BeginScope();
        return new EndScopeDisposable(this);
    }

    protected override void OutputTabs()
    {
        base.OutputTabs();
        _noSeparatorNeeded = false;
    }

    private sealed class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private sealed class EndScopeDisposable : IDisposable
    {
        private CodeIndentedTextWriter? _writer;

        public EndScopeDisposable(CodeIndentedTextWriter writer)
        {
            _writer = writer;
        }

        public void Dispose()
        {
            _writer?.EndScope();
            _writer = null;
        }
    }
}
