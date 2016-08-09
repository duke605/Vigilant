using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp.Internals.Extensions;
using Newtonsoft.Json;
using Vigilant.Extensions;
using Vigilant.Utils;

namespace Vigilant.Commands {
    class Forgive : ICommand
    {
        public string CommandName => "forgive";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            string[] message = e.Message.Text.Split(' ');
            Arguments args = new Arguments();

            try {
                string userString = e.Message.RawText.Trim().Substring("!forgive".Length + 1);
                args.User = e.Channel.GetUser(userString.RReplace("[<>@!]", "").ToUlong());

                // Making sure the user passed is valid
                if (!await GuardHelper.GuardParsing(e, args.User, "forgive"))
                    return null;

                return args;
            } catch (FormatException) {
                await e.Channel.SendMessage("`User argument must be mention. Eg. @Vigilant`");
            } catch (ArgumentOutOfRangeException) {
                await e.Channel.SendMessage("`A user must be supplied. !forgive @Vigilant`");
            } catch (Exception) {
                await e.Channel.SendMessage("`An unknown error occured.`");
            }

            return null;
        }

        public async Task Handle(object args, MessageEventArgs e)
        {
            Arguments a = (Arguments) args;

            // Checking if server owner
            if (e.User.Id != e.Server.Owner.Id)
            {
                await e.Channel.SendMessage("`Only the server owner may use this command.`");
                return;
            }

            using (VigilantDbEntities db = new VigilantDbEntities())
            {
                string serverId = e.Server.Id.ToString();
                string userId = a.User.Id.ToString();

                // Getting the users permanent strikes
                var strikes = db.Strikes.Where(s => 
                    s.ServerId == serverId && 
                    s.Type == (byte) StrikeType.Permanent && 
                    s.ReportedId == userId);

                // Checking if user has any permanent strikes
                if (await strikes.AnyAsync())
                {
                    // Removing strikes
                    db.Strikes.RemoveRange(strikes);
                    if (await Error.TryAsync(db.SaveChangesAsync, -1) <= 0)
                        await e.Channel.SendMessage("`There was an error removing the strikes from the DB.`");
                    else
                        await e.Channel.SendMessage($"`{a.User.GetAnyName()}'s permanent strikes have been forgiven.`");
                }
                else
                    await e.Channel.SendMessage($"`{a.User.GetAnyName()} has no permanent strikes.`");
            }
        }

        public class Arguments
        {
            public User User { get; set; }
        }
    }
}
