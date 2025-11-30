using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;

namespace Origami
{
    public class OrigamiPaper
    {
        public static List<PointF> CreatePaper(PaperType paperType)
        {
            switch (paperType)
            {
                case PaperType.Circle:
                    return ShapeGenerator.CreateCircle(180);
                case PaperType.Diamond:
                    return ShapeGenerator.CreateDiamond(240, 360);
                case PaperType.Hexagon:
                    return ShapeGenerator.CreateHexagon(180);
                case PaperType.House:
                    return ShapeGenerator.CreateHouse(360, 200, 80);
                case PaperType.IrregularHeptagon:
                    return ShapeGenerator.CreateIrregularHeptagon(180);
                case PaperType.Octagon:
                    return ShapeGenerator.CreateOctagon(180);
                case PaperType.Parallelogram:
                    return ShapeGenerator.CreateParallelogram(300, 240, 30);
                case PaperType.Pentagon:
                    return ShapeGenerator.CreatePentagon(180);
                case PaperType.Rectangle:
                    return ShapeGenerator.CreateRectangle(360, 270);
                case PaperType.Square:
                    return ShapeGenerator.CreateSquare(360);
                case PaperType.Star:
                    return ShapeGenerator.CreateStar(5, 180, 120);
                case PaperType.Trapezoid:
                    return ShapeGenerator.CreateTrapezoid(240, 360, 360);
                case PaperType.Triangle:
                    return ShapeGenerator.CreateTriangle(180);
            }
            return null;
        }
    }

    public enum PaperType
    {
        Square,
        Hexagon,
        Pentagon,
        Octagon,
        Triangle,
        Rectangle,
        House,
        Circle,
        Diamond,
        Parallelogram,
        Trapezoid,//梯形
        Star,
        IrregularHeptagon,
    }

    
}
