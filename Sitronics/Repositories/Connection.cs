using Sitronics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sitronics.Repositories
{
    public static class Connection
    {
        public static HttpClient Client { get; set; } = new HttpClient() 
        { 
            BaseAddress = new Uri("https://Dimaso.bsite.net/") 
        };

        public static User CurrentUser { get; set; } = null;
    }
}
