using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;
using Vigilant.Extensions;
using Vigilant.Utils;

namespace Vigilant.Commands {
    class Inspect : ICommand
    {
        public string CommandName => "inspect";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            Arguments a = new Arguments();
            FluentCommandLineParser parser = new FluentCommandLineParser();

            parser.Setup<Arguments.Table>('t', "table")
                .Callback(t => a.TableName = t)
                .Required()
                .WithDescription("The table you want to inspect. (Pluralize it, Eg. Configurations)");

            parser.Setup<bool>('r', "resolve_ids")
                .Callback(r => a.ResolveIds = r)
                .SetDefault(false)
                .WithDescription("Wheather or not to resolve user ids into names.");

            parser.SetupHelp("?", "help");

            var results = parser.Parse(e.Message.Text.Split(' '));

            // Showing help
            if (results.HelpCalled)
            {
                await e.Channel.SendMessage($"```{HelpFormatter.GetHelpForCommand(parser)}```");
                return null;
            }

            // Showing errors
            if (results.HasErrors)
            {
                await e.Channel.SendMessage($"```{results.ErrorText}```");
                return null;
            }

            return a;
        }

        public async Task Handle(object a1, MessageEventArgs e)
        {
            Arguments a = (Arguments) a1;

            // Checking if user is server owner
            if (e.User.Id != e.Server.Owner.Id)
            {
                await e.Channel.SendMessage("`Only the server owner may use this command.`");
            }

            using (VigilantDbEntities db = new VigilantDbEntities())
            {
                switch (a.TableName)
                {
                    case Arguments.Table.Configurations:
                        await showConfigurations(a, e, db);
                        return;
                    case Arguments.Table.ExemptRoles:
                        await showExemptRoles(a, e, db);
                        return;
                    case Arguments.Table.Mutes:
                        await showMutes(a, e, db);
                        return;
                    case Arguments.Table.Ignores:
                        await showIgnores(a, e, db);
                        return;
                    case Arguments.Table.Strikes:
                        await showStrikes(a, e, db);
                        return;
                }
            }
        }

        private async Task showStrikes(Arguments a, MessageEventArgs e, VigilantDbEntities db)
        {
            string serverId = e.Server.Id.ToString();
            var strikes = (await db.Strikes
                .Where(s => s.ServerId == serverId)
                .ToListAsync())
                .Select(s => new
                {
                    Reporter = a.ResolveIds
                    ? s.ReporterId.ToUser(e.Server).GetAnyName()
                    : s.ReporterId,
                    Reported = a.ResolveIds
                    ? s.ReportedId.ToUser(e.Server).GetAnyName()
                    : s.ReportedId,
                    Type = (StrikeType) s.Type,
                    Expires = DateTime.Now.TimeBetween(s.Time)
                });

            if (strikes.Any())
                await e.Channel.SendMessage($"```{TableFormatter.MakeTable(strikes)}```");
            else
                await e.Channel.SendMessage("`No strikes to list.`");
        }

        private async Task showIgnores(Arguments a, MessageEventArgs e, VigilantDbEntities db)
        {
            string serverId = e.Server.Id.ToString();
            IEnumerable<object> ignores = await db.Ignores.Where(i => i.ServerId == serverId).ToListAsync();

            ignores = ignores.Select(i => new
            {
                User = a.ResolveIds
                    ? ((Vigilant.Ignore) i).UserId.ToUser(e.Server).GetAnyName()
                    : ((Vigilant.Ignore) i).UserId.ToString()
            });

            if (ignores.Any())
                await e.Channel.SendMessage($"```{TableFormatter.MakeTable(ignores)}```");
            else
                await e.Channel.SendMessage("`No ignores to list.`");
        }

        private async Task showMutes(Arguments a, MessageEventArgs e, VigilantDbEntities db)
        {
            string serverId = e.Server.Id.ToString();
            IEnumerable<object> mutes = await db.Mutes.Where(m => m.ServerId == serverId).ToListAsync();

            mutes = mutes.Select(m => new
            {
                User = a.ResolveIds 
                ? ((Vigilant.Mute)m).UserId.ToUser(e.Server).GetAnyName() 
                : ((Vigilant.Mute)m).UserId,
                Expires = DateTime.Now.TimeBetween(((Vigilant.Mute)m).Time)
            });

            if (mutes.Any())
                await e.Channel.SendMessage($"```{TableFormatter.MakeTable(mutes)}```");
            else
                await e.Channel.SendMessage("`No mutes to list.`");
        }

        private async Task showExemptRoles(Arguments a, MessageEventArgs e, VigilantDbEntities db)
        {
            string serverId = e.Server.Id.ToString();
            var exempt = await db.ExemptRoles.Where(er => er.ServerId == serverId).Select(er => new { er.Role }).ToListAsync();

            if (exempt.Any())
                await e.Channel.SendMessage($"```{TableFormatter.MakeTable(exempt, new[] {"Exempt Roles"})}```");
            else
                await e.Channel.SendMessage("`No exempt roles to list.`");
        }

        private async Task showConfigurations(Arguments a, MessageEventArgs e, VigilantDbEntities db)
        {
            var config = await db.Configurations.FindAsync(e.Server.Id.ToString());
            string message = $"Toggles:\r\n" +
                             $"Mute: {config.AllowMute.ToReadable("Enabled", "Disabled")}\r\n" +
                             $"Kick: {config.AllowKick.ToReadable("Enabled", "Disabled")}\r\n" +
                             $"Ban: {config.AllowBan.ToReadable("Enabled", "Disabled")}\r\n" +
                             $"\r\nNumbers:\r\n" +
                             $"Mute Number: {config.MuteNum}\r\n" +
                             $"Kick Number: {config.KickNum}\r\n" +
                             $"Ban Number: {config.BanNum}\r\n" +
                             $"\r\nTimes:\r\n" +
                             $"Mute Time: {config.MuteTime}\r\n" +
                             $"Ban Time: {config.BanTime}";

            await e.Channel.SendMessage($"```{message}```");
        } 

        public struct Arguments
        {
            public Table TableName { get; set; }
            public bool ResolveIds { get; set; }

            public enum Table
            {
                Ignores,
                Mutes,
                Configurations,
                Strikes,
                ExemptRoles
            }
        }
    }
}
