using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class RunCommandTests : ICommandTestHelper<RunCommand>
    {
        public RunCommandTests(): base(new RunCommand())
        {
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            string result = GetHelpSummary();

            Assert.Equal(Strings.RunCommand_Description, result);
        }

        [Fact]
        public void GetHelpDetails_WithEmptyParseResultSection_ReturnsNull()
        {
            string result = GetHelpDetails(parseResultSections: string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            string result = GetHelpDetails(parseResultSections: "section1 section2 section3");

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithValidInput_ReturnsDescription()
        {
            string parseResultSections = "run InputFileForRunCommand.txt";
            string result = GetHelpDetails(parseResultSections);

            Assert.Equal(Strings.RunCommand_Usage, result);
        }

        [Fact]
        public async Task ExecuteAsync_IfFileDoesNotExist_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            await ExecuteAsyncWithInvalidParseResultSections(parseResultSections: "run InputFileForRunCommand.txt", shellState);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidInput_ExecutesTheCommandsInTheScript()
        {
            string pathToScript = Path.Combine(Directory.GetCurrentDirectory(), "InputFileForRunCommand.txt");
            string commands = @"set header name value1 value2";

            if (!File.Exists(pathToScript))
            {
                File.WriteAllText(pathToScript, commands);
            }

            string parseResultSections = "run " + pathToScript;
            HttpState httpState = new HttpState(null);
            IShellState shellState = GetShellState(commands, httpState);
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            RunCommand runCommand = new RunCommand();

            await runCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> headers = httpState.Headers;
            KeyValuePair<string, IEnumerable<string>> firstHeader = headers.First();
            KeyValuePair<string, IEnumerable<string>> secondHeader = headers.ElementAt(1);

            Assert.Equal(2, httpState.Headers.Count);
            Assert.Equal("User-Agent", firstHeader.Key);
            Assert.Equal("HTTP-REPL", firstHeader.Value.First());

            Assert.Equal("name", secondHeader.Key);
            Assert.Equal("value1", secondHeader.Value.First());
            Assert.Equal("value2", secondHeader.Value.ElementAt(1));

            File.Delete(pathToScript);
        }

        private IShellState GetShellState(string inputBuffer, HttpState httpState)
        {
            var defaultCommandDispatcher = DefaultCommandDispatcher.Create(x => { }, httpState);
            defaultCommandDispatcher.AddCommand(new SetHeaderCommand());

            Mock<IConsoleManager> mockConsoleManager = new Mock<IConsoleManager>();
            Mock<ICommandHistory> mockCommandHistory = new Mock<ICommandHistory>();
            MockInputManager mockInputManager = new MockInputManager(inputBuffer);

            ShellState shellState = new ShellState(defaultCommandDispatcher,
                consoleManager: mockConsoleManager.Object,
                commandHistory: mockCommandHistory.Object,
                inputManager: mockInputManager);

            Shell shell = new Shell(shellState);

            return shell.ShellState;
        }
    }
}
