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


/*
 * 
 *  1. 判断对象object是否被cut，确定其是否显示？？避免出现混乱的结果？
 *  2. 图案不美化，分布逻辑调整，对象之间的间距要比较大一些。
 * 
 * 
 * 
 * 
 * 
 */
namespace Origami
{
    class OrigamiConfuseScheme
    {
        public static bool _unDoregionIntersect = true;
        public static bool _enableResizeObj = true;
        public static bool _enableRotateObj = true;
        public static bool _unMapRegion = true;
        public static bool _unPaper = true;

        public static int _standSize = 70;
        public static Size _standRtSz = new Size(45, 60);
        public static Random _rand = new Random(Environment.TickCount);

        public static double[] _objProbabilities = new double[] { 0.25, 0.3, 0.4, 0.05 };

        public static double[] _objLocProbabilities = new double[] { 0.2, 0.3, 0.5 };

        public static void RestoreRate()
        {
            _objProbabilities = new double[] { 0.25, 0.3, 0.4, 0.05 };
            _objLocProbabilities = new double[] { 0.2, 0.3, 0.5 };
        }

        public static Region CreateRandomCutRegion(OrigamiCut cutStep)
        {
            Region region = null;

            Point[] shifts = new Point[4];
            int id = _rand.Next() % 8;
            if (id < 4)
            {
                shifts[id] = new Point(_rand.Next() % 12 + 12, _rand.Next() % 12 + 12);
            }
            if (cutStep._cutObjs._squareList.Count>0)
            {
                int[] candidateSlopes = new int[] { 0, 45 };
                int slope = candidateSlopes[_rand.Next() % candidateSlopes.Length];
                region = UnionRegionRect(region, cutStep._cutObjs._squareList[0], 1, slope, shifts[0]);
            }

            if ( cutStep._cutObjs._circleList.Count>0)
            {
                double rate = _rand.NextDouble();// *0.4 + 0.8;
                if (rate > 0.5)
                    rate = 1.1 + rate * 0.2;
                else
                    rate = 0.85 - rate * 0.1;
                region = UnionRegionEllipse(region, cutStep._cutObjs._circleList[0], rate, rate, 0, shifts[1]);
            }

            if ( cutStep._cutObjs._rtList.Count>0)
            {
                int[] candidateSlopes = new int[] {45, 90 };
                int slope = candidateSlopes[_rand.Next() % candidateSlopes.Length];
                region = UnionRegionRect(region, cutStep._cutObjs._rtList[0], 1, 90, shifts[2]);
            }

            if ( cutStep._cutObjs._hexagonList.Count>0)
            {
                int[] candidateSlopes = new int[] { 15, 30, 45 };
                int slope = candidateSlopes[_rand.Next() % candidateSlopes.Length];
                double rate = _rand.NextDouble();// *0.4 + 0.8;
                if (rate > 0.5)
                    rate = 1.1 + rate * 0.2;
                else
                    rate = 0.85 - rate * 0.1;
                region = UnionRegionHexagon(region, cutStep._cutObjs._hexagonList[0], rate, slope, shifts[3]);
            }
            if (!region.IsVisible(new Rectangle(0,0,360,360)))
            {
                int err = 0;
            }
            return region;
        }

