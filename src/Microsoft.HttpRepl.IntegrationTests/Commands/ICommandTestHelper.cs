using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class ICommandTestHelper<T>
        where T : ICommand<HttpState, ICoreParseResult>
    {
        private readonly T _command;

        public ICommandTestHelper(T command)
        {
            _command = command;
        }

        protected bool? CanHandle(string parseResultSections)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);

            return _command.CanHandle(shellState, httpState, parseResult);
        }

        protected string GetHelpDetails(string parseResultSections)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);

            return _command.GetHelpDetails(shellState, httpState, parseResult);
        }

        protected string GetHelpSummary()
        {
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);

            return _command.GetHelpSummary(shellState, httpState);
        }

        protected async Task ExecuteAsyncWithInvalidParseResultSections(string parseResultSections, IShellState shellState, string baseAddress = null)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);
            HttpState httpState = new HttpState(null);
            httpState.BaseAddress = null;

            await _command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);
        }

        protected IEnumerable<string> GetSuggestions(string parseResultSections, int caretPosition = 0)
        {
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections, caretPosition);
            MockedShellState shellState = new MockedShellState();
            HttpState httpState = new HttpState(null);
            return _command.Suggest(shellState, httpState, parseResult);
        }

        protected void VerifyErrorMessageWasWrittenToConsoleManagerError(IShellState shellState)
        {
            Mock<IWritable> error = Mock.Get(shellState.ConsoleManager.Error);

            error.Verify(s => s.WriteLine(It.IsAny<string>()), Times.Once);
        }

        protected async Task<IDirectoryStructure> GetDirectoryStructure(string response, string parseResultSections, CancellationToken cancellationToken, Uri baseAddress = null)
        {
            MockedShellState shellState = new MockedShellState();
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            responseMessage.Content = new MockHttpContent(response);
            MockHttpMessageHandler messageHandler = new MockHttpMessageHandler(responseMessage);
            HttpClient client = new HttpClient(messageHandler);
            HttpState httpState = new HttpState(client);
            httpState.BaseAddress = baseAddress;
            ICoreParseResult parseResult = CoreParseResultHelper.Create(parseResultSections);

            await _command.ExecuteAsync(shellState, httpState, parseResult, cancellationToken);

            return httpState.SwaggerStructure;
        }
    }
}
