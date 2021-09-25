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
                new Argument<FileInfo>("inputFile", "The .cs file containing the filter definition to be compiled."),
                new Option<FileInfo>(new[] { "--outputFile", "-f" }, "The successfully compiled filter will be written to the named file. If no output file is specified and the clipboard flag is not used the XML will be output file will use the name and location of the input file but with a .xml extension."),
                new Option(new[] { "--clipboard", "-c" }, "The successfully compiled filter will be output to the clipboard."),
            };

            command.Handler = CommandHandler.Create<FileInfo, FileInfo, bool>((inputFile, outputFile, clipboard) =>
            {
                IConsoleWriter consoleWriter = new ConsoleWriter();
                IFileSystem fileSystem = new FileSystem();
                IFileInfo inputFileInfo = new FileInfoWrapper(fileSystem, inputFile);
                IFileInfo outputFileInfo = outputFile == null ? null : new FileInfoWrapper(fileSystem, outputFile);

                var config = new Configuration().Build(
                    consoleWriter,
                    fileSystem, 
                    inputFileInfo, 
                    outputFileInfo, 
                    clipboard);

                if (config == Configuration.Error)
                {
                    return;
                }

                new FilterCompiler(config).Execute();
            });

            return command.InvokeAsync(args).Result;
        }
    }
}
