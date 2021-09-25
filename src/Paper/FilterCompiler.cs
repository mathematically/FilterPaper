using static System.AppContext;
using System;
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
    public class FilterCompiler
    {
        private readonly IConsoleWriter console;
        private readonly IFileSystem fileSystem;
        private readonly IFileInfo inputFile;
        private readonly IFileInfo outputFile;
        private readonly bool clipboard;

        public FilterCompiler((IConsoleWriter, IFileSystem, IFileInfo, IFileInfo, bool) config)
        {
            (console, fileSystem, inputFile, outputFile, clipboard) = config;
        }

        private const string CompiledDllName = "temp-compiled-filter.dll";

        private static readonly string[] TrustedPlatformAssemblies = GetData( "TRUSTED_PLATFORM_ASSEMBLIES" )
            ?.ToString()
            ?.Split( ";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        private static MetadataReference[] GetReferences()
        {
            MetadataReference GetAssemblyReference(string dllName)
            {
                return MetadataReference.CreateFromFile(TrustedPlatformAssemblies.Single(
                    a => a.Contains(dllName, StringComparison.OrdinalIgnoreCase)));
            }

            return new[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location), 

                GetAssemblyReference("system.runtime.dll"), 
                GetAssemblyReference("system.io.dll"), 
                GetAssemblyReference("system.private.xml.dll")
            };
        }

        public void Execute()
        {
            var filterCode = fileSystem.File.ReadAllText(inputFile.FullName);

            var compilation = CSharpCompilation.Create(CompiledDllName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(GetReferences())
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(filterCode));

            var compiledDllPath = fileSystem.Path.Combine(Directory.GetCurrentDirectory(), CompiledDllName);
            var compilationResult = compilation.Emit(compiledDllPath);

            if(compilationResult.Success)
            {
                if (AssemblyLoadContext.Default.LoadFromAssemblyPath(compiledDllPath)
                    .GetType("Paper.TestClass")
                    .GetMethod("Execute")
                    .Invoke(null, null) is XmlDocument filterXml)
                {
                    if (outputFile != null)
                    {
                        using var outputFileWriter = new XmlTextWriter(outputFile.FullName, null);
                        outputFileWriter.Formatting = Formatting.Indented;
                        filterXml.Save(outputFileWriter);
                    }

                    if (clipboard)
                    {
                        var stringWriter = new StringWriter();
                        var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };
                        filterXml.WriteTo(xmlTextWriter);

                        ClipboardService.SetText(stringWriter.ToString());
                    }
                }
                else
                {
                    console.WriteLine("Class compiled but produced null xml results.");
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
