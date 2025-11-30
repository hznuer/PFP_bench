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
 
Generate random cutout regions
a. Calculate candidate regions.
b. Set the objects to be filled in candidate regions.
c. Set the size of candidate regions.
d. In a sub-region, random position?
Count (1-4 pieces)
(Random position)
(Random type) (etc.)
Generate candidate answers
Always include various answers?
To simplify the logic, a preliminary judgment of the objects can be made.
How many folds are there?
How many objects are there?
If there are fewer folds and fewer objects,
consider appropriately changing the size of the objects to test if they can be recognized.
One fold is relatively simple.
Two or three folds may lead to more logical variations.
Recommendations:
a. For the intersection algorithm between each region and the cutout region, you can choose to implement or skip the intersection calculation (final intersection with the paper).
b. For multiple folded areas, some regions may not be expanded. For 2 or more folds (with at least 3-4 intervals), partial selection is allowed.
c. For partially expanded regions, you can choose the rotation angle.
d. More designs? What are the approximate answers?
Compare candidate answers that may have conflicts
Image analysis: CalImgSSM
Simulate answering
Database analysis
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */

namespace Origami
{
    
    class OrigamiCut : OrigamiStep
    {
        public OrigamiCutObjs _cutObjs = new OrigamiCutObjs();
        //public List<RectangleF> _squareList = new List<RectangleF>();//1:1
        //public List<RectangleF> _circleList = new List<RectangleF>();//1:1
        //public List<RectangleF> _rtList = new List<RectangleF>();//3:4
        //public List<RectangleF> _hexagonList = new List<RectangleF>();//六边形：外接矩形。
        public Region _cutUnionRegion = null;
        public Region _startRegion = null;
 

        public OrigamiCut()
        { 
        }

        public override Bitmap Render()
        {
            try
            {
                if (_prevStep != null)
                {
                    Bitmap bmp = _prevStep.Render();

                    Graphics memDc = Graphics.FromImage(bmp);

                    Brush brush = new SolidBrush(OrigamiSetting._cutFillClr);
                    Pen pen1 = new Pen(OrigamiSetting._cutClr, OrigamiSetting._lineWidth);
                    //       memDc.TranslateTransform(OrigamiSetting._margin, OrigamiSetting._margin);
                    memDc.TranslateTransform(OrigamiStart._offset.X, OrigamiStart._offset.Y);

                    Region region = CreateCutRegion();
                    Region maxActionRegion = CreateMaxActionRegion();
                    region.Intersect(maxActionRegion);
                    try
                    {
                        memDc.FillRegion(brush, region);
                        //     memDc.DrawPath(pen1, new GraphicsPath());
                    }
                    catch
                    { }

                    memDc.Flush();
                    memDc.Dispose();
                    return bmp;
                }

            }
            catch { }
            return null;
        }

        public  Bitmap RenderCutObjs()
        {
            try
            {
                if (_prevStep != null)
                {
                    OrigamiStep root = GetRootStep();
                    RectangleF outRt = root.GetBounding();

                    Bitmap bmp = new Bitmap(OrigamiStart._renderSz.Width, OrigamiStart._renderSz.Height);

                    Graphics memDc = Graphics.FromImage(bmp);

                    memDc.Clear(OrigamiSetting._bkClr);

                    Brush brush = new SolidBrush(OrigamiSetting._cutFillClr);
                    Pen pen1 = new Pen(OrigamiSetting._cutClr, OrigamiSetting._lineWidth);

                    memDc.TranslateTransform(OrigamiStart._offset.X, OrigamiStart._offset.Y);

                    Region region = CreateCutRegion();
                    Region maxActionRegion = CreateMaxActionRegion();
                    region.Intersect(maxActionRegion);
                    try
                    {
                        memDc.FillRegion(brush, region);
                        //     memDc.DrawPath(pen1, new GraphicsPath());
                    }
                    catch
                    { }

                    memDc.Flush();
                    memDc.Dispose();
                    return bmp;
                }
            
            }
            catch { }
            return null;
        }

        public void ClearObj()
        {
            _cutObjs.Clear();
            _cutUnionRegion = null;
            _startRegion = null;
        }

