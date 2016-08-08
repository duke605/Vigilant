using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;
using Vigilant.Utils;

namespace Vigilant.Commands {
    class Exempt : ICommand
    {
        public string CommandName => "exempt";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            Arguments a = new Arguments();
            FluentCommandLineParser parser = new FluentCommandLineParser();

            try
            {
                string[] args = e.Message.RawText.Split(' ');

                parser.Setup<List<string>>('r', "roles")
                    .Callback(r => a.Roles = r)
                    .Required()
                    .WithDescription("The roles to add to the exempt table.");

                parser.Setup<bool>('d', "remove")
                    .Callback(b => a.Remove = b)
                    .SetDefault(false)
                    .WithDescription("Wheather or not to remove the supplied roles from the exempt table.");

                parser.SetupHelp("?", "help");

                var result = parser.Parse(args);
                
                // Showing help
                if (result.HelpCalled)
                {
                    await e.Channel.SendMessage($"```{HelpFormatter.GetHelpForCommand(parser)}```");
                    return null;
                }

                // Errors
                if (result.HasErrors)
                {
                    await e.Channel.SendMessage($"```{result.ErrorText}```");
                    return null;
                }

                // Checking if server has roles
                foreach (var role in a.Roles)
                {
                    if (!e.Server.FindRoles(role, true).Any())
                    {
                        await e.Channel.SendMessage($"`The role \"{role}\" could not be found. Be sure to use proper casing.`");
                        return null;
                    }
                }

                // Checking if the user is the owner
                if (e.User.Id != e.Server.Owner.Id)
                {
                    await e.Channel.SendMessage("`Only the server owner may use this command.`");
                    return null;
                }

                return a;
            }
            catch (Exception)
            {
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
                string serverId = e.Server.Id.ToString();

                // Removing
                if (a.Remove)
                {
                    var roles = db.ExemptRoles.Where(r => r.ServerId == serverId && a.Roles.Contains(r.Role));
                    db.ExemptRoles.RemoveRange(roles);

                    // Saving
                    if (await Error.TryAsync(db.SaveChangesAsync, -1) < 0)
                        await e.Channel.SendMessage("`There was an error removing the exempt roles from the DB.`");
                    else
                        await e.Channel.SendMessage("`Exempt roles successfully removed.`");

                    return;
                }

                // Adding
                foreach (var role in a.Roles)
                {
                    // Skipping roles that are exempt
                    if (await db.ExemptRoles.CountAsync(r => r.ServerId == serverId && r.Role == role) > 0)
                        continue;

                    // Adding roles
                    ExemptRole er = new ExemptRole
                    {
                        ServerId = serverId,
                        Role = role
                    };

                    db.ExemptRoles.Add(er);
                }

                // Saving
                if (await Error.TryAsync(db.SaveChangesAsync, -1) < 0)
                    await e.Channel.SendMessage("`There was an error saving the exempt roles to the DB.`");
                else
                    await e.Channel.SendMessage("`Exempt roles successfully added.`");
            }
        }

        public struct Arguments
        {
            public List<string> Roles { get; set; }
            public bool Remove { get; set; }
        }
    }
}
