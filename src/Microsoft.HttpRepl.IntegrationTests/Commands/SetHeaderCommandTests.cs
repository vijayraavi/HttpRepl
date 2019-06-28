using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class SetHeaderCommandTests : ICommandTestHelper<SetHeaderCommand>
    {
        public SetHeaderCommandTests()
            : base(new SetHeaderCommand())
        {
        }

        [Fact]
        public void CanHandle_WithParseResultSectionsLessThanTwo_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "set");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithFirstParseResultSectionNotEqualToName_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "test header name");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithSecondParseResultSectionNotEqualToSubCommand_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "set base name");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "set header name");

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            string result = GetHelpSummary();

            Assert.Equal(Strings.SetHeaderCommand_Description, result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsDescription()
        {
            string parseResultSections = "set header";
            string result = GetHelpDetails(parseResultSections);

            Assert.Equal(Strings.SetHeaderCommand_HelpDetails, result);
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyThreeValidParseResultSections_DoesNotUpdateHeaders()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set header test");
            HttpState httpState = new HttpState(null);
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            await setHeaderCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> headers = httpState.Headers;
            KeyValuePair<string, IEnumerable<string>> firstHeader = headers.First();

            Assert.Single(httpState.Headers);
            Assert.Equal("User-Agent", firstHeader.Key);
            Assert.Equal("HTTP-REPL", firstHeader.Value.First());
        }

        [Fact]
        public async Task ExecuteAsync_WithMoreThanThreeValidParseResultSections_AddsEntryToHeaders()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set header name value1 value2");
            HttpState httpState = new HttpState(null);
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            await setHeaderCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Dictionary<string, IEnumerable<string>> headers = httpState.Headers;
            KeyValuePair<string, IEnumerable<string>> firstHeader = headers.First();
            KeyValuePair<string, IEnumerable<string>> secondHeader = headers.ElementAt(1);

            Assert.Equal(2, httpState.Headers.Count);
            Assert.Equal("User-Agent", firstHeader.Key);
            Assert.Equal("HTTP-REPL", firstHeader.Value.First());

            Assert.Equal("name", secondHeader.Key);
            Assert.Equal("value1", secondHeader.Value.First());
            Assert.Equal("value2", secondHeader.Value.ElementAt(1));
        }

        [Fact]
        public void Suggest_WithNoParseResultSections_ReturnsName()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create(string.Empty);
            HttpState httpState = new HttpState(null);
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithOneParseResultSectionAndSelectedSectionGreaterAtZero_ReturnsName()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set", 0);
            HttpState httpState = new HttpState(null);
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithOneParseResultSectionAndSelectedSectionGreaterThanZero_ReturnsName()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set", 3);
            HttpState httpState = new HttpState(null);
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("set", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanOneParseResultSectionAndSelectedSectionGreaterThanZero_ReturnsName()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set header", 10);
            HttpState httpState = new HttpState(null);
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("header", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanTwoParseResultSectionsAndSelectedSectionGreaterThanTwo_ReturnsHeaderCompletion()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set header O", 12);
            HttpState httpState = new HttpState(null);
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Single(suggestions);
            Assert.Equal("Origin", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanThreeParseResultSectionsAndSelectedSectionAtThree_ReturnsValueCompletion()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set header CONTENT-TYPE t", 25);
            HttpState httpState = new HttpState(null);
            IDirectoryStructure directoryStructure = new MockDirectoryStructure("testMethod", "testContentType", "testBody");
            httpState.SwaggerStructure = directoryStructure;
            httpState.BaseAddress = new Uri("http://localhost:5050/");
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            List<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult).ToList();

            Assert.Single(suggestions);
            Assert.Equal("testContentType", suggestions.First());
        }

        [Fact]
        public void Suggest_WithMoreThanThreeParseResultSectionsAndNoMatchingCompletions_ReturnsNothing()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set header CONTENT-TYPE z", 25);
            HttpState httpState = new HttpState(null);
            IDirectoryStructure directoryStructure = new MockDirectoryStructure("testMethod", "testContentType", "testBody");
            httpState.SwaggerStructure = directoryStructure;
            httpState.BaseAddress = new Uri("http://localhost:5050/");
            SetHeaderCommand setHeaderCommand = new SetHeaderCommand();

            IEnumerable<string> suggestions = setHeaderCommand.Suggest(shellState, httpState, parseResult);

            Assert.Empty(suggestions);
        }
    }
}
