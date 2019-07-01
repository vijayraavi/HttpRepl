using Microsoft.HttpRepl.Commands;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class RunCommandTests : ICommandTestHelper<RunCommand>
    {
        public RunCommandTests(): base(new RunCommand())
        {
        }
    }
}
