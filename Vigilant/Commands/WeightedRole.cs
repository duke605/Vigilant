using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;
using Fclp.Internals.Extensions;
using Vigilant.Utils;

namespace Vigilant.Commands {
    class WeightedRole : ICommand
    {
        public string CommandName => "weighted_role";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            Arguments a = new Arguments();
            FluentCommandLineParser parser = new FluentCommandLineParser();

            parser.Setup<List<string>>('r', "roles")
                .Callback(r => a.Roles = r)
                .Required()
                .WithDescription("The roles to assign the given weight to.");

            parser.Setup<int>('w', "weight")
                .Callback(w => a.Weight = w)
                .WithDescription("The weight to assign to the given roles.");

            parser.Setup<bool>('d', "remove")
                .Callback(r => a.Remove = r)
                .SetDefault(false)
                .WithDescription("Wheter or not to remove the given roles.");

            var result = parser.Parse(e.Message.Text.Split(' '));

            // Showing help
            if (result.HelpCalled) {
                await e.Channel.SendMessage($"```{HelpFormatter.GetHelpForCommand(parser)}```");
                return null;
            }

            // Errors
            if (result.HasErrors) {
                await e.Channel.SendMessage($"```{result.ErrorText}```");
                return null;
            }

            // Checking if remove or weight was users
            if (!a.Remove && a.Weight == null)
            {
                await e.Channel.SendMessage("`The remove or weight options must be used.`");
                return null;
            }

            // Checking if server has roles
            foreach (var role in a.Roles) {
                if (!e.Server.FindRoles(role, true).Any()) {
                    await e.Channel.SendMessage($"`The role \"{role}\" could not be found. Be sure to use proper casing.`");
                    return null;
                }
            }

            return a;
        }

        public async Task Handle(object args, MessageEventArgs e)
        {
            Arguments a = (Arguments) args;

            // Checking if server owner
            if (e.User.Id != e.Server.Owner.Id)
            {
                await e.Channel.SendMessage("`Only the server owner may use this command`");
                return;
            }

            using (VigilantDbEntities db = new VigilantDbEntities())
            {
                string serverId = e.Server.Id.ToString();

                // Removing
                if (a.Remove)
                {
                    var wr = db.WeightedRoles.Where(w => w.ServerId == serverId && a.Roles.Contains(w.Role));
                    db.WeightedRoles.RemoveRange(wr);
                    if (await Error.TryAsync(db.SaveChangesAsync, -1) < 0)
                        await e.Channel.SendMessage("`There was an error removing weighted roles from the DB.`");
                    else
                        await e.Channel.SendMessage("`Removed weighted roles from the DB.`");
                }

                // Adding
                else
                {
                    // Getting existing weights
                    var weights = await db.WeightedRoles.Where(w => w.ServerId == serverId && a.Roles.Contains(w.Role)).ToListAsync();
                    weights.ForEach(w => w.Num = a.Weight ?? 1);
                    
                    // Adding new weights
                    a.Roles
                        .Except(weights.Select(w => w.Role))
                        .ForEach(r => db.WeightedRoles.Add(new Vigilant.WeightedRole {
                            ServerId = serverId,
                            Role = r,
                            Num = a.Weight ?? 1
                        }));

                    if (await Error.TryAsync(db.SaveChangesAsync, -1) <= 0)
                        await e.Channel.SendMessage("`An error occured when adding/updating weighted roles to the DB.`");
                    else
                        await e.Channel.SendMessage("`Added/Updated weighted roles to the DB.`");
                }
            }
        }

        public class Arguments
        {
            public List<string> Roles { get; set; }
            public int? Weight { get; set; }
            public bool Remove { get; set; }
        }

        public static async Task<int> GetWeight(VigilantDbEntities db, MessageEventArgs e)
        {
            string serverId = e.Server.Id.ToString();
            IEnumerable<string> roles = e.User.Roles.Select(r => r.Name);

            // Looking for for highest weight
            var weight = await db.WeightedRoles
                .OrderByDescending(w => w.Num)
                .FirstOrDefaultAsync(w =>
                w.ServerId == serverId &&
                roles.Contains(w.Role));

            return weight?.Num ?? 1;
        }
    }
}
