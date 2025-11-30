using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Management;
using System.Management.Instrumentation;

namespace Origami
{
    public class GlobalUtils
    {
        private static object _lockObj = new object();

        public static bool IsSameByteArray(byte[] b1, byte[] b2)
        {
            if (b1 != null && b2 != null)
            {
                if (b1.Length == b2.Length)
                {
                    for (int i = 0; i < b1.Length; i++)
                    {
                        if (b1[i] != b2[i])
                            return false;
                    }
                    return true;
                }
                return false;
            }
            else
            {
                if (b1 == null && b2 == null)
                    return true;
            }
            return false;
        }

        static public string PointFToString(PointF pt, string format)
        {
            return "(X=" + pt.X.ToString(format) + ",Y=" + pt.Y.ToString("#0.0") + ")";
        }

        static public string PointFToString(PointF pt)
        {
            return pt.X.ToString() + "," + pt.Y.ToString();
        }

        static public string SizeFToString(SizeF sz)
        {
            return sz.Width.ToString() + "," + sz.Height.ToString();
        }

        static public MyRangeF RangeFParse(string str)
        {
            try
            {

                MyRangeF s = new MyRangeF();
                string[] val = str.Split(',');
                if (val.Length > 1)
                {
                    s._min = float.Parse(val[0]);
                    s._max = float.Parse(val[1]);
                }
                return s;
            }
            catch
            {
                return new MyRangeF();
            }
        }


        static public string RangeFToString(MyRangeF s)
        {
            return s._min.ToString() + "," + s._max.ToString();
        }

        static public string PointToString(Point pt)
        {
            return pt.X.ToString() + "," + pt.Y.ToString();
        }

        static public string PointFListToString(List<PointF> pointArray)
        {
            string strPtList = "";
            for (int i = 0; i < pointArray.Count; i++)
            {
                if (i != pointArray.Count - 1)
                    strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString() + ",";
                else
                    strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString();
            }
            return strPtList;
        }

        public static bool IsBytesSame(byte[] buf1, byte[] buf2, int len)
        {
            try
            {
                if (buf1 == null && buf2 == null)
                    return true;
                if (buf1 == null && buf2 != null)
                    return false;
                if (buf1 != null && buf2 == null)
                    return false;
                return BytesToString(buf1, len) == BytesToString(buf2,len);
            }
            catch
            { }
            return false;
        }

        public static string BytesToString(byte[] data, int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
                str += byteToStr(data[i]) + " ";
            return str;
        }

        public static string byteToStr(byte b)
        {
            int h = b / 16;
            int l = b % 16;

            string str = "";
            if (h < 10)
            {
                str += h.ToString();
            }
            else
            {
                str += (char)('A' + h - 10);
            }

            if (l < 10)
            {
                str += l.ToString();
            }
            else
            {
                str += (char)('A' + l - 10);
            }

            return str;
        }

        static public string PointListToString(List<Point> pointArray)
        {
            string strPtList = "";
            for (int i = 0; i < pointArray.Count; i++)
            {
                if (i != pointArray.Count - 1)
                    strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString() + ",";
                else
                    strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString();
            }
            return strPtList;
        }

        //static public string PointFListToString(List<PointF> pointArray)
        //{
        //    string strPtList = "";
        //    for (int i = 0; i < pointArray.Count; i++)
        //    {
        //        if (i != pointArray.Count - 1)
        //            strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString() + ",";
        //        else
        //            strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString();
        //    }
        //    return strPtList;
        //}


        static public string PointListToString(Point[] pointArray)
        {
            string strPtList = "";
            for (int i = 0; i < pointArray.Length; i++)
            {
                if (i != pointArray.Length - 1)
                    strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString() + ",";
                else
                    strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString();
            }
            return strPtList;
        }

        static public string PointFListToString(PointF[] pointArray)
        {
            string strPtList = "";
            for (int i = 0; i < pointArray.Length; i++)
            {
                if (i != pointArray.Length - 1)
                    strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString() + ",";
                else
                    strPtList += pointArray[i].X.ToString() + "," + pointArray[i].Y.ToString();
            }
            return strPtList;
        }

        static public PointF PointFParse(string str)
        {
            try
            {
                PointF pt = new PointF();
                string[] val = str.Split(',');
                if (val.Length > 1)
                {
                    pt.X = float.Parse(val[0]);
                    pt.Y = float.Parse(val[1]);
                }
                return pt;
            }
            catch
            {
                return new PointF();
            }
        }

        static public Point PointParse(string str)
        {
            try
            {
                Point pt = new Point();
                string[] val = str.Split(',');
                if (val.Length > 1)
                {
                    pt.X = int.Parse(val[0]);
                    pt.Y = int.Parse(val[1]);
                }
                return pt;
            }
            catch
            {
                return new Point();
            }
        }

