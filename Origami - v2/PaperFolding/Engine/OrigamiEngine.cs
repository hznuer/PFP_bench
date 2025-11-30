using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Xml;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;



namespace Origami
{
    public static class OrigamiEngine
    {
        public static Size _baseSz = new Size(360, 360);

        public static List<Image> RenderImageList(OrigamiStart startStep, bool includingAnswer = false)
        {
            List<Image> imgList = new List<Image>();
            OrigamiStep step = startStep;
            while (step != null)
            {
                imgList.Add(step.Render());
                step = step._nextStep;
            }
            if (includingAnswer)
                imgList.Add(startStep.GetLastStep().RenderAnswer());
            return imgList;
        }

        public static List<OrigamiStep> ParseFile(string xmlPath)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(xmlPath);
                XmlNodeList nodeList = xmlDoc.SelectNodes("/Origamis/Origami");
                List<OrigamiStep> stepList = new List<OrigamiStep>();
                foreach (XmlNode node in nodeList)
                {
                    OrigamiStart startStep = null;

                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (subNode.Name == "Start")
                        {
                            int id = GlobalUtils.IntParse(GetAttributeValue(subNode, "id"), -1);

                            List<PointF> ptList = OrigamiPaper.CreatePaper((PaperType)id);
                            if (ptList == null)
                                ptList = GlobalUtils.PointFListParse(GetAttributeValue(subNode, "Id"));

                            if (ptList == null || ptList.Count == 0)
                                ptList = OrigamiPaper.CreatePaper((PaperType)0);
                            startStep = new OrigamiStart(new Size(360, 360), OrigamiPaperType.UnRegular, ptList);
                            stepList.Add(startStep);

                        }
                        else
                        {
                            if (startStep == null)
                                continue;

                            if (subNode.Name == "SingleFold")
                            {
                                OrigamiFolding foldstep = new OrigamiFolding();
                                startStep.AppendLastStep(foldstep);
                                FoldingType type = (FoldingType)ToEnum<FoldingType>(GetAttributeValue(subNode, "Type"), FoldingType.Empty);
                                int id = GlobalUtils.IntParse(GetAttributeValue(subNode, "layerId"), -1);

                                try
                                {
                                    switch (type)
                                    {
                                        case FoldingType.Hori_Left:
                                        case FoldingType.Hori_Right:
                                        case FoldingType.Vert_Bottom:
                                        case FoldingType.Vert_Top:
                                            {
                                                float rate = GlobalUtils.FloatParse(GetAttributeValue(subNode, "Rate"));
                                                foldstep.DoClip(type, rate, id);
                                            }
                                            break;
                                        case FoldingType.Angle:
                                            {
                                                PointF p1 = GlobalUtils.PointFParse(GetAttributeValue(subNode, "P1"));
                                                PointF p2 = GlobalUtils.PointFParse(GetAttributeValue(subNode, "P2"));
                                                foldstep.DoClip(p1, p2, id);
                                            }
                                            break;

                                    }
                                }
                                catch
                                { }
                            }
                            else if (subNode.Name == "Folds")
                            {
                                OrigamiFolding foldstep = new OrigamiFolding();
                                startStep.AppendLastStep(foldstep);
                                foreach (XmlNode foldNode in subNode.ChildNodes)
                                {
                                    FoldingType type = (FoldingType)ToEnum<FoldingType>(GetAttributeValue(foldNode, "Type"), FoldingType.Empty);
                                    int id = GlobalUtils.IntParse(GetAttributeValue(foldNode, "layerId"), -1);

                                    try
                                    {
                                        switch (type)
                                        {
                                            case FoldingType.Hori_Left:
                                            case FoldingType.Hori_Right:
                                            case FoldingType.Vert_Bottom:
                                            case FoldingType.Vert_Top:
                                                {
                                                    float rate = GlobalUtils.FloatParse(GetAttributeValue(foldNode, "Rate"));
                                                    foldstep.DoClip(type, rate, id);
                                                }
                                                break;
                                            case FoldingType.Angle:
                                                {
                                                    PointF p1 = GlobalUtils.PointFParse(GetAttributeValue(foldNode, "P1"));
                                                    PointF p2 = GlobalUtils.PointFParse(GetAttributeValue(foldNode, "P2"));
                                                    foldstep.DoClip(p1, p2, id);
                                                }
                                                break;

                                        }
                                    }
                                    catch
                                    { }
                                }
                            }
                            if (subNode.Name == "Cut")
                            {
                                OrigamiCut cutStep = new OrigamiCut();
                                startStep.AppendLastStep(cutStep);
                                cutStep.RandAddObj();
                            }
                            if (subNode.Name == "Confuses")
                            {
                                if (startStep == null)
                                    continue;
                                foreach (XmlNode confuseNode in subNode.ChildNodes)
                                {
                                    if (confuseNode.Name == "Confuse")
                                    {
                                        OrigamiStart confuseStep = startStep.SimpleClone();
                                        startStep._confuseList.Add(confuseStep);
 

                                        foreach (XmlNode subFoldNode in confuseNode.ChildNodes)
                                        {
                                            if (subFoldNode.Name == "SingleFold")
                                            {
                                                OrigamiFolding foldstep = new OrigamiFolding();
                                                confuseStep.AppendLastStep(foldstep);
                                                FoldingType type = (FoldingType)ToEnum<FoldingType>(GetAttributeValue(subFoldNode, "Type"), FoldingType.Empty);
                                                int id = GlobalUtils.IntParse(GetAttributeValue(subFoldNode, "layerId"), -1);

                                                try
                                                {
                                                    switch (type)
                                                    {
                                                        case FoldingType.Hori_Left:
                                                        case FoldingType.Hori_Right:
                                                        case FoldingType.Vert_Bottom:
                                                        case FoldingType.Vert_Top:
                                                            {
                                                                float rate = GlobalUtils.FloatParse(GetAttributeValue(subFoldNode, "Rate"));
                                                                foldstep.DoClip(type, rate, id);
                                                            }
                                                            break;
                                                        case FoldingType.Angle:
                                                            {
                                                                PointF p1 = GlobalUtils.PointFParse(GetAttributeValue(subFoldNode, "P1"));
                                                                PointF p2 = GlobalUtils.PointFParse(GetAttributeValue(subFoldNode, "P2"));
                                                                foldstep.DoClip(p1, p2, id);
                                                            }
                                                            break;
                                                    }
                                                }
                                                catch
                                                { }
                                            }
                                            else if (subFoldNode.Name == "Folds")
                                            {
                                                OrigamiFolding foldstep = new OrigamiFolding();

                                                confuseStep.AppendLastStep(foldstep);
                                                foreach (XmlNode foldNode in subFoldNode.ChildNodes)
                                                {
                                                    FoldingType type = (FoldingType)ToEnum<FoldingType>(GetAttributeValue(foldNode, "Type"), FoldingType.Empty);
                                                    int id = GlobalUtils.IntParse(GetAttributeValue(foldNode, "layerId"), -1);

                                                    try
                                                    {
                                                        switch (type)
                                                        {
                                                            case FoldingType.Hori_Left:
                                                            case FoldingType.Hori_Right:
                                                            case FoldingType.Vert_Bottom:
                                                            case FoldingType.Vert_Top:
                                                                {
                                                                    float rate = GlobalUtils.FloatParse(GetAttributeValue(foldNode, "Rate"));
                                                                    foldstep.DoClip(type, rate, id);
                                                                }
                                                                break;
                                                            case FoldingType.Angle:
                                                                {
                                                                    PointF p1 = GlobalUtils.PointFParse(GetAttributeValue(foldNode, "P1"));
                                                                    PointF p2 = GlobalUtils.PointFParse(GetAttributeValue(foldNode, "P2"));
                                                                    foldstep.DoClip(p1, p2, id);
                                                                }
                                                                break;

                                                        }
                                                    }
                                                    catch
                                                    { }
                                                }
                                            }
                                        }
                                    }
                                }

                                //List<Image> imgList = startStep.GetConfuseImageList();
                                //int tmpId = 0;
                                //foreach (Image img in imgList)
                                //{
                                //    tmpId++;
                                //    img.Save("f:\\tmpData\\" + tmpId + ".bmp");
                                //}
    
                            }
                        }
                    }

                }
                return stepList;
            }
            catch (Exception ex)
            { }
            return new List<OrigamiStep>();

        }

        public static string GetAttributeValue(XmlNode node, string attributeName)
        {
            string value = null;
            if (node != null)
            {
                XmlAttribute attribute = node.Attributes[attributeName];
                if (attribute != null)
                {
                    value = attribute.Value;
                }
            }
            return value;
        }

        public static XmlAttribute CreateAttribute(XmlNode node, string attributeName, string value)
        {
            XmlDocument doc = node.OwnerDocument;
            XmlAttribute attr = null;
            // create new attribute
            attr = doc.CreateAttribute(attributeName);
            attr.Value = value;
            // link attribute to node
            node.Attributes.SetNamedItem(attr);
            return attr;
        }

        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            try
            {
                if (!string.IsNullOrEmpty(value))
                    return (T)Enum.Parse(typeof(T), value);
            }
            catch
            { }
            return defaultValue;
        }

        public static bool IsImgSSM(Bitmap answerBmp, List<Bitmap> candidateList, Bitmap bmp, int diffTh = 200)
        {
            try
            {

                if (GetImgSSMDiff(answerBmp, bmp) < diffTh)
                    return true;
                foreach(Bitmap candidate in candidateList)
                    if (GetImgSSMDiff(candidate, bmp) < diffTh)
                        return true;
                return false;
                
            }
            catch
            { }
            return true;
 
        }

        public static int GetImgSSMDiff(Bitmap bmp1, Bitmap bmp2)
        {
            try
            {
                int  differentCount = 0;
                {
                    // 确保两个图像尺寸相同
                    if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
                    {
                        Console.WriteLine("错误：两个图像尺寸不一致，无法比较。");
                        return 0;
                    }

                    // 统计相似和不相似的像素个数
                    int width = bmp1.Width;
                    int height = bmp1.Height;
                    int similarCount = 0;

                    // 使用LockBits方法锁定内存区域进行直接访问
                    BitmapData bmpData1 = bmp1.LockBits(new Rectangle(0, 0, width, height),
                                                       ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    BitmapData bmpData2 = bmp2.LockBits(new Rectangle(0, 0, width, height),
                                                       ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    try
                    {
                        // 启用unsafe模式
                        unsafe
                        {
                            // 获取图像数据指针
                            byte* ptr1 = (byte*)bmpData1.Scan0;
                            byte* ptr2 = (byte*)bmpData2.Scan0;

                            // 计算每行的字节数（考虑内存对齐）
                            int bytesPerLine = bmpData1.Stride;
                            int bytesPerPixel = 3; // 24位RGB格式，每个像素3字节

                            // 逐像素比较（使用指针操作）
                            for (int y = 0; y < height; y++)
                            {
                                byte* row1 = ptr1 + y * bytesPerLine;
                                byte* row2 = ptr2 + y * bytesPerLine;

                                for (int x = 0; x < width; x++)
                                {
                                    int idx = x * bytesPerPixel;

                                    // 比较RGB值（注意BGR顺序）
                                    if (row1[idx] == row2[idx] &&     // B
                                        row1[idx + 1] == row2[idx + 1] && // G
                                        row1[idx + 2] == row2[idx + 2])   // R
                                    {
                                        similarCount++;
                                    }
                                    else
                                    {
                                        differentCount++;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        // 解锁位图数据
                        bmp1.UnlockBits(bmpData1);
                        bmp2.UnlockBits(bmpData2);
                    }

                    return differentCount;

                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public static double CalImgSSM(Bitmap bmp1, Bitmap bmp2, ref int differentCount)
        {
            try
            {
                differentCount = 0;
                {
                    // 确保两个图像尺寸相同
                    if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
                    {
                        Console.WriteLine("错误：两个图像尺寸不一致，无法比较。");
                        return 0;
                    }

                    // 统计相似和不相似的像素个数
                    int width = bmp1.Width;
                    int height = bmp1.Height;
                    int similarCount = 0;

                    // 使用LockBits方法锁定内存区域进行直接访问
                    BitmapData bmpData1 = bmp1.LockBits(new Rectangle(0, 0, width, height), 
                                                       ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    BitmapData bmpData2 = bmp2.LockBits(new Rectangle(0, 0, width, height), 
                                                       ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    try
                    {
                        // 启用unsafe模式
                        unsafe
                        {
                            // 获取图像数据指针
                            byte* ptr1 = (byte*)bmpData1.Scan0;
                            byte* ptr2 = (byte*)bmpData2.Scan0;

                            // 计算每行的字节数（考虑内存对齐）
                            int bytesPerLine = bmpData1.Stride;
                            int bytesPerPixel = 3; // 24位RGB格式，每个像素3字节

                            // 逐像素比较（使用指针操作）
                            for (int y = 0; y < height; y++)
                            {
                                byte* row1 = ptr1 + y * bytesPerLine;
                                byte* row2 = ptr2 + y * bytesPerLine;
                                
                                for (int x = 0; x < width; x++)
                                {
                                    int idx = x * bytesPerPixel;
                                    
                                    // 比较RGB值（注意BGR顺序）
                                    if (row1[idx]     == row2[idx] &&     // B
                                        row1[idx + 1] == row2[idx + 1] && // G
                                        row1[idx + 2] == row2[idx + 2])   // R
                                    {
                                        similarCount++;
                                    }
                                    else
                                    {
                                        differentCount++;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        // 解锁位图数据
                        bmp1.UnlockBits(bmpData1);
                        bmp2.UnlockBits(bmpData2);
                    }

                    return (double)(similarCount)/(bmp1.Width*bmp1.Height);

                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static List<PointF> CalculateIntersections(RectangleF rect, PointF percentPoint, float angleDegree)
        {
            // 将百分比坐标转换为实际坐标
            float x = rect.Left + percentPoint.X * rect.Width;
            float y = rect.Top + percentPoint.Y * rect.Height;
            PointF startPoint = new PointF(x, y);

            // 将角度转换为弧度
            float angleRadian = angleDegree * (float)Math.PI / 180;

            // 计算直线的方向向量
            float dx = (float)Math.Cos(angleRadian);
            float dy = (float)Math.Sin(angleRadian);

            // 存储所有交点
            List<PointF> intersections = new List<PointF>();

            // 计算与四条边的交点
            AddEdgeIntersection(rect.Left, rect.Top, rect.Right, rect.Top, startPoint, dx, dy, intersections);
            AddEdgeIntersection(rect.Right, rect.Top, rect.Right, rect.Bottom, startPoint, dx, dy, intersections);
            AddEdgeIntersection(rect.Right, rect.Bottom, rect.Left, rect.Bottom, startPoint, dx, dy, intersections);
            AddEdgeIntersection(rect.Left, rect.Bottom, rect.Left, rect.Top, startPoint, dx, dy, intersections);

            // 去重处理（由于浮点数精度问题，可能会有非常接近的点）
            return RemoveDuplicates(intersections);
        }

        private static void AddEdgeIntersection(float x1, float y1, float x2, float y2,
                                               PointF startPoint, float dx, float dy,
                                               List<PointF> intersections)
        {
            // 线段向量
            float sx = x2 - x1;
            float sy = y2 - y1;

            // 计算行列式
            float det = dx * sy - dy * sx;

            // 如果行列式为0，表示平行或共线
            if (Math.Abs(det) < 1e-6f)
                return;

            // 计算参数t和u
            float t = ((startPoint.X - x1) * sy - (startPoint.Y - y1) * sx) / det;
            float u = ((startPoint.X - x1) * dy - (startPoint.Y - y1) * dx) / det;

            // 检查参数范围（u在线段上，t在直线上的任意位置）
            if (u >= 0 && u <= 1)
            {
                PointF intersection = new PointF(startPoint.X + t * dx, startPoint.Y + t * dy);
                intersections.Add(intersection);
            }
        }

        private static List<PointF> RemoveDuplicates(List<PointF> points)
        {
            const float tolerance = 1e-4f;
            List<PointF> result = new List<PointF>();

            foreach (var point in points)
            {
                bool isDuplicate = false;
                foreach (var existing in result)
                {
                    if (Math.Abs(point.X - existing.X) < tolerance &&
                        Math.Abs(point.Y - existing.Y) < tolerance)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                    result.Add(point);
            }

            return result;
        }
    }
}
