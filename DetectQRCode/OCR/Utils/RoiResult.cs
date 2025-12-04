using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectQRCode.OCR.Utils
{
    public class RoiResult
    {
        public Bitmap? Image { get; set; }
        public Rectangle Mapped { get; set; }  // vùng c?t trong ?nh g?c
        public float Scale { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
    }
}

