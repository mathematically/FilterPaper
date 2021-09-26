using static System.AppContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TextCopy;

namespace Paper
{
    public static class ReferencesFactory
    {
        private static readonly string[] TrustedPlatformAssemblies = GetData( "TRUSTED_PLATFORM_ASSEMBLIES" )
            ?.ToString()
            ?.Split( ";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        static MetadataReference GetAssemblyReference(string dllName)
        {
            return MetadataReference.CreateFromFile(TrustedPlatformAssemblies.Single(
                a => a.Contains(dllName, StringComparison.OrdinalIgnoreCase)));
        }

        private static MetadataReference[] GetReferences()
        {

            return new[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location), 

                GetAssemblyReference("system.runtime.dll"), 
                GetAssemblyReference("system.io.dll"), 
                GetAssemblyReference("system.private.xml.dll")
            };
        }

        public static MetadataReference[] Create(params string[] assemblyNames)
        {
            List<MetadataReference> references = new()
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };

            references.AddRange(assemblyNames.Select(GetAssemblyReference));

            return references.ToArray();
        }
    }

    public class FilterCompiler
    {
        private const string CompiledDllName = "temp-compiled-filter.dll";

        private const string Namespace = "LastEpoch";
        private const string ClassName = "Filter";
        private const string MethodName = "Execute";

        private readonly IConsoleWriter console;
        private readonly IFileSystem fileSystem;
        private readonly IFileInfo inputFile;
        private readonly IFileInfo outputFile;
        private readonly bool clipboard;

        public FilterCompiler(IConsoleWriter console, IFileSystem fileSystem, IFileInfo inputFile, IFileInfo outputFile, bool clipboard)
        {
            this.console = console;
            this.fileSystem = fileSystem;
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.clipboard = clipboard;
        }

        public void Execute()
        {
            var filterCode = fileSystem.File.ReadAllText(inputFile.FullName);

            var assemblyReferences = ReferencesFactory.Create(
                "system.runtime.dll", 
                "system.io.dll", 
                "system.private.xml.dll");

            var compilation = CSharpCompilation.Create(CompiledDllName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(assemblyReferences)
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(filterCode));

            var compiledDllPath = fileSystem.Path.Combine(Directory.GetCurrentDirectory(), CompiledDllName);
            var compilationResult = compilation.Emit(compiledDllPath);

            if(compilationResult.Success)
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(compiledDllPath);

                var classType = assembly.GetType($"{Namespace}.{ClassName}");
                if (classType == null)
                {
                    console.WriteLine($"Assembly compiled but could not find class {ClassName} in namespace {Namespace}.");
                    return;
                }

                var method = classType.GetMethod(MethodName);
                if (method == null)
                {
                    console.WriteLine($"Assembly compiled but could not find method {MethodName} in class {Namespace}.{ClassName}.");
                    return;
                }

                if (method.Invoke(null, null) is XmlDocument filterXml)
                {
                    if (outputFile != null)
                    {
                        using var outputFileWriter = new XmlTextWriter(outputFile.FullName, null);
                        outputFileWriter.Formatting = Formatting.Indented;

                        filterXml.Save(outputFileWriter);

                        console.WriteLine($"Filter written to {outputFile.FullName}");
                    }

                    if (clipboard)
                    {
                        var stringWriter = new StringWriter();
                        var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };
                        filterXml.WriteTo(xmlTextWriter);

                        ClipboardService.SetText(stringWriter.ToString());

                        console.WriteLine($"Filter copied to clipboard.");
                    }
                }
                else
                {
                    console.WriteLine($"Invoke of method {MethodName} in class {Namespace}.{ClassName}.");
                }
            }
            else
            {
                foreach (var diagnostic in compilationResult.Diagnostics)
                {
                    console.WriteLine($@"ID: {diagnostic.Id}, Message: {diagnostic.GetMessage()}, Location: {diagnostic.Location.GetLineSpan()}, Severity: {diagnostic.Severity}");
                }
            }
        }
    }
}
