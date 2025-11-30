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
    [Serializable]
    public class OrigamiCutObjs
    {
        public List<RectangleF> _squareList = new List<RectangleF>();//1:1
        public List<RectangleF> _circleList = new List<RectangleF>();//1:1
        public List<RectangleF> _rtList = new List<RectangleF>();//3:4
        public List<RectangleF> _hexagonList = new List<RectangleF>();//六边形：外接矩形。

        public List<Bitmap> _candidateList = new List<Bitmap>();

        public void Clear()
        {
            _rtList.Clear();
            _circleList.Clear();
            _squareList.Clear();
            _hexagonList.Clear();
        }

        public OrigamiCutObjs Clone()
        {
            return DeepCopyHelper.DeepCopy<OrigamiCutObjs>(this);
        }

        public bool IsNearObj(OrigamiCutObjs obj, double distTh = 40)
        {
            if (_rtList.Count == obj._rtList.Count &&
                _squareList.Count == obj._squareList.Count &&
                _circleList.Count == obj._circleList.Count &&
                _hexagonList.Count == obj._hexagonList.Count)
            {
                double maxDist = 0;
                if (_rtList.Count > 0)
                {
                    maxDist = Math.Max(maxDist, Dist(_rtList[0].Location, obj._rtList[0].Location));
                }
                if (_squareList.Count > 0)
                {
                    maxDist = Math.Max(maxDist, Dist(_squareList[0].Location, obj._squareList[0].Location));
                }
                if (_circleList.Count > 0)
                {
                    maxDist = Math.Max(maxDist, Dist(_circleList[0].Location, obj._circleList[0].Location));
                }
                if (_hexagonList.Count > 0)
                {
                    maxDist = Math.Max(maxDist, Dist(_hexagonList[0].Location, obj._hexagonList[0].Location));
                }
                return maxDist < distTh;
            }


            return false;
        }

        public double Dist(PointF p1, PointF p2)
        {
            float x = p1.X - p2.X;
            float y = p1.Y - p2.Y;
            return Math.Sqrt(x * x + y * y);
        }

        
    }
}
