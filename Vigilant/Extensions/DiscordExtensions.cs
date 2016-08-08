using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Vigilant.Extensions {
    static class DiscordExtensions {

        public static string GetAnyName(this User user) => 
            user.Nickname ?? user.Name;
    }
}
