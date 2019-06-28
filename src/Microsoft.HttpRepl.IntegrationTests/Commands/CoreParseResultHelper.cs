using System;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    internal static class CoreParseResultHelper
    {
        public static ICoreParseResult Create(string commandText, int caretPosition = 0)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            CoreParser coreParser = new CoreParser();
            ICoreParseResult parseResult = coreParser.Parse(commandText, caretPosition);

            return parseResult;
        }
    }
}
