using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class EsimSign
    {
        public string reason { get; set; }
        public string place { get; set; }
        public bool show_signby { get; set; }
    }
}