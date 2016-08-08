using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vigilant.Utils;

namespace Vigilant.Jobs {
    class RemoveStrikes : IJob 
    {
        public async Task Handle(VigilantDbEntities db)
        {
            // Getting results to remove
            IQueryable<Strike> results = db.Strikes.Where(s => s.Time <= DateTime.Now);

            // Checking if there are any old
            if (!await results.AnyAsync())
                return;

            // Removing
            db.Strikes.RemoveRange(results);
            await Error.TryAsync(db.SaveChangesAsync, -1, ex =>
            {
                Program.Client.Log.Error(nameof(db), "Error occured when deleting old strikes", ex);
            });
        }
    }
}