        public void RandAddObj()
        {
            try
            {
                OrigamiConfuseScheme.RandomAddObj(this);
                return;

                if (_prevStep != null)
                {
                    RectangleF box = GetBounding();

                    PointF cenPt = new PointF(270, 90);
                    _cutObjs._squareList.Add(new RectangleF(cenPt, new SizeF(30, 50)));
                    _cutObjs._circleList.Add(new RectangleF(cenPt, new SizeF(40, 40)));
                }
            }
            catch
            { }
        }

        public List<Region> _candidateRegionList = new List<Region>();

        public void InitCandidateRegionList()
        {
            _candidateRegionList = OrigamiConfuseScheme.CreateCutRegionList(this);
       //     _candidateRegionList = OrigamiConfuseScheme.FilterEqualRegion(_candidateRegionList);
            CreateMaxActionRegion();
            CreateStartRegion();
        }

        public void ClearCandidateRegionList()
        {
            _candidateRegionList.Clear();
        }

        public Bitmap RenderCandidateAnswer()
        {
            try
            {
                OrigamiStep startStep = GetRootStep();
                Bitmap bmp = startStep.Render();
                Graphics memDc = Graphics.FromImage(bmp);
                Region paperRegion = (startStep as OrigamiStart).CreatePaperRegion();

                memDc.TranslateTransform(OrigamiStart._offset.X, OrigamiStart._offset.Y);

                Brush brush = new SolidBrush(OrigamiSetting._cutFillClr);
                Pen pen1 = new Pen(OrigamiSetting._cutClr, OrigamiSetting._lineWidth);

                List<ClipAction> clipActionList = new List<ClipAction>();

                if (_prevStep is OrigamiFolding)
                    clipActionList = (_prevStep as OrigamiFolding)._clipActionList;
                else if (_prevStep is OrigamiStart)
                    clipActionList.Add((_prevStep as OrigamiStart)._startAction);
                Region region = CreateCutRegion();

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
                            Region clipRegion = InverseMap(memDc, region, polygon);
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

        public Random _rand = new Random(Environment.TickCount);

        public Bitmap RenderOneFold()
        {
            try
            {
                double randRate1 = _rand.NextDouble();
                double randRate2 = _rand.NextDouble();
                OrigamiStep startStep = GetRootStep();
                Bitmap bmp = startStep.Render();
                Graphics memDc = Graphics.FromImage(bmp);

                memDc.TranslateTransform(OrigamiStart._offset.X, OrigamiStart._offset.Y);

                Brush brush = new SolidBrush(OrigamiSetting._cutFillClr);
                Pen pen1 = new Pen(OrigamiSetting._cutClr, OrigamiSetting._lineWidth);

                List<ClipAction> clipActionList = new List<ClipAction>();

                if (_prevStep is OrigamiFolding)
                    clipActionList = (_prevStep as OrigamiFolding)._clipActionList;
                else if (_prevStep is OrigamiStart)
                    clipActionList.Add((_prevStep as OrigamiStart)._startAction);
         //       Region region = CreateCutRegion();
                
                if (_candidateRegionList.Count>0)
                {
                    foreach (ClipAction action in clipActionList)
                    {
                        foreach (FoldingPolygon polygon in action._foldingPolyList)
                        {
                            ////策略1: 一定的概率选择不同区域
                            Region region = null;
                            int tmpId = _rand.Next() % _candidateRegionList.Count;
                            if (polygon.matrixList.Count > 0)
                            {
                                //if (_candidateRegionList.Count > 1)
                                //{
                                //    //tmpId = _rand.Next() % (_candidateRegionList.Count-1);
                                //    //region = _candidateRegionList[tmpId+1].Clone();
                                    
                                //}
                                //else
                                //    region = _candidateRegionList[tmpId].Clone();
                                if (_rand.NextDouble() > 0.5)
                                    region = OrigamiConfuseScheme.CreateRandomCutRegion(this);
                                else
                                {
                                    if (_candidateRegionList.Count > 1)
                                    {
                                        tmpId = _rand.Next() % (_candidateRegionList.Count - 1);
                                        region = _candidateRegionList[tmpId + 1].Clone();
                                    }
                                    else
                                        region = _candidateRegionList[tmpId].Clone();
                                }

                            }
                            else
                                region = _candidateRegionList[0].Clone();


                            //策略2：一定的概率交集。
                            if (randRate1 < 0.8)
                            {
                                Region pathRegion = null;

                                //随机是否切割？？
                                if (polygon._ptList != null && polygon._ptList.Count() >= 3)
                                {
                                    GraphicsPath path = new GraphicsPath();
                                    path.AddPolygon(polygon._ptList);
                                    pathRegion = new Region(path);
                                    region.Intersect(pathRegion);
                                }
                            }
                     //       if (randRate2 < 0.95)
                            {
                                double[,] matrix = polygon.GenMatrix();
                                Matrix ttMat = new Matrix();

                                for (int i = polygon.matrixList.Count - 1; i >= 0; i--)
                                {
                                    region.Transform(GeometricUtils.InverseMatrix(polygon.matrixList[i]));

                                    ttMat.Multiply(GeometricUtils.InverseMatrix(polygon.matrixList[i]));
                                }
                                region.Intersect(_startRegion);
                               // region.Intersect(_cutUnionRegion);

                                memDc.FillRegion(brush, region);
                            }
                            //else if (randRate2 < 0.85)
                            //{
                            //    double[,] matrix = polygon.GenMatrix();
                            //    Matrix ttMat = new Matrix();

                            //    for (int i = polygon.matrixList.Count - 1; i >= 0; i--)
                            //    {
                            //    //    pathRegion.Transform(GeometricUtils.InverseMatrix(polygon.matrixList[i]));

                            //        ttMat.Multiply(GeometricUtils.InverseMatrix(polygon.matrixList[i]));
                            //    }
                            //    region.Transform(ttMat);
                            //    region.Intersect(_cutUnionRegion);

                            //    memDc.FillRegion(brush, region);
                            //}
                        }

                    }
                }

                memDc.Flush();
                memDc.Dispose();

                return bmp;
            }
            catch(Exception ex)
            { }
            return null;
        }

        public  List<Bitmap> RenderCandidateAnswers(int minNum=3)
        {
            try
            {
                Bitmap answerBmp = RenderAnswer();
                List<Bitmap> bmpList = new List<Bitmap>();

                int loopNum = 0;
                int maxLoop = minNum*2;
                while (bmpList.Count < minNum)
                {
                    if (loopNum > maxLoop)
                        return bmpList;
                    loopNum++;
                    Bitmap bmp = null;
                    if (!OrigamiEngine.IsImgSSM(answerBmp, bmpList, bmp))
                        bmpList.Add(bmp);
                }

                



                return bmpList;
            }
            catch
            {
                
            }

            return new List<Bitmap>();
        }

        public override Bitmap RenderAnswer()
        {
            OrigamiStep startStep = GetRootStep();
            Bitmap bmp = startStep.Render();
            Graphics memDc = Graphics.FromImage(bmp);

            memDc.TranslateTransform(OrigamiStart._offset.X, OrigamiStart._offset.Y);

            Brush brush = new SolidBrush(OrigamiSetting._cutFillClr);
            Pen pen1 = new Pen(OrigamiSetting._cutClr, OrigamiSetting._lineWidth);
 
            List<ClipAction> clipActionList = new List<ClipAction>();

            if (_prevStep is OrigamiFolding)
                clipActionList = (_prevStep as OrigamiFolding)._clipActionList;
            else if (_prevStep is OrigamiStart)
                clipActionList.Add((_prevStep as OrigamiStart)._startAction);
            Region region = CreateCutRegion();

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
                        Region clipRegion = InverseMap(memDc, region, polygon);
                        memDc.FillRegion(brush, clipRegion);
                    }

                }
            }