        //获取各个可能的Region，甚至是漏掉部分？？
        public static List<Region> CreateCutRegionList(OrigamiCut cutStep)
        {
            List<Region> regList = new List<Region>();
            regList.Add(cutStep.CreateCutRegion());
            List<object> objList = new List<object>();
            if(cutStep._cutObjs._squareList.Count>0)
                objList.Add(cutStep._cutObjs._squareList);
            if(cutStep._cutObjs._circleList.Count>0)
                objList.Add(cutStep._cutObjs._circleList);
            if(cutStep._cutObjs._hexagonList.Count>0)
                objList.Add(cutStep._cutObjs._hexagonList);
            if(cutStep._cutObjs._rtList.Count>0)
                objList.Add(cutStep._cutObjs._rtList);
                    
            for (int i = 0; i < objList.Count; i++)
            {
                Region region = null;
                for (int j = 0; j < objList.Count; j++)
                {
                    if (i == j)
                        continue;
                    if (objList[j]==cutStep._cutObjs._squareList)
                        region = UnionRegionRect(region, cutStep._cutObjs._squareList[0]);
                    
                    if ( objList[j]==cutStep._cutObjs._circleList)
                        region = UnionRegionEllipse(region, cutStep._cutObjs._circleList[0]);

                    if (objList[j] == cutStep._cutObjs._rtList)
                        region = UnionRegionRect(region, cutStep._cutObjs._rtList[0]);

                    if (objList[j] == cutStep._cutObjs._hexagonList)
                        region = UnionRegionHexagon(region, cutStep._cutObjs._hexagonList[0]);
                }
                if(region!=null)
                    regList.Add(region);
            }
       //     return regList;

            for (int i = 0; i < objList.Count; i++)
            {
                Region region = null;

                for (int j = 0; j < objList.Count; j++)
                {
                    double rate = _rand.NextDouble();// *0.4 + 0.8;
                    if (rate > 0.5)
                        rate = 1.1 + rate * 0.2;
                    else
                        rate = 0.85 - rate * 0.1;
                    if (objList[j] == cutStep._cutObjs._squareList)
                    {
                        region = UnionRegionRect(region, cutStep._cutObjs._squareList[0], rate);
                    }

                    if (objList[j] == cutStep._cutObjs._circleList)
                        region = UnionRegionEllipse(region, cutStep._cutObjs._circleList[0], rate);

                    if (objList[j] == cutStep._cutObjs._rtList)
                        region = UnionRegionRect(region, cutStep._cutObjs._rtList[0], rate);

                    if (objList[j] == cutStep._cutObjs._hexagonList)
                        region = UnionRegionHexagon(region, cutStep._cutObjs._hexagonList[0], rate);
                }
                if (region != null)
                    regList.Add(region);
            }
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < objList.Count; i++)
                {
                    Region region = null;

                    for (int j = 0; j < objList.Count; j++)
                    {

                        if (objList[j] == cutStep._cutObjs._squareList)
                        {
                            int[] candidateSlopes = new int[] { 0, 45 };
                            int slope = candidateSlopes[_rand.Next() % candidateSlopes.Length];
                            region = UnionRegionRect(region, cutStep._cutObjs._squareList[0], 1, slope);
                        }

                        if (objList[j] == cutStep._cutObjs._circleList)
                        {
                            double rate = _rand.NextDouble();// *0.4 + 0.8;
                            if (rate > 0.5)
                                rate = 1.1 + rate * 0.2;
                            else
                                rate = 0.85 - rate * 0.1;
                            region = UnionRegionEllipse(region, cutStep._cutObjs._circleList[0], rate, rate);
                        }

                        if (objList[j] == cutStep._cutObjs._rtList)
                        {
                            int[] candidateSlopes = new int[] {45, 90 };
                            int slope = candidateSlopes[_rand.Next() % candidateSlopes.Length];
                            region = UnionRegionRect(region, cutStep._cutObjs._rtList[0], 1, 90);
                        }

                        if (objList[j] == cutStep._cutObjs._hexagonList)
                        {
                            int[] candidateSlopes = new int[] { 15, 30, 45 };
                            int slope = candidateSlopes[_rand.Next() % candidateSlopes.Length];
                            double rate = _rand.NextDouble();// *0.4 + 0.8;
                            if (rate > 0.5)
                                rate = 1.1 + rate * 0.2;
                            else
                                rate = 0.85 - rate * 0.1;
                            region = UnionRegionHexagon(region, cutStep._cutObjs._hexagonList[0], rate, slope);
                        }
                    }
                    if (region != null)
                        regList.Add(region);
                }
            }

