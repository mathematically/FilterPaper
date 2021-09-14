using System.IO.Abstractions;
using NSubstitute;
using Paper.Domain;
using Xunit;

namespace Paper.Tests
{
    public class CommandLineTests   
    {
        private readonly IConsoleWriter console = Substitute.For<IConsoleWriter>();
        private readonly IFileSystem fileSystem = Substitute.For<IFileSystem>();
        private readonly IFileInfo inputFile = Substitute.For<IFileInfo>();
        private IFileInfo outputFile = Substitute.For<IFileInfo>();
        private bool clipboard;

        private FilterGenerator CreateSUT()
        {
            return new FilterGenerator(console, fileSystem, inputFile, outputFile, clipboard);
        }

        [Fact]
        public void When_input_file_missing_error_message_displayed()
        {
            inputFile.FullName.Returns("c:\\test.cs");
            inputFile.Exists.Returns(false);
            outputFile.FullName.Returns("c:\\test.xml");
            clipboard = true;

            CreateSUT().Execute();

            console.Received().Write(Arg.Is("Loot filter definition file c:\\test.cs does not exist."));
            console.DidNotReceive().Write(Arg.Is("Loot filter written to c:\\test.xml."));
            console.DidNotReceive().Write(Arg.Is("Loot filter copied to clipboard."));
        }

        [Fact]
        public void When_output_file_not_specified_use_input_file_with_xml_extension()
        {
            inputFile.FullName.Returns("c:\\test.cs");
            inputFile.Exists.Returns(true);
            outputFile = null;
            clipboard = false;

            CreateSUT().Execute();

            console.Received().Write(Arg.Is("Loot filter written to c:\\test.xml."));
            console.DidNotReceive().Write(Arg.Is("Loot filter copied to clipboard."));
        }

        [Fact]
        public void When_clipboard_selected_but_no_output_file_write_to_clipboard()
        {
            inputFile.FullName.Returns("c:\\test.cs");
            inputFile.Exists.Returns(true);
            outputFile = null;
            clipboard = true;

            CreateSUT().Execute();

            console.Received().Write(Arg.Is("Loot filter copied to clipboard."));
            console.DidNotReceive().Write(Arg.Is("Loot filter written to c:\\test.xml."));
        }

        [Fact]
        public void When_clipboard_selected_and_output_file_write_to_both()
        {
            inputFile.FullName.Returns("c:\\test.cs");
            inputFile.Exists.Returns(true);
            outputFile.FullName.Returns("c:\\test.xml");
            clipboard = true;

            CreateSUT().Execute();

            console.Received().Write(Arg.Is("Loot filter copied to clipboard."));
            console.Received().Write(Arg.Is("Loot filter written to c:\\test.xml."));
        }
    }
}
