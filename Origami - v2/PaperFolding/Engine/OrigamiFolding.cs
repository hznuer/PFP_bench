using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;

namespace Origami
{
    public class OrigamiFolding : OrigamiStep
    {
        public List<ClipAction> _clipActionList = new List<ClipAction>();

        public OrigamiFolding()
        {
        
        }

        // This code requires the user to verify the validity of the ID. The system cannot intelligently calculate the object links between different paper folding regions.
        
        // Calculate based on the reverse-order layers from clipAction. If id = -1, all layers are included; if id = 1, only the first layer is included for folding.

        private void DoFolding(ClipAction action, FoldingParam param, int id = -1)
        {
            ClipAction tmpAction = new ClipAction();
            UpdateSegLine(param);
            tmpAction._param = param;

            List<FoldingPolygon> oldfoldingPloyList = action._foldingPolyList;
            int num = oldfoldingPloyList.Count;

            List<FoldingPolygon> newfoldingPloyList = new List<FoldingPolygon>();
 
            List<FoldingPolygon> tmpList = new List<FoldingPolygon>();
            double[,] matrix = GeometricUtils.CalReflectionMatrix(param._segLine.P1, param._segLine.P2);

            if (id == -1)
            {
                foreach (FoldingPolygon tmp in oldfoldingPloyList)
                {
                    
                    List<PointF> inList = GeometricUtils.DoClip(new List<PointF>(tmp._ptList), param._segLine);

                    if (inList.Count() > 2)
                    {
                        FoldingPolygon xx = new FoldingPolygon();
                        xx._ptList = inList.ToArray();
                        xx._matrix = tmp._matrix;
                        newfoldingPloyList.Add(xx);
                        xx.matrixList = DeepCopyHelper.DeepCopy(tmp.matrixList);
                    }

                    List<PointF> outList = GeometricUtils.DoClip(new List<PointF>(tmp._ptList), new LineSegment2DF(param._segLine.P2, param._segLine.P1));

                    if (outList.Count() > 2)
                    {
                        FoldingPolygon yy = new FoldingPolygon();
                        yy._matrix = GeometricUtils.MulMatrices(matrix, tmp._matrix);
                        Matrix invert = GeometricUtils.InverseMatrix(yy._matrix);
                        yy._ptList = GeometricUtils.ReflectionPoly(outList, matrix).ToArray();
                        yy._oldPtList = outList.ToArray();
                        tmpList.Add(yy);
                        yy.matrixList = DeepCopyHelper.DeepCopy(tmp.matrixList);
                        yy.matrixList.Add(matrix);
                    }
                }




            }
            else
            {
                for(int i=0; i<oldfoldingPloyList.Count; i++)
                {
                    FoldingPolygon tmp = oldfoldingPloyList[i];
                    if (i < oldfoldingPloyList.Count - id)
                    {
                        FoldingPolygon xx = new FoldingPolygon();
                        xx._ptList = DeepCopyHelper.DeepCopy<PointF[]>(tmp._ptList);
                        xx._matrix = tmp._matrix;
                        newfoldingPloyList.Add(xx);
                        xx.matrixList = DeepCopyHelper.DeepCopy(tmp.matrixList);
                    
                    }
                    else
                    {
                        List<PointF> inList = GeometricUtils.DoClip(new List<PointF>(tmp._ptList), param._segLine);

                        if (inList.Count() > 2)
                        {
                            FoldingPolygon xx = new FoldingPolygon();
                            xx._ptList = inList.ToArray();
                            xx._matrix = tmp._matrix;
                            newfoldingPloyList.Add(xx);
                            xx.matrixList = DeepCopyHelper.DeepCopy(tmp.matrixList);
                        }

                        List<PointF> outList = GeometricUtils.DoClip(new List<PointF>(tmp._ptList), new LineSegment2DF(param._segLine.P2, param._segLine.P1));

                        if (outList.Count() > 2)
                        {
                            FoldingPolygon yy = new FoldingPolygon();
                            yy._matrix = GeometricUtils.MulMatrices(matrix, tmp._matrix);
                            Matrix invert = GeometricUtils.InverseMatrix(yy._matrix);
                            yy._ptList = GeometricUtils.ReflectionPoly(outList, matrix).ToArray();
                            yy._oldPtList = outList.ToArray();
                            tmpList.Add(yy);
                            yy.matrixList = DeepCopyHelper.DeepCopy(tmp.matrixList);
                            yy.matrixList.Add(matrix);
                        }
                    }
                }

  
            }

            for (int i = tmpList.Count - 1; i >= 0; i--)
                newfoldingPloyList.Add(tmpList[i]);
            tmpAction._foldingPolyList = newfoldingPloyList;
            _clipActionList.Add(tmpAction);

        }

