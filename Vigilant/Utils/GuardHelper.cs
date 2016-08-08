using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Vigilant.Utils {
    class GuardHelper {

        public static async Task<bool> GuardParsing(MessageEventArgs e, User user, string verb = "report")
        {
            // Checking if the user wasn't found
            if (user == null)
            {
                await e.Channel.SendMessage("`User could not be found.`");
                return false;
            }

            // Checking if user is owner
            if (e.Server.Owner.Id == user.Id) {
                await e.Channel.SendMessage($"`You cannot {verb} the server owner.`");
                return false;
            }

            // Checking if user is me
            if (user.Id == Program.Client.CurrentUser.Id)
            {
                await e.Channel.SendMessage($"`You cannot {verb} me!`");
                return false;
            }

            // Checking if user is a bot
            if (user.IsBot)
            {
                await e.Channel.SendMessage($"`You cannot {verb} a bot.`");
                return false;
            }

            // Checking if user is self
            if (user.Id == e.User.Id)
            {
                await e.Channel.SendMessage($"`You cannot {verb} yourself.`");
                return false;
            }

            return true;
        }
    }
}
