using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class WaterTxt
    {
        public string Text { get; set; }
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public string PageRange { get; set; }
        public double HorizontalDistance { get; set; }
        public double VerticalDistance { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
        public double Opacity { get; set; }
    }
}