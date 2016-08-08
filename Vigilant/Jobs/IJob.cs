using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigilant.Jobs {
    interface IJob {
        Task Handle(VigilantDbEntities db);
    }
}
