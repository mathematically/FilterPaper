using System.IO.Abstractions;
using NSubstitute;
using Xunit;

namespace Paper.Tests
{
    public class ConfigurationTests   
    {
        private readonly IConsoleWriter console = Substitute.For<IConsoleWriter>();
        private readonly IFileSystem fileSystem = Substitute.For<IFileSystem>();
        private readonly IFileInfo inputFile = Substitute.For<IFileInfo>();
        private IFileInfo outputFile = Substitute.For<IFileInfo>();
        private bool clipboard;

        [Fact]
        public void When_input_file_missing_then_error_message_is_displayed()
        {
            inputFile.FullName.Returns("c:\\test.cs");
            inputFile.Exists.Returns(false);
            outputFile.FullName.Returns("c:\\test.xml");
            clipboard = true;

            var config = new Configuration().Build(console, fileSystem, inputFile, outputFile, clipboard);

            Assert.Equal(Configuration.Error, config);

            console.Received().WriteLine(Arg.Is("Loot filter definition file c:\\test.cs does not exist."));
            console.DidNotReceive().WriteLine(Arg.Is("Loot filter written to c:\\test.xml."));
            console.DidNotReceive().WriteLine(Arg.Is("Loot filter copied to clipboard."));
        }

        [Fact]
        public void When_output_file_not_specified_then_use_input_file_with_xml_extension()
        {
            inputFile.FullName.Returns("c:\\test.cs");
            inputFile.Exists.Returns(true);
            outputFile = null;
            clipboard = false;

            new Configuration().Build(console, fileSystem, inputFile, outputFile, clipboard);

            console.Received().WriteLine(Arg.Is("Loot filter written to c:\\test.xml."));
            console.DidNotReceive().WriteLine(Arg.Is("Loot filter copied to clipboard."));
        }

        [Fact]
        public void When_output_file_not_specified_but_clipboard_selected_then_write_to_the_clipboard()
        {
            inputFile.FullName.Returns("c:\\test.cs");
            inputFile.Exists.Returns(true);
            outputFile = null;
            clipboard = true;

            new Configuration().Build(console, fileSystem, inputFile, outputFile, clipboard);

            console.Received().WriteLine(Arg.Is("Loot filter copied to clipboard."));
            console.DidNotReceive().WriteLine(Arg.Is("Loot filter written to c:\\test.xml."));
        }

        [Fact]
        public void When_output_file_specified_and_clipboard_selected_then_write_to_both()
        {
            inputFile.FullName.Returns("c:\\test.cs");
            inputFile.Exists.Returns(true);
            outputFile.FullName.Returns("c:\\test.xml");
            clipboard = true;

            new Configuration().Build(console, fileSystem, inputFile, outputFile, clipboard);

            console.Received().WriteLine(Arg.Is("Loot filter copied to clipboard."));
            console.Received().WriteLine(Arg.Is("Loot filter written to c:\\test.xml."));
        }
    }
}
