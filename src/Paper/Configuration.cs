﻿using System;
using System.IO;
using System.IO.Abstractions;

namespace Paper
{
    public class Configuration
    {
        public static readonly ValueTuple<IConsoleWriter, IFileSystem, IFileInfo, IFileInfo, bool> Error = (null, null, null, null, false);

        public (IConsoleWriter, IFileSystem, IFileInfo, IFileInfo, bool) 
            Build(IConsoleWriter console, IFileSystem fileSystem, IFileInfo inputFile, IFileInfo outputFile = null, bool clipboard = false)
        {
            console.WriteLine($"Filter Paper v{GetType().Assembly.GetName().Version}");

            if (!inputFile.Exists)
            {
                console.WriteLine($"Loot filter definition file {inputFile.FullName} does not exist.");
                return Error;
            }

            if (outputFile == null && !clipboard)
            {
                outputFile = new FileInfoWrapper(fileSystem, new FileInfo(Path.ChangeExtension(inputFile.FullName, "xml") ?? string.Empty));
            }

            if (clipboard)
            {
                console.WriteLine($"Loot filter copied to clipboard.");
            }

            if (outputFile != null)
            {
                console.WriteLine($"Loot filter written to {outputFile.FullName}.");
            }

            return ( console, fileSystem, inputFile, outputFile, clipboard );
        }
    }
}