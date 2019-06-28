using System;
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
    public class SetBaseCommandTests : ICommandTestHelper<SetBaseCommand>
    {
        public SetBaseCommandTests()
            : base(new SetBaseCommand())
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
            bool? result = CanHandle(parseResultSections: "set header name");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidInput_ReturnsTrue()
        {
            bool? result = CanHandle(parseResultSections: "set base \"https://localhost:44366/\"");

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            string result = GetHelpSummary();

            Assert.Equal(Strings.SetBaseCommand_Description, result);
        }

        [Fact]
        public void GetHelpDetails_ReturnsUsage()
        {
            string parseResultSections = "set base";
            string result = GetHelpDetails(parseResultSections);

            Assert.Equal(Strings.SetBaseCommand_Usage, result);
        }

        [Fact]
        public void Suggest_WithNoParseResultSections_ReturnsName()
        {
            IEnumerable<string> suggestions = GetSuggestions(parseResultSections: string.Empty);
            string expected = "set";

            Assert.Single(suggestions);
            Assert.Equal(expected, suggestions.First());
        }

        [Fact]
        public void Suggest_WithSelectedSectionAtZeroAndParseResultSectionStartsWithName_ReturnsName()
        {
            IEnumerable<string> suggestions = GetSuggestions(parseResultSections: "s");
            string expected = "set";

            Assert.Single(suggestions);
            Assert.Equal(expected, suggestions.First());
        }

        [Fact]
        public void Suggest_WithNameParseResultSectionAndSelectedSectionAtOne_ReturnsSubCommand()
        {
            IEnumerable<string> suggestions = GetSuggestions(parseResultSections: "set ", caretPosition: 4);
            string expected = "base";

            Assert.Single(suggestions);
            Assert.Equal(expected, suggestions.First());
        }

        [Fact]
        public async Task ExecuteAsync_WithExactlyTwoParseResultSections_SetsBaseAddressAndSwaggerStructureToNull()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set base");
            HttpState httpState = new HttpState(null);
            SetBaseCommand setBaseCommand = new SetBaseCommand();

            await setBaseCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.BaseAddress);
            Assert.Null(httpState.SwaggerStructure);
        }

        [Fact]
        public async Task ExecuteAsync_IfCancellationRequested_SetsSwaggerStructureToNull()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api"": {
      ""get"": {
        ""tags"": [ ""Employees"" ],
        ""operationId"": ""GetEmployee"",
        ""consumes"": [],
        ""produces"": [ ""text/plain"", ""application/json"", ""text/json"" ],
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""schema"": {
              ""uniqueItems"": false,
              ""type"": ""array""
            }
          }
        }
      }
    }
  }
}";
            string parseResultSections = "set base \"https://localhost:44366/\"";
            Uri baseAddress = new Uri("https://localhost:44366/");
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            IDirectoryStructure directoryStructure = await GetDirectoryStructure(response, parseResultSections, cts.Token, baseAddress).ConfigureAwait(false);

            Assert.Null(directoryStructure);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyUri_WritesErrorToConsole()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("set base ");
            HttpState httpState = new HttpState(null);
            SetBaseCommand setBaseCommand = new SetBaseCommand();

            await setBaseCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidInput_CreateDirectoryStructureForSwaggerEndpoint()
        {
            string response = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api"": {
      ""get"": {
        ""tags"": [ ""Employees"" ],
        ""operationId"": ""GetEmployee"",
        ""consumes"": [],
        ""produces"": [ ""text/plain"", ""application/json"", ""text/json"" ],
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""schema"": {
              ""uniqueItems"": false,
              ""type"": ""array""
            }
          }
        }
      }
    }
  }
}";
            string parseResultSections = "set base \"https://localhost:44366/\"";
            Uri baseAddress = new Uri("https://localhost:44366/");
            IDirectoryStructure directoryStructure = await GetDirectoryStructure(response, parseResultSections, CancellationToken.None, baseAddress).ConfigureAwait(false);
            List<string> directoryNames = directoryStructure.DirectoryNames.ToList();
            string expectedDirectoryName = "api";

            Assert.Single(directoryNames);
            Assert.Equal("api", directoryNames.First());

            IDirectoryStructure childDirectoryStructure = directoryStructure.GetChildDirectory(expectedDirectoryName);

            Assert.Empty(childDirectoryStructure.DirectoryNames);
        }
    }
}
