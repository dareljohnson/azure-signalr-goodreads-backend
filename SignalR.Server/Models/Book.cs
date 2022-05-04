using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Server.Models
{
    public class Book
    {
        public string Id { get; set; }
        public string ImagePath { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ComputedHash { get; set; }
        public string LastEvent { get; set; }
        public string Created { get; set; }
        public string LastUpdated { get; set; }
    }
}
