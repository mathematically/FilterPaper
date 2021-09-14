using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;
using Paper.Domain;

namespace Paper
{
    static class Program
    {
        static int Main(string[] args)
        {
            var command = new RootCommand
            {
                new Argument<FileInfo>("inputFile", "The .cs file containing the filter definition to be compiled."),
                new Option<FileInfo>(new[] { "--outputFile", "-f" }, "The successfully compiled filter will be written to the named file."),
                new Option(new[] { "--clipboard", "-c" }, "The successfully compiled filter will be output to the clipboard."),
            };

            command.Handler = CommandHandler.Create<FileInfo, FileInfo, bool>((inputFile, outputFile, clipboard) =>
            {
                IConsoleWriter console = new ConsoleWriter();
                IFileSystem fileSystem = new FileSystem();
                IFileInfo inputFileInfo = new FileInfoWrapper(fileSystem, inputFile);
                IFileInfo outputFileInfo = outputFile == null ? null : new FileInfoWrapper(fileSystem, outputFile);

                new FilterGenerator(console, fileSystem, inputFileInfo, outputFileInfo, clipboard).Execute();
            });

            return command.InvokeAsync(args).Result;
        }
    }
}
