using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Timers;
using Discord;
using Fclp.Internals.Extensions;
using Vigilant.Commands;
using Vigilant.Extensions;
using Vigilant.Jobs;
using Vigilant.Utils;

namespace Vigilant {
    class Program {

        public static readonly DiscordClient Client = new DiscordClient();
        public static readonly Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>();
        public static readonly List<IJob> Jobs = new List<IJob>();

        public static void Main(string[] args)
        {
            SetupTasks();
            Setup();
            Start();
        }

        private static void SetupTasks()
        {
            VigilantDbEntities db = new VigilantDbEntities();

            // Looking for jobs
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(T => typeof(IJob).IsAssignableFrom(T) && typeof(IJob) != T);

            // Registering jobs
            foreach (Type t in types) {
                IJob job = (IJob) t.GetConstructor(Type.EmptyTypes).Invoke(null);
                Jobs.Add(job);
            }
            
            // Starting timer
            new Timer(30000) { Enabled = true }.Elapsed += async (c, e) =>
            {

                // Handling jobs
                foreach (var job in Jobs) await job.Handle(db);
            };
        }

        private static void Setup()
        {
            // Looking for commands
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(T => typeof(ICommand).IsAssignableFrom(T) && typeof(ICommand) != T);

            // Registering commands
            foreach (Type t in types) {
                ICommand command = (ICommand) t.GetConstructor(Type.EmptyTypes).Invoke(null);
                Commands[command.CommandName] = command;
            }

            /*
             * DB setup
             */
            Client.ServerAvailable += async (s, e) =>
            {
                using (VigilantDbEntities db = new VigilantDbEntities())
                {
                    Configuration config = await db.Configurations.FindAsync(e.Server.Id.ToString());

                    // Checking if configurations exist for this server
                    if (config != null)
                        return;

                    Console.WriteLine($"Joined server: {e.Server.Name}");

                    // Creating new config
                    config = new Configuration
                    {
                        ServerId = e.Server.Id.ToString(),
                        AllowMute = true,
                        AllowBan = true,
                        AllowKick = true,
                        AllowPBan = false,
                        BanNum = 25,
                        PBanNum = 5,
                        BanTime = 30,
                        Global = false,
                        KickNum = 15,
                        MuteNum = 10,
                        MuteTime = 30,
                        BlockNum = 5
                    };

                    db.Configurations.Add(config);
                    await db.SaveChangesAsync();
                }
            };

            /*
             * Commands
             */
            Client.MessageReceived += async (s, e) =>
            {
                string commandName = e.Message.RawText.Split(' ')[0].Replace("!", "");

                // Checking if the message is a command
                if (!Commands.ContainsKey(commandName)) 
                {
                    return;
                }

                // Parsing command
                object arguments = await Commands[commandName].ParseArguments(e);

                // Checking if the parse was good
                if (arguments == null)
                    return;
                
                // Handling the command
                await Commands[commandName].Handle(arguments, e);
            };
        }

        private static void Start()
        {
            Client.ExecuteAndWait(async () =>
            {
                Console.Write("Connecting... ");
                await Client.Connect(Secret.DiscordToken);
                Console.WriteLine("Done.");
            });
        }
    }

    // Adding this here for now because it keeps getting overwritten
    public enum StrikeType : byte
    {
        Kick = 1,
        Mute = 2,
        Ban = 3,
        Permanent = 4
    }
}
