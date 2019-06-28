using System.Collections.Generic;
using System.Linq;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Resources;
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
    }
}
