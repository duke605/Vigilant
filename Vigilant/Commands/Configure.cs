﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;
using Vigilant.Extensions;
using Vigilant.Utils;

namespace Vigilant.Commands {
    class Configure : ICommand
    {
        public string CommandName => "config";

        public async Task<object> ParseArguments(MessageEventArgs e)
        {
            Arguments args = new Arguments();
            FluentCommandLineParser parser = new FluentCommandLineParser();
            
            parser.Setup<int>('k', "kick_num")
                .Callback(n => args.KickNum = n)
                .WithDescription("The number of strikes needed to kick a user.");

            parser.Setup<int>('m', "mute_num")
                .Callback(n => args.MuteNum = n)
                .WithDescription("The number of strikes need to mute a user.");
            
            parser.Setup<int>('f', "pban_num")
                .Callback(b => args.PBanNum = b)
                .WithDescription("The number of permanent strikes a user can recieve before being permanently banned.");

            parser.Setup<int>('b', "block_num")
                .Callback(b => args.BlockNum = b)
                .WithDescription("The number of concurrent strikes a user can recieve before being ignored by the bot.");

            parser.Setup<int>('M', "mute_time")
                .Callback(n => args.MuteTime = n)
                .WithDescription("The number of minutes a user will be muted for.");

            parser.Setup<bool>('s', "allow_mute")
                .Callback(b => args.AllowMute = b)
                .WithDescription("Wheather or not users can report users to mute them.");

            parser.Setup<bool>('l', "allow_kick")
                .Callback(b => args.AllowKick = b)
                .WithDescription("Wheather or not users can report users to kick them.");

            parser.Setup<bool>('F', "allow_pban")
                .Callback(b => args.AllowPBan = b)
                .WithDescription("Wheather or not users can be permanently banned.");

            parser.SetupHelp("?", "help");

            var results = parser.Parse(e.Message.Text.Split(' '));

            // Checking if help was called
            if (results.HelpCalled)
                await e.Channel.SendMessage($"```{HelpFormatter.GetHelpForCommand(parser)}```");

            // Checking for errors
            else if (results.HasErrors)
                await e.Channel.SendMessage($"```{results.ErrorText}```");

            // All clear
            else
                return args;

            return null;
        }

        public async Task Handle(object arg, MessageEventArgs e)
        {
            Arguments a = (Arguments) arg;

            // Checking if the user is the owner
            if (e.User.Id != e.Server.Owner.Id)
            {
                await e.Channel.SendMessage("`Only the server owner may use this command.`");
                return;
            }

            using (VigilantDbEntities db = new VigilantDbEntities())
            {
                Configuration config = db.Configurations.Find(e.Server.Id.ToString());

                // Updating
                config.AllowKick = a.AllowKick ?? config.AllowKick;
                config.AllowMute = a.AllowMute ?? config.AllowMute;
                config.AllowPBan = a.AllowPBan ?? config.AllowPBan;

                config.KickNum = a.KickNum ?? config.KickNum;
                config.MuteNum = a.MuteNum ?? config.MuteNum;
                config.PBanNum = a.PBanNum ?? config.PBanNum;
                config.BlockNum = a.BlockNum ?? config.BlockNum;

                config.MuteTime = a.MuteTime ?? config.MuteTime;

                // Saving to the DB
                if (await Error.TryAsync(db.SaveChangesAsync, -1) < 0)
                    await e.Channel.SendMessage("`There was a problem saving the configuration to the DB.`");
                else
                    await e.Channel.SendMessage("`Server configuration updated.`");
            }
        }

        public class Arguments
        {
            public int? KickNum { get; set; }
            public int? MuteNum { get; set; }
            public int? BlockNum { get; set; }
            public int? MuteTime { get; set; }
            public int? PBanNum { get; set; }
            public bool? AllowMute { get; set; }
            public bool? AllowKick { get; set; }
            public bool? AllowPBan { get; set; }
        }
    }
}