            memDc.Flush();
            memDc.Dispose();

            return bmp;
        }

        public Region CreateCutRegion()
        {
            Region region = null;
            foreach (RectangleF rt in _cutObjs._squareList)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(rt);
                Region tmp = new Region(path);
                if (region != null)
                    region.Union(tmp);
                else
                    region = tmp;
            }
            foreach (RectangleF rt in _cutObjs._circleList)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(rt);
                Region tmp = new Region(path);
                if (region != null)
                    region.Union(tmp);
                else
                    region = tmp;
            }
            foreach (RectangleF rt in _cutObjs._rtList)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(rt);
                Region tmp = new Region(path);
                if (region != null)
                    region.Union(tmp);
                else
                    region = tmp;
            }
            foreach (RectangleF rt in _cutObjs._hexagonList)
            {
                PointF cenPt = new PointF(rt.X + rt.Width / 2f, rt.Y + rt.Height / 2f);
                float radius = rt.Width / 2f;
                List<PointF> points = new List<PointF>();
                float angleStep = (float)(Math.PI * 2 / 6);

                for (int i = 0; i < 6; i++)
                {
                    float angle = (float)(i * angleStep - Math.PI / 2); // 从顶部开始
                    float x = cenPt.X + radius * (float)Math.Cos(angle);
                    float y = cenPt.Y + radius * (float)Math.Sin(angle);
                    points.Add(new PointF(x, y));
                }


                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(points.ToArray());
                Region tmp = new Region(path);
                if (region != null)
                    region.Union(tmp);
                else
                    region = tmp;
 
            }
            return region;
        }

        public Region CreateStartRegion()
        {
            try
            {
                if (_startRegion != null)
                    return _startRegion;
                OrigamiStart start = GetRootStep() as OrigamiStart;
                
                Region region = null;

                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddPolygon(start._polyList.ToArray());
                    if (region == null)
                        region = new Region(path);
                    else
                        region.Union(new Region(path));
                }
                _startRegion = region;
                return region;
            }
            catch
            { }
            return null;
        }


        public Region CreateMaxActionRegion()
        {
            try
            {
                if (_cutUnionRegion != null)
                    return _cutUnionRegion;
                ClipAction action = _prevStep.GetLast();
                Region region = null;
                foreach (FoldingPolygon polygon in action._foldingPolyList)
                {
                    if(polygon._ptList.Length<3)
                        continue;

                    GraphicsPath path = new GraphicsPath();
                    path.AddPolygon(polygon._ptList.ToArray());
                    if (region == null)
                        region = new Region(path);
                    else
                        region.Union(new Region(path));
                    
                }
                _cutUnionRegion = region;
                return region;
                
            }
            catch
            { }
            return null;
        }


        public Region InverseMap(Graphics memDc, Region region, FoldingPolygon polygon)
        {
            if (region == null)
                return null;
         //    = CreateCutRegion();
            Region pathRegion = null;
            if (polygon._ptList != null && polygon._ptList.Count() >= 3)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(polygon._ptList);
                pathRegion = new Region(path);
            }
            else
                return null;
            pathRegion.Intersect(region);
            double[,] matrix = polygon.GenMatrix();
            Matrix ttMat = new Matrix();
            for (int i = polygon.matrixList.Count - 1; i >= 0; i--)
            {
                pathRegion.Transform(GeometricUtils.InverseMatrix(polygon.matrixList[i]));

                ttMat.Multiply(GeometricUtils.InverseMatrix(polygon.matrixList[i]));
            }
            //ttMat.Elements[4] = -180;
           // pathRegion.Transform(ttMat);
            //matrix = GeometricUtils.CalculateRotationMatrix(180, 180, -90);
            //pathRegion.Transform(GeometricUtils.InverseMatrix(matrix));

            return pathRegion;
        }

        public List<Region> InverseMapList(Graphics memDc, Region region, FoldingPolygon polygon)
        {
            if (region == null)
                return null;

            Region pathRegion = null;
            if (polygon._ptList != null && polygon._ptList.Count() >= 3)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(polygon._ptList);
                pathRegion = new Region(path);
            }
            else
                return null;
            pathRegion.Intersect(region);
            double[,] matrix = polygon.GenMatrix();
            Matrix ttMat = new Matrix();
            for (int i = polygon.matrixList.Count - 1; i >= 0; i--)
            {
                pathRegion.Transform(GeometricUtils.InverseMatrix(polygon.matrixList[i]));

                ttMat.Multiply(GeometricUtils.InverseMatrix(polygon.matrixList[i]));
            }

            return new List<Region>();
        //    return pathRegion;
        }

        public override RectangleF GetBounding()
        {
            OrigamiFolding step = _prevStep as OrigamiFolding;
            if (step != null)
            {
                return step.GetLastClipActionBounding();
            }
            return new RectangleF();
        }

    }
}
