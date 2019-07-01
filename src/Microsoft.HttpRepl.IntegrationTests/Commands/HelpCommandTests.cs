using Microsoft.HttpRepl.Commands;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class HelpCommandTests : ICommandTestHelper<HelpCommand>
    {
        public HelpCommandTests(): base(new HelpCommand())
        {
        }
    }
}
