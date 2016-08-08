using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Vigilant.Extensions;
using Vigilant.Utils;

namespace Vigilant.Commands {
    class Kick : ICommand
    {
        public string CommandName => "kick";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            var args = new Arguments();

            try
            {
                string userString = e.Message.RawText.Trim().Substring("!kick".Length + 1);
                args.User = e.Channel.GetUser(userString.RReplace("[<>@!]", "").ToUlong());

                // Making sure the user passed is valid
                if (!await GuardHelper.GuardParsing(e, args.User))
                    return null;

                return args;
            }
            catch (FormatException)
            {
                await e.Channel.SendMessage("`User argument must be mention. Eg. @Vigilant`");
            }
            catch (ArgumentOutOfRangeException)
            {
                await e.Channel.SendMessage("`A user must be supplied. !kick @Vigilant`");
            } 
            catch (Exception) {
                await e.Channel.SendMessage("`An unknown error occured.`");
            }

            return null;
        }

        public async Task Handle(object a, MessageEventArgs e)
        {
            Arguments arguments = (Arguments) a;

            using (VigilantDbEntities db = new VigilantDbEntities())
            {
                int reportsNeeds = db.Configurations.Find(e.Server.Id.ToString()).KickNum;
                User reporter = e.Message.User;
                User reported = arguments.User;

                string serverId = e.Server.Id.ToString();
                string reporterId = reporter.Id.ToString();
                string reportedId = reported.Id.ToString();

                // Checking if this command can be used
                if (!db.Configurations.Find(e.Server.Id.ToString()).AllowKick)
                {
                    await e.Channel.SendMessage("`Kick is disabled on this server.`");
                }

                // Checking if user can use command
                if (!await GuardHelper.GuardHandle(e, db))
                    return;

                // Checking if user is exempt
                var reportedRoles = reported.Roles.Select(r => r.Name);
                if (await db.ExemptRoles.AnyAsync(er => reportedRoles.Contains(er.Role))) {
                    await e.Channel.SendMessage($"`{reported.GetAnyName()} is exempt from being reported.`");
                    return;
                }

                // Adding strike to the user if reporting user has not submited one previously
                if (await db.Strikes.FirstOrDefaultAsync(s =>
                    s.ServerId == serverId &&
                    s.ChannelId == "-1" &&
                    s.ReporterId == reporterId &&
                    s.ReportedId == reportedId &&
                    s.Type == (byte) StrikeType.Kick) != null)
                {
                    await e.Channel.SendMessage("`You have already reported that user in the past 30 minutes.`");
                    return;
                }

                Strike strike = new Strike
                {
                    ServerId =  serverId,
                    ChannelId = "-1",
                    ReportedId = reportedId,
                    ReporterId = reporterId,
                    Type = (byte) StrikeType.Kick,
                    Time = DateTime.Now.AddMinutes(30)
                };

                // Adding the strike to the DB
                db.Strikes.Add(strike);

                // Saving the strike to the DB
                if (await Error.TryAsync(db.SaveChangesAsync, -1) <= 0)
                {
                    await e.Channel.SendMessage("`There was a problem saving the strike to the DB`");
                    return;
                }

                // Getting number of kick reports agaisnt the user
                var reports = db.Strikes.Where(s =>
                    s.ServerId == serverId &&
                    s.ChannelId == "-1" &&
                    s.ReporterId == reporterId &&
                    s.ReportedId == reportedId &&
                    s.Type == (byte) StrikeType.Kick);

                int reportCount = await reports.CountAsync();

                // Kicking user
                if (reportCount >= reportsNeeds)
                {
                    await e.Channel.SendMessage($"`{reported.GetAnyName()} has reached the maximum number of kick reports. Kicking user...`");
                    await reported.Kick();

                    db.Strikes.RemoveRange(reports);
                    if (await Error.TryAsync(db.SaveChangesAsync, -1) <= 0)
                        await e.Channel.SendMessage("`There was a problem deleting the reports.`");
                }
                else
                {
                    await e.Channel.SendMessage($"`{reported.GetAnyName()} has been reported and now has {reportCount} kick report(s). {reportsNeeds} needed to kick.`");
                }
            }
        }

        public class Arguments {
            public User User { get; set; }
        }
    }
}