        public RectangleF GetLastClipActionBounding()
        {
            RectangleF rt = new RectangleF();
            ClipAction action = GetLast();
            if (action == null)
            {
                if (_prevStep != null)
                    action = _prevStep.GetLast();
            }
            if (action != null)
            {
                List<FoldingPolygon> foldingPtsList = action._foldingPolyList;
                bool isSet = false;

                float x1=0, x2=0, y1=0, y2 = 0;

                if (foldingPtsList != null && foldingPtsList.Count > 0 )
                {
                    foreach (FoldingPolygon pts in foldingPtsList)
                    {
                        foreach (PointF pt in pts._ptList)
                        {
                            if (isSet)
                            {
                                x1 = Math.Min(x1, pt.X);
                                x2 = Math.Max(x2, pt.X);
                                y1 = Math.Min(y1, pt.Y);
                                y2 = Math.Max(y2, pt.Y);
                            }
                            else
                            {
                                x1 = pt.X;
                                x2 = x1;
                                y1 = pt.Y;
                                y2 = y1;
                                isSet = true;
                            }
                        }
                        if (isSet)
                            rt = new RectangleF(x1, y1, x2 - x1, y2 - y1);
                    }
 
                }
            }

            return rt;
            
        }

        public void UpdateSegLine(FoldingParam param)
        {
            RectangleF box = this.GetLastClipActionBounding();
            switch (param._type)
            {
                //左手法则？？
                case FoldingType.Hori_Left:
                case FoldingType.Hori_Right:
                    {
                        PointF pt1 = new PointF((int)(box.X + box.Width *param._rate+0.5f), box.Bottom);
                        PointF pt2 = new PointF(pt1.X, box.Top);
                        if (param._type == FoldingType.Hori_Left)
                            param._segLine = new LineSegment2DF(pt1, pt2);
                        else
                            param._segLine = new LineSegment2DF(pt2, pt1);
 
                    }

                    break;
                case FoldingType.Vert_Top:
                case FoldingType.Vert_Bottom:
                    {
                        PointF pt1 = new PointF(box.X, (int)(box.Y + box.Height *param._rate+0.5f));
                        PointF pt2 = new PointF(box.Right, pt1.Y);
                        if (param._type == FoldingType.Vert_Top)
                            param._segLine = new LineSegment2DF(pt1, pt2);
                        else
                            param._segLine = new LineSegment2DF(pt2, pt1);
                     }
                    break;
                case FoldingType.Angle:
                    {
                    }
                    break;
            }
        }

        public void DoClip(PointF p1, PointF p2, int id =-1)
        {
            RectangleF rt = GetRootStep().GetBounding(); //GetLastClipActionBounding();
            float x1 = rt.X + rt.Width * p1.X;
            float y1 = rt.Y + rt.Height * p1.Y;
            float x2 = rt.X + rt.Width * p2.X;
            float y2 = rt.Y + rt.Height * p2.Y;
            DoClip(new FoldingParam(new LineSegment2DF(new PointF(x1, y1), new PointF(x2, y2))), id);
        }

        public void DoClip(FoldingType type, float rate, int id = -1)
        {
            DoClip(new FoldingParam(type, rate),id);
        }

        public void DoClip(string str)
        {
            FoldingParam param = new FoldingParam();
            int id = -1;
            DoClip(param, id);
        }

        public void DoClip(FoldingParam param, int id = -1)
        {

            if (_clipActionList.Count > 0)
                DoFolding(_clipActionList.Last(), param, id);
            else
            {
                if (_prevStep != null && _prevStep.GetLast() != null)
                    DoFolding(_prevStep.GetLast(), param, id);
            }
        }


        public override ClipAction GetLast()
        {
            if (_clipActionList.Count > 0)
                return _clipActionList.Last();
            return null;
        }

