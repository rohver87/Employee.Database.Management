using Employee.Database.Management.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Database.Management.Worker.EventHandler
{
    public class EmailEvent:EventArgs
    {
        public PublicHoliday Holiday {  get; set; }
    }
}
