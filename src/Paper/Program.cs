using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;

namespace Paper
{
    static class Program
    {
        static int Main(string[] args)
        {
            var command = new RootCommand
            {
                new Argument<FileInfo>("inputFile", 
                    "The .cs file containing the filter definition to be compiled."),
                new Option<FileInfo>(new[] { "--outputFile", "-f" }, 
                    "The successfully compiled filter will be written to the named file. If no output file is specified and the clipboard flag is not used the XML will be output file will use the name and location of the input file but with a .xml extension."),
                new Option(new[] { "--clipboard", "-c" }, 
                    "The successfully compiled filter will be output to the clipboard."),
            };

            command.Handler = CommandHandler.Create<FileInfo, FileInfo, bool>((inputFile, outputFile, clipboard) =>
            {
                IConsoleWriter console = new ConsoleWriter();
                IFileSystem fileSystem = new FileSystem();
                IFileInfo inputFileInfo = new FileInfoWrapper(fileSystem, inputFile);
                IFileInfo outputFileInfo;

                if (!inputFile.Exists)
                {
                    console.WriteLine($"Loot filter definition file {inputFile.FullName} does not exist.");
                    return;
                }

                if (outputFile != null)
                {
                    outputFileInfo = new FileInfoWrapper(fileSystem, outputFile);
                }
                else
                {
                    outputFileInfo = new FileInfoWrapper(fileSystem,
                        new FileInfo(Path.ChangeExtension(inputFile.FullName, "xml") ?? string.Empty));
                }

                new FilterCompiler(console, fileSystem, inputFileInfo, outputFileInfo, clipboard).Execute();
            });

            return command.InvokeAsync(args).Result;
        }
    }
}
