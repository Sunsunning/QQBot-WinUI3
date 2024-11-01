using System.Collections.Generic;

namespace QQBotCodePlugin.QQBot.abilities.utils
{
    public class HelpCommandHelper
    {
        public Dictionary<string, List<string>> commands { get; }

        public HelpCommandHelper()
        {
            commands = new Dictionary<string, List<string>>();
        }

        public void addCommands(string header, List<string> _commands)
        {
            commands[header] = _commands;
        }
        public List<string> getCommands(string header) {
            if (!commands.ContainsKey(header)) {
                addCommands(header, new List<string>());
                return new List<string>();
            }
            if (commands[header] == null) {
                return new List<string>();                
            }
            return commands[header];
        }
    }
}
