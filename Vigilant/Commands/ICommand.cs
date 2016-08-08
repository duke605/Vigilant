using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;

namespace Vigilant.Commands {
    interface ICommand
    {
        string CommandName { get; }
        Task<object> ParseArguments(MessageEventArgs e);
        Task Handle(object a, MessageEventArgs e);
    }
}
