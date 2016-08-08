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
    class Mute : ICommand
    {
        public string CommandName => "mute";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            var args = new Arguments();

            try {
                string userString = e.Message.RawText.Trim().Substring("!mute".Length + 1);
                User user = e.Channel.GetUser(userString.RReplace("[<>@!]", "").ToUlong());

                // Checking if the user wasn't found
                if (user == null) {
                    await e.Channel.SendMessage("`User could not be found.`");
                    return null;
                }

                // Checking if user is owner
                if (e.Server.Owner.Id == user.Id) {
                    await e.Channel.SendMessage("`You cannot report the server owner.`");
                    return null;
                }

                // Checking if user is me
                if (user.Id == Program.Client.CurrentUser.Id) {
                    await e.Channel.SendMessage("`You cannot report me!`");
                    return null;
                }

                // Checking if user is a bot
                if (user.IsBot) {
                    await e.Channel.SendMessage("`You cannot report a bot.`");
                    return null;
                }

                // Checking if user is self
                if (user.Id == e.User.Id) {
                    await e.Channel.SendMessage("`You cannot report yourself.`");
                    return null;
                }

                // Setting the reported
                args.User = user;

                return args;
            } catch (FormatException) {
                await e.Channel.SendMessage("`User argument must be mention. Eg. @Vigilant`");
            } catch (ArgumentOutOfRangeException) {
                await e.Channel.SendMessage("`A user must be supplied. !mute @Vigilant`");
            } catch (Exception) {
                await e.Channel.SendMessage("`An unknown error occured.`");
            }


            return null;
        }

        public async Task Handle(object a, MessageEventArgs e)
        {
            Arguments arguments = (Arguments)a;

            using (VigilantDbEntities db = new VigilantDbEntities()) {
                int reportsNeeds = (await db.Configurations.FindAsync(e.Server.Id.ToString())).MuteNum;
                User reporter = e.Message.User;
                User reported = arguments.User;

                string serverId = e.Server.Id.ToString();
                string channelId = e.Channel.Id.ToString();
                string reporterId = reporter.Id.ToString();
                string reportedId = reported.Id.ToString();

                // Checking if this command can be used
                if (!db.Configurations.Find(e.Server.Id.ToString()).AllowMute) {
                    await e.Channel.SendMessage("`Mute is disabled on this server.`");
                }
                
                // Checking if user is exempt
                var reportedRoles = reported.Roles.Select(r => r.Name);
                if (await db.ExemptRoles.AnyAsync(er => reportedRoles.Contains(er.Role)))
                {
                    await e.Channel.SendMessage($"`{reported.GetAnyName()} is exempt from being reported.`");
                    return;
                }

                // Checking if the player is muted
                if (e.Channel.GetPermissionsRule(reported).SendMessages == PermValue.Deny)
                {
                    await e.Channel.SendMessage($"`{reported.GetAnyName()} is already muted`");
                    return;
                }

                // Adding strike to the user if reporting user has not submited one previously
                if (await db.Strikes.FirstOrDefaultAsync(s =>
                    s.ServerId == serverId &&
                    s.ChannelId == channelId &&
                    s.ReporterId == reporterId &&
                    s.ReportedId == reportedId &&
                    s.Type == (byte)StrikeType.Mute) != null) {
                    await e.Channel.SendMessage("`You have already reported that user in the past 30 minutes.`");
                    return;
                }

                Strike strike = new Strike {
                    ServerId = serverId,
                    ChannelId = channelId,
                    ReportedId = reportedId,
                    ReporterId = reporterId,
                    Type = (byte)StrikeType.Mute,
                    Time = DateTime.Now.AddMinutes(30)
                };

                // Adding the strike to the DB
                db.Strikes.Add(strike);

                // Saving the strike to the DB
                if (await Error.TryAsync(db.SaveChangesAsync, -1) <= 0) {
                    await e.Channel.SendMessage("`There was a problem saving the strike to the DB`");
                    return;
                }

                // Getting number of kick reports agaisnt the user
                var reports = db.Strikes.Where(s =>
                    s.ServerId == serverId &&
                    s.ChannelId == channelId &&
                    s.ReporterId == reporterId &&
                    s.ReportedId == reportedId &&
                    s.Type == (byte)StrikeType.Mute);

                int reportCount = await reports.CountAsync();

                // Kicking user
                if (reportCount >= reportsNeeds) {
                    await e.Channel.SendMessage($"`{reported.GetAnyName()} has reached the maximum number of mute reports. Muting user...`");
                    await e.Channel.AddPermissionsRule(reported, new ChannelPermissionOverrides(sendMessages: PermValue.Deny));

                    // Adding timed unmute to DB
                    db.Mutes.Add(new Vigilant.Mute
                    {
                        ServerId = serverId,
                        ChannelId = channelId,
                        UserId = reportedId,
                        Time = DateTime.Now.AddMinutes((await db.Configurations.FindAsync(serverId)).MuteTime)
                    });

                    // Removing all strikes
                    db.Strikes.RemoveRange(reports);
                    if (await Error.TryAsync(db.SaveChangesAsync, -1) <= 0)
                        await e.Channel.SendMessage("`There was a problem deleting the reports.`");
                } else {
                    await e.Channel.SendMessage($"`{reported.GetAnyName()} has been reported and now has {reportCount} mute report(s). {reportsNeeds} needed to mute.`");
                }
            }
        }

        public struct Arguments
        {
            public User User { get; set; }
        }
    }
}
