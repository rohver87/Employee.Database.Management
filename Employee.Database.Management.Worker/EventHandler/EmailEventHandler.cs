using Employee.Database.Management.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Database.Management.Worker.EventHandler
{
    /// <summary>
    /// Email handling logic will go here
    /// </summary>
    public class EmailEventHandler
    {
        public void OnEventTriggered(object sender, EmailEvent e)
        {
            Console.WriteLine("Email handler code execution..");
        }
    }
}
