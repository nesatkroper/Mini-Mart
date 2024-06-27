using MiniMartSystem.DotNunServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniMartSystem.Lib
{
    internal class dbConnect
    {
        public static string conStr = @"Data Source=(localdb)\local;Initial Catalog=MiniMart;Integrated Security=True";
    }
}