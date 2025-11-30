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
    public class OrigamiStart : OrigamiStep
    {
        public FoldingPolygon _foldingPolygon = new FoldingPolygon();
        public List<PointF> _polyList = new List<PointF>();
        public ClipAction _startAction = null;

        public OrigamiPaperType _type = OrigamiPaperType.Rectangle;

        public static Size _defaultSz = new Size(360,360);

        public static PointF _offset = new PointF();
        public static Size _renderSz = new Size(400, 400);

        public List<OrigamiStep> _confuseList = new List<OrigamiStep>();

        //待完成
        public OrigamiStart(List<PointF> ptList = null)
        { 
            
        }

        public OrigamiStart SimpleClone()
        {
            return new OrigamiStart(new Size(360, 360), OrigamiPaperType.UnRegular, new List<PointF>(_polyList));
        }
        


        public OrigamiStart(Size sz, OrigamiPaperType type = OrigamiPaperType.Rectangle, List<PointF> ptList = null)
        {
            _polyList = new List<PointF>();
            _type = type;
            switch (type)
            {
                case OrigamiPaperType.Rectangle:
                    {
                        _polyList.Add(new PointF());
                        _polyList.Add(new PointF(sz.Width, 0));
                        _polyList.Add(new PointF(sz.Width, sz.Height));
                        _polyList.Add(new PointF(0, sz.Height));
                    }
                    break;
                case OrigamiPaperType.Ellipse:
                    {
                        _polyList.Add(new PointF());
                        _polyList.Add(new PointF(sz.Width, 0));
                        _polyList.Add(new PointF(sz.Width, sz.Height));
                        _polyList.Add(new PointF(0, sz.Height));
                    }
                    break;
                case OrigamiPaperType.UnRegular:
                    _polyList = ptList;
                    break;
            }

            _foldingPolygon._ptList = _polyList.ToArray();
            _startAction = new ClipAction();
            _startAction._param = new FoldingParam();
            _startAction._foldingPolyList.Add(_foldingPolygon);
            RectangleF box = GetBounding();
            float maxSz = Math.Max(box.Width, box.Height);

            _offset = new PointF(maxSz*0.6f - box.Width/2f, maxSz*0.6f - box.Height/2f);
            if(box.Height != 280)
                _offset = new PointF(maxSz * 0.1f , maxSz * 0.1f);
            _renderSz.Width = (int)(maxSz * 1.2f + 0.5);
            _renderSz.Height = _renderSz.Width;
        }

        public List<Image> GetConfuseImageList()
        {
            try
            {
                if (_confuseList != null)
                {
                    List<Image> imgList = new List<Image>();
                    foreach (OrigamiStep step in _confuseList)
                        imgList.Add(step.GetLastStep().Render());
                    return imgList;
                }
            }
            catch
            { }
            return new List<Image>();
        }

        public override ClipAction GetLast()
        {
            return _startAction;
        }

        public override RectangleF GetBounding()
        {

            if (_polyList.Count == 0)
                return RectangleF.Empty;

            float minX = _polyList[0].X;
            float minY = _polyList[0].Y;
            float maxX = _polyList[0].X;
            float maxY = _polyList[0].Y;

            foreach (PointF point in _polyList)
            {
                minX = Math.Min(minX, point.X);
                maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
            }
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public override Bitmap Render()
        {
            try
            {
                //RectangleF rt = GetBounding();
                //int w = (int)( rt.Width)+OrigamiSetting._margin*2;
                //int h = (int)(rt.Height)+OrigamiSetting._margin*2;
                Bitmap bmp = new Bitmap(_renderSz.Width, _renderSz.Height);

                Graphics memDc = Graphics.FromImage(bmp);

                memDc.Clear(OrigamiSetting._bkClr);
                //memDc.TranslateTransform(OrigamiSetting._margin, OrigamiSetting._margin);
                memDc.TranslateTransform(_offset.X, _offset.Y);
                Brush frontBrush = new SolidBrush(OrigamiSetting._paperFrontClr); 

                switch (_type)
                {
                    case OrigamiPaperType.Rectangle:
                        {
                            using(Pen pen = new Pen( OrigamiSetting._lineClr, OrigamiSetting._lineWidth))
                            {

                                memDc.FillPolygon(frontBrush, _polyList.ToArray());
                                memDc.DrawPolygon(pen, _polyList.ToArray());                         
                            }
                        }
                        break;
                    case OrigamiPaperType.Ellipse:

                        break;
                    case OrigamiPaperType.UnRegular:
                        {
                            using(Pen pen = new Pen( OrigamiSetting._lineClr, OrigamiSetting._lineWidth))
                            {

                                memDc.FillPolygon(frontBrush, _polyList.ToArray());
                                memDc.DrawPolygon(pen, _polyList.ToArray());                         
                            }
                        }
                        break;
                }
 
                memDc.Flush();
                memDc.Dispose();
                //bmp.Save("F:\\tmpData\\1.bmp");

                return bmp;
            }
            catch
            { }
            return null;


        }

        public int GetFoldingCount()
        {
            int num = 0;
            OrigamiStep step = _nextStep;
            while (step != null)
            {
                if (step is OrigamiFolding)
                    num++;
                step = step._nextStep;
            }
            if (num > 3)
            {
                int err = 0;
            }
            return num;
        }

        public Region CreatePaperRegion()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(_polyList.ToArray());
            Region region =  new Region(path);
            return region;
        }
    }

    public enum OrigamiPaperType
    {
        Rectangle,
        Ellipse,
        UnRegular,
    }
}
