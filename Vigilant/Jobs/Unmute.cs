using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Vigilant.Extensions;
using Vigilant.Utils;

namespace Vigilant.Jobs {
    class Unmute : IJob 
    { 
        public async Task Handle(VigilantDbEntities db)
        {
            IQueryable<Mute> mutes = db.Mutes.Where(s => s.Time <= DateTime.Now);

            // Checking if there are any old
            if (!await mutes.AnyAsync())
                return;

            // notifying users
            foreach (var mute in mutes) {
                Server server = Program.Client.GetServer(mute.ServerId.ToUlong());
                Channel channel = server?.GetChannel(mute.ChannelId.ToUlong());
                User user = server?.GetUser(mute.UserId.ToUlong());

                // Checking if user is found
                if (server == null || user == null || channel == null)
                    continue;
                
                await channel.RemovePermissionsRule(user);
                await user.SendMessage($"`You have been unmuted from #{channel.Name} on {server.Name}`");
                Console.WriteLine("Removing mute");
            }

            // Removing
            db.Mutes.RemoveRange(mutes);
            await Error.TryAsync(db.SaveChangesAsync, -1, ex => {
                Program.Client.Log.Error(nameof(db), "Error occured when deleting expired mutes", ex);
            });
        }
    }
}