            if (regList.Count > 1)
            {
                int err = 0;
            }
            return regList;
        }

        public static Region UnionRegionRect(Region region, RectangleF rt, double scale = 1, int slope = 0, Point shift = new Point())
        {
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(rt);
            path = TransformRegion(path, (float)scale, (float)scale, (float)slope);
            Region tmp = new Region(path);

            if (region != null)
                region.Union(tmp);
            else
                region = tmp;
            return region;
        }


        public static Region UnionRegionEllipse(Region region, RectangleF rt, double scalex = 1, double scaley = 1, int slope = 0,Point shift = new Point())
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(rt);
            path = TransformRegion(path, (float)scalex, (float)scaley, (float)slope, shift);
            Region tmp = new Region(path);
            if (region != null)
                region.Union(tmp);
            else
                region = tmp;
            return region;
        }

        public static Region UnionRegionHexagon(Region region, RectangleF rt, double scale =1, double slope = 0, Point shift = new Point())
        {
            PointF cenPt = new PointF(rt.X + rt.Width / 2f, rt.Y + rt.Height / 2f);
            float radius = rt.Width / 2f;
            List<PointF> points = new List<PointF>();
            float angleStep = (float)(Math.PI * 2 / 6);
            slope = slope / 180 * Math.PI;
            for (int k = 0; k < 6; k++)
            {
                float angle = (float)(k * angleStep - Math.PI / 2); // 从顶部开始
                float x = cenPt.X + radius * (float)Math.Cos(angle);
                float y = cenPt.Y + radius * (float)Math.Sin(angle);
                points.Add(new PointF(x, y));
            }


            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(points.ToArray());
            path = TransformRegion(path, (float)scale, (float)scale, (float)slope, shift);
            Region tmp = new Region(path);
            if (region != null)
                region.Union(tmp);
            else
                region = tmp;
            return region;
        }

        public static GraphicsPath TransformRegion(GraphicsPath path, float scalex, float scaley, float rotationAngle, Point shift = new Point())
        {
            RectangleF bounds = path.GetBounds();
            {
                // 获取区域边界矩形

                PointF center = new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);

                // 创建组合变换矩阵
                Matrix matrix = new Matrix();

                // 1. 平移到原点
                matrix.Translate(-center.X, -center.Y, MatrixOrder.Append);

                // 2. 缩放
                matrix.Scale(scalex, scaley, MatrixOrder.Append);

                // 3. 旋转
                matrix.Rotate(rotationAngle, MatrixOrder.Append);

                // 4. 平移回原位置
                matrix.Translate(center.X, center.Y, MatrixOrder.Append);

                matrix.Translate(shift.X, shift.Y, MatrixOrder.Append);

                // 应用变换到路径
                path.Transform(matrix);

                // 创建变换后的新区域
                return path;
            }
        }
        public static List<Region> _candidateRegionList = new List<Region>();

        public static List<Bitmap> RenderOneFold(OrigamiCut cutStep, int minNum=4)
        {
            try
            { 
                //对于任意一个布局？？当前的cutStep??

                //生成各类对象？

                //独立的？判断是否有差异？？
                //
                //生成随机的region.
                //选择是否随机生成随机的region.
                cutStep.InitCandidateRegionList();
 
                //随机选一个?
                // 绘制： {第一个区域绘制} 随机开启是否交集？
                // 原始区域：开启区域的是否合并？
                // 第二个：翻折区域？先任选一个区域？选择开启是否合并？是否显示？？等等？
                //随机再选一个？
                //折纸区域1： 原始REGION。开启交集？？
                //折纸区域2： 任选一个区域，交集设置，是否映射设置一定的概率？？

                Bitmap answerBmp = cutStep.RenderAnswer();
                List<Bitmap> bmpList = new List<Bitmap>();
                bmpList.Add(answerBmp);
                int loopNum = 0;
                int maxLoop = minNum * 3;
                while (bmpList.Count < minNum)
                {
                    if (loopNum > maxLoop)
                    {
                        break;
                    }
                    loopNum++;
                    Bitmap bmp = cutStep.RenderOneFold();
                    if (bmp == null)
                        continue;


                    if (!OrigamiEngine.IsImgSSM(answerBmp, bmpList, bmp))
                        bmpList.Add(bmp);
                }



                cutStep._cutObjs._candidateList = bmpList;
                cutStep.ClearCandidateRegionList();
                return bmpList;


            }
            catch(Exception ex)
            { 
            }
            return null;
        }

        public static List<Region> FilterEqualRegion(List<Region> srcList)
        {
            List<Region> regList = new List<Region>();
            if (srcList.Count > 0)
                regList.Add(srcList[0]);

            for (int i = 1; i < srcList.Count; i++)
            {
                bool isSame = false;
                foreach (Region reg in regList)
                {
                    if(RegionsAreEqual(reg, srcList[i]))
                    {
                        isSame = true;
                        break;
                    }
                }
                if(!isSame)
                    regList.Add(srcList[i]);
            }

            return regList;
        }

        public static bool RegionsAreEqual(Region region1, Region region2)
        {
            try
            {
                using (Region temp = region1.Clone())
                {
                    temp.Xor(region2);
                    return temp.IsEmpty(Graphics.FromHwnd(IntPtr.Zero));
                }
            }
            catch
            { }
            return false;
        }

        public  static List<OrigamiCutObjs> RandomCreateCutObjsList(OrigamiCut cutStep, int num=30)
        {
            List<OrigamiCutObjs> cutObjsList = new List<OrigamiCutObjs>();
            while (cutObjsList.Count < num)
            {
                cutStep.RandAddObj();
                if (cutObjsList.Count == 0)
                    cutObjsList.Add(cutStep._cutObjs.Clone());
                else
                {
                    foreach (OrigamiCutObjs tmp in cutObjsList)
                    {
                        if (IsNearObj(tmp, cutStep._cutObjs))
                            continue;
                    }
                    cutObjsList.Add(cutStep._cutObjs.Clone());
                }
            }

            return cutObjsList;
        }

        public static bool IsNearObj(OrigamiCutObjs obj, OrigamiCutObjs obj2, double distTh = 20)
        {
            if (obj2._rtList.Count == obj._rtList.Count &&
                obj2._squareList.Count == obj._squareList.Count &&
                obj2._circleList.Count == obj._circleList.Count &&
                obj2._hexagonList.Count == obj._hexagonList.Count)
            {
                double maxDist = 0;
                if (obj2._rtList.Count > 0)
                {
                    maxDist = Math.Max(maxDist, GlobalUtils.Dist(obj2._rtList[0].Location, obj._rtList[0].Location));
                }
                if (obj2._squareList.Count > 0)
                {
                    maxDist = Math.Max(maxDist, GlobalUtils.Dist(obj2._squareList[0].Location, obj._squareList[0].Location));
                }
                if (obj2._circleList.Count > 0)
                {
                    maxDist = Math.Max(maxDist, GlobalUtils.Dist(obj2._circleList[0].Location, obj._circleList[0].Location));
                }
                if (obj2._hexagonList.Count > 0)
                {
                    maxDist = Math.Max(maxDist, GlobalUtils. Dist(obj2._hexagonList[0].Location, obj._hexagonList[0].Location));
                }
                return maxDist < distTh;
            }


            return false;
        }


        public Bitmap RenderCandidateAnswer(OrigamiCut cutStep)
        {
            try
            {

                OrigamiStep startStep = cutStep.GetRootStep();
                Bitmap bmp = startStep.Render();
                Graphics memDc = Graphics.FromImage(bmp);
                Region paperRegion = (startStep as OrigamiStart).CreatePaperRegion();

                memDc.TranslateTransform(OrigamiStart._offset.X, OrigamiStart._offset.Y);

                Brush brush = new SolidBrush(OrigamiSetting._cutFillClr);
                Pen pen1 = new Pen(OrigamiSetting._cutClr, OrigamiSetting._lineWidth);

                List<ClipAction> clipActionList = new List<ClipAction>();

                if (cutStep._prevStep is OrigamiFolding)
                    clipActionList = (cutStep._prevStep as OrigamiFolding)._clipActionList;
                else if (cutStep._prevStep is OrigamiStart)
                    clipActionList.Add((cutStep._prevStep as OrigamiStart)._startAction);
                Region region = cutStep.CreateCutRegion();

                if (region != null)
                {

                    foreach (ClipAction action in clipActionList)
                    {
                        int id = 0;


                        foreach (FoldingPolygon polygon in action._foldingPolyList)
                        {
                            id++;
                            //int[] bugList = new int[] {3 };
                            //if (!bugList.Contains(id))
                            //    continue;
                            Region clipRegion = cutStep.InverseMap(memDc, region, polygon);
                            memDc.FillRegion(brush, clipRegion);
                        }

                    }
                }

                memDc.Flush();
                memDc.Dispose();

                return bmp;
            }
            catch
            { }
            return null;
        }

        public static Point  GetRandPos(int size)
        {
            return new Point (_rand.Next() % size, _rand.Next() % size);
        }

        public static Bitmap GenRandomBitmap(int size = 128)
        {
            Bitmap bmp = new Bitmap(size, size );
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.White, new Rectangle(0,0, size, size));

            int num = GetRandNObjNum();

            num = Math.Min(2, num);
            List<int> objTypeList = new List<int>();
            int numofType = _objProbabilities.Length;

            for (int i = 0; i < numofType; i++)
                objTypeList.Add(i);
            Pen pen = new Pen(Color.Blue, 5);

              int       standSize = 64;
              Size  standRtSz = new Size(48, 64);
            for (int i = 0; i < num; i++)
            {
                int typeid = _rand.Next() % (objTypeList.Count);

                //添加四类对象
                Point  cenPt = GetRandPos(size);
                if (typeid==0)
                {

                    Rectangle rt = new Rectangle(cenPt.X - standSize / 2, cenPt.Y - standSize / 2, standSize, standSize);


                    if (_rand.NextDouble() > 0.3)
                        g.FillRectangle(Brushes.Blue, rt);
                    else
                        g.DrawRectangle(pen, rt);
                        

                }
                if (typeid == 1)
                {
                    Rectangle rt = new Rectangle(cenPt.X - standSize / 2, cenPt.Y - standSize / 2, standSize, standSize);
                    if (_rand.NextDouble() > 0.3)
                        g.FillEllipse(Brushes.Blue, rt);
                    else
                        g.DrawEllipse(pen, rt);
                }
                if (typeid == 2)
                {
                    //PointF cenPt = GetRandLoc(cutStep);
                    //if (cenPt.X > 360 || cenPt.Y > 360)
                    //{
                    //    int err = 0;
                    //}
                    Rectangle rt = new Rectangle(cenPt.X - standRtSz.Width / 2, cenPt.Y - standRtSz.Height / 2, standRtSz.Width, standRtSz.Height);
                    if (_rand.NextDouble() > 0.3)
                        g.FillRectangle(Brushes.Blue, rt);
                    else
                        g.DrawRectangle(pen, rt);
                }
                if (typeid ==3)
                {

                    float radius = standSize / 2f;
                    List<PointF> points = new List<PointF>();
                    float angleStep = (float)(Math.PI * 2 / 6);
                    int randomAngle = _rand.Next(60);
                    for (int k = 0; k < 6; k++)
                    {
                        float angle = (float)(k * angleStep - Math.PI / 2) + randomAngle; // 从顶部开始
                        float x = cenPt.X + radius * (float)Math.Cos(angle);
                        float y = cenPt.Y + radius * (float)Math.Sin(angle);
                        points.Add(new PointF(x, y));
                    }


                    GraphicsPath path = new GraphicsPath();
                    path.AddPolygon(points.ToArray());
                    if (_rand.NextDouble() > 0.3)
                        g.FillPath(Brushes.Blue, path);
                    else
                        g.DrawPath(pen, path);
                    path.Dispose();

                   // Region tmp = new Region(path);
                    //PointF cenPt = GetRandLoc(cutStep);
                    //if (cenPt.X > 360 || cenPt.Y > 360)
                    //{
                    //    int err = 0;
                    //}
                    //RectangleF rt = new RectangleF(cenPt.X - _standSize / 2, cenPt.Y - _standSize / 2, _standSize, _standSize);
                    //cutStep._cutObjs._hexagonList.Add(rt);
                }

            }

  


  


            return bmp;
        }


        public static int GetRandNObjNum()
        {
            List<double> objProbHighList = new List<double>();
            double cur = 0;
            foreach (double val in _objProbabilities)
            {
                cur += val;
                objProbHighList.Add(cur);
            }
            
            double tmp = _rand.NextDouble();
            for(int i=0; i<objProbHighList.Count; i++)
                if(tmp<=objProbHighList[i])
                    return i+1;
            return 1;
        }

        public static void RandomAddObj(OrigamiCut cutStep)
        {
            cutStep.ClearObj();
            int num = GetRandNObjNum();
            List<int> objTypeList = new List<int>();
            int numofType = _objProbabilities.Length;
            
            for(int i=0; i<numofType; i++)
                objTypeList.Add(i);
            List<int> candidateList = new List<int>();
            for (int i = 0; i < num; i++)
            {
                int id = _rand.Next() % (objTypeList.Count);
                candidateList.Add(objTypeList[id]);
                objTypeList.RemoveAt(id);
            }

            RectangleF box = cutStep.GetBounding();
            //添加四类对象
            if (candidateList.Contains(0))
            {
                PointF cenPt = GetRandLoc(cutStep);
                if (cenPt.X > 360 || cenPt.Y > 360)
                {
                    int err = 0;
                }
                RectangleF rt = new RectangleF(cenPt.X - _standSize / 2, cenPt.Y - _standSize / 2, _standSize, _standSize);

                cutStep._cutObjs._squareList.Add(rt);
                
            }
            if (candidateList.Contains(1))
            {
                PointF cenPt = GetRandLoc(cutStep);
                if (cenPt.X > 360 || cenPt.Y > 360)
                {
                    int err = 0;
                }
                RectangleF rt = new RectangleF(cenPt.X - _standSize / 2, cenPt.Y - _standSize / 2, _standSize, _standSize);
                cutStep._cutObjs._circleList.Add(rt);
            }
            if (candidateList.Contains(2))
            {
                PointF cenPt = GetRandLoc(cutStep);
                if (cenPt.X > 360 || cenPt.Y > 360)
                {
                    int err = 0;
                }
                RectangleF rt = new RectangleF(cenPt.X - _standRtSz.Width / 2, cenPt.Y - _standRtSz.Height / 2, _standRtSz.Width, _standRtSz.Height);
                cutStep._cutObjs._rtList.Add(rt);
            }
            if (candidateList.Contains(3))
            {
                PointF cenPt = GetRandLoc(cutStep);
                if (cenPt.X > 360 || cenPt.Y > 360)
                {
                    int err = 0;
                }
                RectangleF rt = new RectangleF(cenPt.X - _standSize / 2, cenPt.Y - _standSize / 2, _standSize, _standSize);
                cutStep._cutObjs._hexagonList.Add(rt);
            }
        }

        public static PointF GetRandLoc(OrigamiCut cutStep)
        {
            List<double> objLocTypeList = new List<double>();
            double cur = 0;
            foreach (double val in _objLocProbabilities)
            {
                cur += val;
                objLocTypeList.Add(cur);
            }
            double randVal = _rand.NextDouble();
            ObjCenLocType locType = ObjCenLocType.Edge;
            for (int i = 0; i < objLocTypeList.Count; i++)
            {
                if (randVal <= objLocTypeList[i])
                {
                    locType = (ObjCenLocType)i;
                    break;
                }
            }
            switch (locType)
            {
                case ObjCenLocType.Vertex:
                    return GetRandomVertexLoc(cutStep);

                case ObjCenLocType.Edge:
                    return GetRandomEdgeLoc(cutStep);
                case ObjCenLocType.Rand:
                    return GetRandomLoc(cutStep);



            }
            return new PointF();
        }

        public static PointF GetRandomVertexLoc(OrigamiCut cutStep)
        {
            ClipAction action = cutStep._prevStep.GetLast();
            List<PointF> ptList = action.GetVertexList();
            int id = _rand.Next() % ptList.Count;
            PointF pt = ptList[id];
            if (pt.X > 360 || pt.Y > 360)
            {
                int err = 0;
            }
            return pt;
        }

        public static PointF LinearEdgePt(LineSegment2DF line, float rate)
        {
            PointF pt1 = line.P1;
            PointF pt2 = line.P2;
            float x = pt1.X * (1 - rate) + pt2.X*rate;
            float y = pt1.Y * (1 - rate) + pt2.Y * rate;
            return new PointF(x, y);
        }

        public static PointF GetRandomEdgeLoc(OrigamiCut cutStep)
        {
            try
            {
                ClipAction action = cutStep._prevStep.GetLast();
                List<LineSegment2DF> edgeList = action.GetLastEdgeList();
                int id = _rand.Next() % edgeList.Count;
          //      return LinearEdgePt(edgeList[id], (float)_rand.NextDouble());
                PointF pt = LinearEdgePt(edgeList[id], 0.5f);
                if (pt.X > 360 || pt.Y > 360)
                {
                    int err = 0;
                }
                return pt;
 
            }
            catch
            { }

            return new PointF();
        }

        public static PointF GetRandomLoc(OrigamiCut cutStep)
        {
            Region cutUnionRegion = cutStep.CreateMaxActionRegion();
            RectangleF box = cutStep.GetBounding();
            int maxLoop = 20;
            int loop =0;
            PointF pt = new PointF();
            while (loop < maxLoop)
            {

                float x = box.X + (float)( 0.2+0.6* _rand.NextDouble() * box.Width);
                float y = box.Y + (float)(0.2+0.6*_rand.NextDouble() * box.Height);
                pt = new PointF(x, y);
                if (cutUnionRegion.IsVisible(pt))
                {
                    if (pt.X > 360 || pt.Y > 360)
                    {
                        int err = 0;
                    }
                    return pt;
                }
                loop++;
                
            }
            return pt;
        }
    }

    public enum ObjCenLocType
    {
        Vertex=0,
        Edge,
        Rand,
    }
}
