using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Vigilant.Extensions;
using Vigilant.Utils;

namespace Vigilant.Commands {
    class Ignore : ICommand
    {
        public string CommandName => "ignore";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            var args = new Arguments();

            try {
                string userString = e.Message.RawText.Trim().Substring("!ignore".Length + 1);
                args.User = e.Channel.GetUser(userString.RReplace("[<>@!]", "").ToUlong());

                // Making sure the user passed is valid
                if (!await GuardHelper.GuardParsing(e, args.User, "ignore"))
                    return null;

                return args;
            } catch (FormatException) {
                await e.Channel.SendMessage("`User argument must be mention. Eg. @Vigilant`");
            } catch (ArgumentOutOfRangeException) {
                await e.Channel.SendMessage("`A user must be supplied. !ignore @Vigilant`");
            } catch (Exception) {
                await e.Channel.SendMessage("`An unknown error occured.`");
            }
            
            return null;
        }

        public async Task Handle(object args, MessageEventArgs e)
        {
            Arguments a = (Arguments) args;

            // Checking for server owner
            if (e.User.Id != e.Server.Owner.Id) {
                await e.Channel.SendMessage("`Only the server owner may use this command.`");
                return;
            }

            using (VigilantDbEntities db = new VigilantDbEntities())
            {
                string userId = a.User.Id.ToString();
                string serverId = e.Server.Id.ToString();

                // Checking if person is already muted
                if (await db.Ignores.AnyAsync(i => i.ServerId == serverId && i.UserId == userId))
                {
                    await e.Channel.SendMessage("`That user is already ignored.`");
                }

                // Saving ignore to DB
                Vigilant.Ignore ignore = new Vigilant.Ignore
                {
                    ServerId = serverId,
                    UserId = userId
                };
                db.Ignores.Add(ignore);
                if (await Error.TryAsync(db.SaveChangesAsync, -1) <= 0)
                    await e.Channel.SendMessage("`There was an error saving the ignore record to the DB`");
                else
                    await e.Channel.SendMessage($"`{a.User.GetAnyName()} has been ignored.`");
            }
        }

        public struct Arguments
        {
            public User User { get; set; }
        }
    }
}
