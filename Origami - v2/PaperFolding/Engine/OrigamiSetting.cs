using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;

namespace Origami
{
    public class OrigamiSetting
    {
        public static byte _bkGray = 255;
        
        public static byte _lineGray = 0;
        public static int _lineWidth = 3;
        //public static int _margin = 32;

        public static Color _bkClr = Color.White;
        public static Color _lineClr = Color.Black;
        public static Color _cutClr = Color.PaleVioletRed;
        public static Color _cutFillClr = Color.Blue;
        public static Color _paperFrontClr = Color.FromArgb(208, 208, 208);
        public static Color _paperBkClr = Color.FromArgb(160, 160, 160);
    }
}