        static public List<PointF> PointFListParse(string str)
        {
            try
            {
                List<PointF> ptList = new List<PointF>();
                string[] val = str.Split(',');
                for (int i = 0; i < val.Length / 2; i++)
                {
                    ptList.Add(new PointF(float.Parse(val[2 * i]), float.Parse(val[2 * i + 1])));
                }
                return ptList;
            }
            catch
            {
                return new List<PointF>();
            }
        }

        static public List<Point> PointListParse(string str)
        {
            try
            {
                List<Point> ptList = new List<Point>();
                string[] val = str.Split(',');
                for (int i = 0; i < val.Length / 2; i++)
                {
                    ptList.Add(new Point(int.Parse(val[2 * i]), int.Parse(val[2 * i + 1])));
                }
                return ptList;
            }
            catch
            {
                return new List<Point>();
            }
        }

        static public Size SizeParse(string str)
        {
            try
            {

                Size s = new Size();
                string[] val = str.Split(',');
                if (val.Length > 1)
                {
                    s.Width = int.Parse(val[0]);
                    s.Height = int.Parse(val[1]);
                }
                return s;
            }
            catch
            {
                return new Size();
            }
        }

        static public SizeF SizeFParse(string str)
        {
            try
            {

                SizeF s = new SizeF();
                string[] val = str.Split(',');
                if (val.Length > 1)
                {
                    s.Width = float.Parse(val[0]);
                    s.Height = float.Parse(val[1]);
                }
                return s;
            }
            catch
            {
                return new SizeF();
            }
        }

        static public RectangleF RectangleFParse(string str)
        {
            try
            {
                RectangleF r = new RectangleF();
                string[] val = str.Split(',');
                if (val.Length > 3)
                {
                    r.X = float.Parse(val[0]);
                    r.Y = float.Parse(val[1]);
                    r.Width = float.Parse(val[2]);
                    r.Height = float.Parse(val[3]);
                }
                return r;
            }
            catch
            {
                return new RectangleF();
            }
        }

        static public string RectangleFToString(RectangleF rt)
        {
            return rt.X.ToString() + "," + rt.Y.ToString() + "," + rt.Width.ToString() + "," + rt.Height.ToString();
        }


        static public Rectangle RectangleParse(string str)
        {
            try
            {
                Rectangle r = new Rectangle();
                string[] val = str.Split(',');
                if (val.Length > 3)
                {
                    r.X = int.Parse(val[0]);
                    r.Y = int.Parse(val[1]);
                    r.Width = int.Parse(val[2]);
                    r.Height = int.Parse(val[3]);
                }
                return r;
            }
            catch
            {
                return new Rectangle();
            }
        }

        static public string RectangleToString(Rectangle rt)
        {
            return rt.X.ToString() + "," + rt.Y.ToString() + "," + rt.Width.ToString() + "," + rt.Height.ToString();
        }

        static public int IntParse(string str, int val =0)
        {
            try
            {
                if (str != null && str != "")
                    return int.Parse(str);
                return val;
            }
            catch
            {
                return val;
            }
        }

        static public bool BoolParse(string str, bool val = false)
        {
            try
            {
                if (str != null && str != "")
                    return bool.Parse(str);
                return val;
            }
            catch
            {
                return val;
            }
        }

        static public float FloatParse(string str, float val =0)
        {
            try
            {
                if (str != null && str != "")
                    return float.Parse(str);
                return val;
            }
            catch
            {
                return val;
            }
        }

        static public IList<int> IntListParse(string str)
        {
            IList<int> list = new List<int>();
            if (str.Length > 0)
            {
                string[] val = str.Split(',');
                for (int i = 0; i < val.Length; i++)
                {
                    list.Add(int.Parse(val[i]));
                }
            }
            return list;
        }

        static public IList<float> FloatListParse(string str)
        {
            IList<float> list = new List<float>();
            if (str.Length > 0)
            {
                string[] val = str.Split(',');
                for (int i = 0; i < val.Length; i++)
                {
                    list.Add(float.Parse(val[i]));
                }
            }
            return list;
        }

        static public string FloatListToString(IList<float> valList)
        {
            if (valList == null || valList.Count == 0)
                return "";

            string str = "";
            for (int i = 0; i < valList.Count; i++)
            {
                str += valList[i].ToString();
                if (i != valList.Count - 1)
                    str += ",";
            }
            return str;

        }

        public static double Dist(PointF p1, PointF p2)
        {
            float x = p1.X - p2.X;
            float y = p1.Y - p2.Y;
            return Math.Sqrt(x * x + y * y);
        }

    }

    public struct MyRange
    {
        public int _min;
        public int _max;


        public MyRange(int min = 0, int max = 0)
        {
            _min = min;
            _max = max;
        }
    }

    public struct MyRangeF
    {
        public float _min;
        public float _max;

        public MyRangeF(float min = 0, float max = 0)
        {
            _min = min;
            _max = max;
        }


        public bool Contain(float r)
        {
            if (_min <= r && r <= _max)
                return true;
            return false;
        }
    }
}
