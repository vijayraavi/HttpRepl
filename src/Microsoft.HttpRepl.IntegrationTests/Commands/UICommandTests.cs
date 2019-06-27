using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class UICommandTests
    {
        [Fact]
        public void CanHandle_WithParseResultSectionsGreaterThan1_ReturnsNull()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1 section2");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState();
            UICommand uiCommand = new UICommand();

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithCommandNameNotEqualToUI_ReturnsNull()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("section1");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState();
            UICommand uiCommand = new UICommand();

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidCommand_ReturnsTrue()
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create("ui");
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState();
            UICommand uiCommand = new UICommand();

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }
    }
}