        public override Bitmap Render()
        {
            try
            {
                OrigamiStep root = GetRootStep();
                RectangleF outRt = root.GetBounding();

                Bitmap bmp = new Bitmap(OrigamiStart._renderSz.Width, OrigamiStart._renderSz.Height);

                Graphics memDc = Graphics.FromImage(bmp);

                memDc.Clear(OrigamiSetting._bkClr);

                memDc.TranslateTransform(OrigamiStart._offset.X, OrigamiStart._offset.Y);
                Brush brush = new SolidBrush(OrigamiSetting._bkClr);
                Brush frontBrush = new SolidBrush(OrigamiSetting._paperFrontClr);
                Brush bkBrush = new SolidBrush(OrigamiSetting._paperBkClr);

                Pen pen1 = new Pen(OrigamiSetting._lineClr, OrigamiSetting._lineWidth);
                Pen dashedPen = new Pen(OrigamiSetting._lineClr, OrigamiSetting._lineWidth);
                dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                Pen bkPen = new Pen(OrigamiSetting._bkClr, OrigamiSetting._lineWidth);
                 foreach (ClipAction action in _clipActionList)
                {
                //    ClipAction action = _clipActionList.Last();
                    foreach (FoldingPolygon polygon in action._foldingPolyList)
                    {
                        if (polygon._oldPtList != null)
                        {
                            memDc.DrawPolygon(bkPen, polygon._oldPtList.ToArray());
                            memDc.DrawPolygon(dashedPen, polygon._oldPtList.ToArray());
                        }
                        if (action == _clipActionList.Last())
                        {
                            if (polygon.matrixList.Count % 2 == 0)
                                memDc.FillPolygon(frontBrush, polygon._ptList.ToArray());
                            else
                                memDc.FillPolygon(bkBrush, polygon._ptList.ToArray());
                        }
                        memDc.DrawPolygon(pen1, polygon._ptList.ToArray());
                    }

                }
                //画虚线分割线；
                foreach (ClipAction action in _clipActionList)
                {
               //     memDc.DrawLine(bkPen, action._param._segLine.P1, action._param._segLine.P2);
               //     memDc.DrawLine(dashedPen, action._param._segLine.P1, action._param._segLine.P2);
                }
                memDc.Flush();
                memDc.Dispose();
                 return bmp;     
            }
            catch
            { }
            return null;

        }
    }

    [Serializable]
    public class ClipAction
    {
        public List<FoldingPolygon> _foldingPolyList = new List<FoldingPolygon>();
        public FoldingParam _param = new FoldingParam();

        public List<PointF> GetVertexList()
        {
            List<PointF> ptList = new List<PointF>();
            foreach (FoldingPolygon polygon in _foldingPolyList)
            {
                ptList.AddRange(polygon._ptList);
            }
            return ptList;
        }

        public List<LineSegment2DF> GetLastEdgeList()
        {
            List<LineSegment2DF> edgeList = new List<LineSegment2DF>();
            PointF[] ptList = _foldingPolyList.Last()._ptList;
            for (int i = 0; i < ptList.Length; i++)
            {
                edgeList.Add(new LineSegment2DF(ptList[i], ptList[(i + 1) % ptList.Length]));
            }
            return edgeList;
        }
    }

    [Serializable]
    public class FoldingPolygon
    {
        public PointF[] _ptList = null;
        public PointF[] _oldPtList = null;
        public double[,] _matrix = GeometricUtils.GenIdentityMatrix();
        public List<double[,]> matrixList = new List<double[,]>();
        public double[,] GenMatrix()
        {
            return GeometricUtils.CombineMatrix(matrixList);
        }
    }


    [Serializable]
    public class FoldingParam
    {
        public LineSegment2DF _segLine = new LineSegment2DF();
        public FoldingType _type = FoldingType.Empty;
        public float _rate = 0.5f;

        public FoldingParam()
        {
 
        }

        public FoldingParam(FoldingType foldingType, float rate )
        {
            _type = foldingType;
            _rate = rate;
        }

        public FoldingParam(LineSegment2DF line)
        {
            _type = FoldingType.Angle;
            _segLine = line;
        }

        public FoldingParam(FoldingType foldingType, float rate, LineSegment2DF line)
        {
            _type = foldingType;
            _rate = rate;
            _segLine = line;
        }

    }

    public enum FoldingType
    {
        Hori_Left,
        Hori_Right,
        Vert_Top,
        Vert_Bottom,
        Angle,
        Empty
    }
}
