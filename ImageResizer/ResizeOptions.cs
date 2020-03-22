using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ResizeOptions
    {
        public int SourceWidth { get; set; }
        public int SourceHeight { get; set; }
        public int DestinationWidth { get; set; }
        public int DestionatonHeight { get; set; }
        public string DestionatonPath { get; set; }
    }
}
