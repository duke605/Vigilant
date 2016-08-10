using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Vigilant.Extensions;

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

        public static async Task<bool> GuardHandle(MessageEventArgs e, VigilantDbEntities db, bool ownerOnly = false)
        {
            string serverId = e.Server.Id.ToString();
            string userId = e.User.Id.ToString();
            var config = await db.Configurations.FindAsync(serverId);

            // Checking for server owner
            if (ownerOnly && e.User.Id != e.Server.Owner.Id)
            {
                await e.Channel.SendMessage("`Only the server owner may use this command.`");
                return false;
            }

            // Checking ignore list
            if (await db.Ignores.AnyAsync(i => i.ServerId == serverId && i.UserId == userId))
            {
                await e.Channel.SendMessage("`You are on the ignore list and therefore may not use commands.`");
                return false;
            }
            
            // Checking if player has too many strikes
            if (await db.Strikes.Where(s =>
                s.ServerId == serverId &&
                s.ReportedId == userId)
                .CountAsync() > config.BlockNum)
            {
                await e.Channel.SendMessage("`You have too many strikes to use that command.`");
                return false;
            }

            return true;
        }

        public static async Task TryPBan(VigilantDbEntities db, MessageEventArgs e, string userId, string serverId)
        {
            Configuration config = await db.Configurations.FindAsync(serverId);
            
            // Checking if permanent bans are allowed
            if (!config.AllowPBan)
                return;

            // Checking how many strikes the user has
            int permanentCount = await db.Strikes.CountAsync(s =>
                s.ServerId == serverId &&
                s.ReportedId == userId &&
                s.Type == (byte)StrikeType.Permanent);

            // Checking if strikes exceed limit
            if (permanentCount < config.PBanNum)
                return;

            await e.Channel.SendMessage($"`{userId.ToUser(e.Server).GetAnyName()} has reached the maximum amount of permanent strikes. Permanently banning user...`");
            await e.Server.Ban(userId.ToUser(e.Server));
        }

        private struct PBanQuery
        {
            public int PBanNum { get; set; }
        }
    }
}
