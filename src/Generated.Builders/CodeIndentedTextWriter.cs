using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace Generated.Builders;

class CodeIndentedTextWriter : IndentedTextWriter
{
    private bool _noSeparatorNeeded = false;

    public CodeIndentedTextWriter(TextWriter writer) : base(writer)
    {
    }

    public void Constructor(string className)
    {
        WriteSeparatorLine();
        WriteLine($"private {className}()");
        BeginScope();
        EndScope();
    }

    public void Init(string className)
    {
        WriteSeparatorLine();
        WriteLine($"public static {className} Init()");
        BeginScope();
        WriteLine($"return new {className}();");
        EndScope();
    }

    public void Members<T>(IEnumerable<T> members, Action<T> action)
        => ForeachWithSeparator(members, action);

    public void Usings(IEnumerable<string> nss)
        => ForeachWithSeparator(nss, ns => WriteLine($"using {ns};"));

    public void ForeachWithSeparator<T>(IEnumerable<T> elements, Action<T> action)
    {
        bool anyWritter = false;

        foreach (var element in elements)
        {
            action(element);
            anyWritter = true;
        }

        if (anyWritter)
        {
            WriteSeparatorLine();
        }
    }

    public void Member(string type, string name)
    {
        WriteLine($"private {type} {name};");
    }

    public void BeginScope()
    {
        WriteLine("{");
        Indent++;
        _noSeparatorNeeded = true;
    }

    public void EndScope(string text = null)
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

    public IDisposable Namespace(string @namespace)
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

    public void With(string className, string name, string typeName, string valueMemberName)
    {
        WriteSeparatorLine();
        WriteLine($"public {className} With{name}({typeName} value)");
        BeginScope();
        WriteLine($"{valueMemberName} = value;");
        WriteLine($"return this;");
        EndScope();
    }

    public void WithCollection(string className, string name, string typeName, string valueMemberName)
    {
        WriteSeparatorLine();
        WriteLine($"public {className} With{name}(IEnumerable<{typeName}> values)");
        BeginScope();
        WriteLine($"{valueMemberName} = new List<{typeName}>(values);");
        WriteLine($"return this;");
        EndScope();

        WriteSeparatorLine();
        WriteLine($"public {className} AddTo{name}({typeName} value)");
        BeginScope();
        WriteLine($"if ({valueMemberName} == null)");
        BeginScope();
        WriteLine($"{valueMemberName} = new List<{typeName}>();");
        EndScope();
        WriteSeparatorLine();
        WriteLine($"{valueMemberName}.Add(value);");
        WriteSeparatorLine();
        WriteLine($"return this;");
        EndScope();
    }

    internal void WithDictionary(string className, string name, string keyTypeName, string valueTypeName, string valueMemberName)
    {
        WriteSeparatorLine();
        WriteLine($"public {className} With{name}(IDictionary<{keyTypeName}, {valueTypeName}> values)");
        BeginScope();
        WriteLine($"{valueMemberName} = new Dictionary<{keyTypeName}, {valueTypeName}>(values);");
        WriteLine($"return this;");
        EndScope();

        WriteSeparatorLine();
        WriteLine($"public {className} AddTo{name}({keyTypeName} key, {valueTypeName} value)");
        BeginScope();
        WriteLine($"if ({valueMemberName} == null)");
        BeginScope();
        WriteLine($"{valueMemberName} = new Dictionary<{keyTypeName}, {valueTypeName}>();");
        EndScope();
        WriteSeparatorLine();
        WriteLine($"{valueMemberName}.Add(key, value);");
        WriteSeparatorLine();
        WriteLine($"return this;");
        EndScope();
    }

    public void Build(string className, Action constructorParameterInitialization, Action memberInitialization, Action postMemberInitialization)
    {
        WriteSeparatorLine();
        WriteLine($"public {className} Build()");
        BeginScope();

        bool needsVariable = postMemberInitialization != null;

        Write(needsVariable?  "var ret = " : "return ");
        Write($"new {className}(");

        if (constructorParameterInitialization != null)
        {
            constructorParameterInitialization();
        }

        if (memberInitialization != null)
        {
            WriteLine(")");
            BeginScope();
            memberInitialization();
            EndScope(";");
        }
        else
        {
            WriteLine(");");
        }

        if (needsVariable)
        {
            postMemberInitialization();
            WriteLine("return ret;");
        }

        EndScope();
    }

    private sealed class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private sealed class EndScopeDisposable : IDisposable
    {
        private CodeIndentedTextWriter _writer;

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
