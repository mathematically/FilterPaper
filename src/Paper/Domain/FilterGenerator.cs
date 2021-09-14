using System.IO;
using System.IO.Abstractions;

namespace Paper.Domain
{
    public class FilterCompiler
    {

    }

    public class FilterWriter
    {

    }

    public class FilterGenerator
    {
        private readonly IConsoleWriter console;
        private readonly IFileSystem fileSystem;
        private readonly IFileInfo inputFile;
        private IFileInfo outputFile;
        private readonly bool clipboard;

        public FilterGenerator(IConsoleWriter console, IFileSystem fileSystem, IFileInfo inputFile, IFileInfo outputFile = null, bool clipboard = false)
        {
            this.console = console;
            this.fileSystem = fileSystem;
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.clipboard = clipboard;
        }

        public void Execute()
        {
            console.Write($"Filter Paper v{GetType().Assembly.GetName().Version}");

            if (!inputFile.Exists)
            {
                console.Write($"Loot filter definition file {inputFile.FullName} does not exist.");
                return;
            }

            if (outputFile == null && !clipboard)
            {
                outputFile ??= new FileInfoWrapper(fileSystem, new FileInfo(Path.ChangeExtension(inputFile.FullName, "xml") ?? string.Empty));
            }

            if (clipboard)
            {
                console.Write($"Loot filter copied to clipboard.");
            }

            if (outputFile != null)
            {
                console.Write($"Loot filter written to {outputFile.FullName}.");
            }
        }
    }
}