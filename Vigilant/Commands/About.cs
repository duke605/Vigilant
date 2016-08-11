using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Vigilant.Commands {
    class About : ICommand
    {
        public string CommandName => "about";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            await Task.Delay(0);
            return "";
        }

        public async Task Handle(object a, MessageEventArgs e)
        {
            await e.Channel.SendMessage(
                $"__Author:__ Duke605\r\n" +
                $"__Library:__ Discord.Net\r\n" +
                $"__Version:__ 1.1.0\r\n" +
                $"__Github:__ <https://github.com/duke605/Vigilant>\r\n" +
                $"__Report Issue:__ <https://github.com/duke605/Vigilant/issues>\r\n"
            );
        }
    }
}
