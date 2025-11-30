using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Emgu.CV;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;

namespace Origami
{
    public class GeometricUtils
    {
        const double Tolerance = 1e-7;

        public static Point[] ToPointList(PointF[] ptList)
        {
            Point[] ret = new Point[ptList.Length];
            for (int i = 0; i < ptList.Length; i++)
                ret[i] = new Point((int)ptList[i].X, (int)ptList[i].Y);

            return ret;
        }
        
        public static List<PointF> DoClip(List<PointF> ptList, LineSegment2DF segLine)
        {
            if (ptList.Count == 0)
                return new List<PointF>();
            List<PointF> retList = new List<PointF>();
            int num = ptList.Count;
            for (int i = 0; i < num; i++)
            {
                bool flag1 = IsInside(segLine, ptList[i]);
                bool flag2 = IsInside(segLine, ptList[(i + 1) % num]);

                if (flag1 == false && flag2 == false)
                    continue;
                else if (flag1 && flag2)
                    retList.Add(ptList[(i + 1) % num]);
                else
                {
                    PointF interPt = CalInterPt(ptList[i], ptList[(i + 1) % num], segLine.P1, segLine.P2);
                    if (flag1 && (!flag2))
                    {
                        retList.Add(interPt);
                    }
                    else
                    {
                        if (PointFLength(interPt, ptList[(i + 1) % num]) > 1e-4)
                        {
                            retList.Add(interPt);
                            retList.Add(ptList[(i + 1) % num]);
                        }
                        else
                            retList.Add(ptList[(i + 1) % num]);   
                    }
                }
            }

            while (retList.Count>3)
            {
                num = retList.Count();
                bool isSamePt = false;
                for (int i = 0; i < num; i++)
                {
                    if (PointFLength(retList[i], retList[(i + 1) % num]) <1e-4)
                    {
                        retList.RemoveAt(i);
                        isSamePt = true;
                        break;
                    }
 
                }
                if (!isSamePt)
                    break;
            }

            return retList;
        }

        public static List<PointF> DoClip(List<PointF> ptList, LineSegment2DF segLine, out List<PointF> outList)
        {
            outList = new List<PointF>();
            if (ptList.Count == 0)
                return new List<PointF>();
            List<PointF> retList = new List<PointF>();
            int num = ptList.Count;
            for (int i = 0; i < num; i++)
            {
                bool flag1 = IsInside(segLine, ptList[i]);
                bool flag2 = IsInside(segLine, ptList[(i + 1) % num]);

                if (flag1 == false && flag2 == false)
                {
                    outList.Add(ptList[(i + 1) % num]);
                }
                else if (flag1 && flag2)
                    retList.Add(ptList[(i + 1) % num]);
                else
                {
                    PointF interPt = CalInterPt(ptList[i], ptList[(i + 1) % num], segLine.P1, segLine.P2);
                    if (flag1 && (!flag2))
                    {
                        retList.Add(interPt);
                        outList.Add(interPt);
                        outList.Add(ptList[(i + 1) % num]);
                    }
                    else
                    {
                        retList.Add(interPt);
                        retList.Add(ptList[(i + 1) % num]);
                        outList.Add(ptList[(i + 1) % num]);
                    }
                }
            }
            return retList;
        }


        public static bool IsInside(LineSegment2DF line, PointF pt)
        {
            PointF pt1 = line.P1;
            PointF pt2 = line.P2;
            double[] lineParam = CalLineEquation(pt1, pt2);


            if (IsPointOnLine(pt1, pt2, pt))
                return true;
            return (pt.X * lineParam[0] + pt.Y * lineParam[1] + lineParam[2]) >= 0;
        }

        public static double PointFLength(PointF p1, PointF p2)
        {
            float x = p1.X - p2.X;
            float y = p1.Y - p2.Y;
            return Math.Sqrt(x * x + y * y);
        }

        public static bool IsPointOnLine(PointF pt1, PointF pt2, PointF pt)
        {
            // 计算向量 pt1pt 和向量 pt1pt2 的叉积
            double crossProduct = (pt.X - pt1.X) * (pt2.Y - pt1.Y) - (pt.Y - pt1.Y) * (pt2.X - pt1.X);

            // 判断叉积是否在误差范围内为 0
            return Math.Abs(crossProduct) < Tolerance;
        }

        // 生成单位矩阵
        public static double[,] GenIdentityMatrix(int size=3)
        {
            double[,] identityMatrix = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                    {
                        identityMatrix[i, j] = 1;
                    }
                    else
                    {
                        identityMatrix[i, j] = 0;
                    }
                }
            }
            return identityMatrix;
        }

        public static double[,] CombineMatrix(List<double[,]> matrixList)
        {
            double[,] matrix = GeometricUtils.GenIdentityMatrix();

            if (matrixList!=null&&matrixList.Count > 0)
            {
              //  for (int i = matrixList.Count - 1; i >= 0; i--)
                    for (int i = 0; i <matrixList.Count; i++)
                {
                    matrix = GeometricUtils.MulMatrices( matrixList[i], matrix);
                }
            }
            return matrix;

        }

        public static double[] CalLineEquation(PointF pt1, PointF pt2)
        {
            double a = pt2.Y - pt1.Y;
            double b = pt1.X - pt2.X;
            double c = pt2.X * pt1.Y - pt1.X * pt2.Y;
            return new double[] { a, b, c };
        }

        public static List<PointF> ReflectionPoly(List<PointF> outList, double[,] matrix)
        {
            List<PointF> retList = new List<PointF>();

            foreach (PointF pt in outList)
                retList.Add(ApplyTransformation(matrix, pt));

            return retList;
 
        }

        // 计算两条直线的交点
        public static PointF CalInterPt(PointF pt1, PointF pt2, PointF p1, PointF p2)
        {
            // 计算两条直线的一般式方程系数
            double[] param1 = CalLineEquation(pt1, pt2);
            double[] param2 = CalLineEquation(p1, p2);
            double a1 = param1[0], b1 = param1[1], c1 = param1[2];
            double a2 = param2[0], b2 = param2[1], c2 = param2[2];
            // 计算分母
            double denominator = a1 * b2 - a2 * b1;

            // 判断两条直线是否平行
            if (Math.Abs(denominator) < Tolerance)
            {
                return new PointF(); // 两条直线平行，无交点
            }

            // 计算交点的 x 和 y 坐标
            double x = (b1 * c2 - b2 * c1) / denominator;
            double y = (a2 * c1 - a1 * c2) / denominator;
            return new PointF((float)x, (float)y);
        }

        public static double[,] CalReflectionMatrix(PointF pt1, PointF pt2)
        {
            return CalReflectionMatrix(CalLineEquation(pt1, pt2));
        }

        //static double[,] CalReflectionMatrix(double[] lineParam)
        //{
        //    double a = lineParam[0], b = lineParam[1], c = lineParam[2];
        //    // 平移直线使其经过原点
        //    double[,] translation;
        //    if (b != 0)
        //    {
        //        translation = new double[3, 3] {
        //            { 1, 0, 0 },
        //            { 0, 1, -c / b },
        //            { 0, 0, 1 }
        //        };
        //    }
        //    else if (a != 0)
        //    {
        //        translation = new double[3, 3] {
        //            { 1, 0, -c / a },
        //            { 0, 1, 0 },
        //            { 0, 0, 1 }
        //        };
        //    }
        //    else
        //    {
        //        throw new ArgumentException("直线方程无效，a 和 b 不能同时为 0。");
        //    }

        //    // 计算直线的角度
        //    double angle = Math.Atan2(-a, b);

        //    // 旋转直线使其与 x 轴重合
        //    double cosAngle = Math.Cos(-angle);
        //    double sinAngle = Math.Sin(-angle);
        //    double[,] rotation = {
        //        { cosAngle, -sinAngle, 0 },
        //        { sinAngle, cosAngle, 0 },
        //        { 0, 0, 1 }
        //    };

        //    // 关于 x 轴反射
        //    double[,] reflection = {
        //        { 1, 0, 0 },
        //        { 0, -1, 0 },
        //        { 0, 0, 1 }
        //    };

        //    // 反向旋转
        //    cosAngle = Math.Cos(angle);
        //    sinAngle = Math.Sin(angle);
        //    double[,] inverseRotation = {
        //        { cosAngle, -sinAngle, 0 },
        //        { sinAngle, cosAngle, 0 },
        //        { 0, 0, 1 }
        //    };

        //    // 反向平移
        //    double[,] inverseTranslation = new double[3, 3];
        //    for (int i = 0; i < 3; i++)
        //    {
        //        for (int j = 0; j < 3; j++)
        //        {
        //            if (i == j)
        //            {
        //                inverseTranslation[i, j] = 1;
        //            }
        //            else if (i == 0 && j == 2)
        //            {
        //                inverseTranslation[i, j] = -translation[i, j];
        //            }
        //            else if (i == 1 && j == 2)
        //            {
        //                inverseTranslation[i, j] = -translation[i, j];
        //            }
        //            else
        //            {
        //                inverseTranslation[i, j] = 0;
        //            }
        //        }
        //    }

        //    // 组合变换矩阵
        //    double[,] combinedMatrix = MulMatrices(translation, rotation);
        //    combinedMatrix = MulMatrices(combinedMatrix, reflection);
        //    combinedMatrix = MulMatrices(combinedMatrix, inverseRotation);
        //    combinedMatrix = MulMatrices(combinedMatrix, inverseTranslation);

        //    return combinedMatrix;
        //}

        public static double[,] CalReflectionMatrix(double[] lineParam)
        {
            double a = lineParam[0], b = lineParam[1], c = lineParam[2];
            double denominator = a * a + b * b;
            double[,] matrix = new double[3, 3];
            matrix[0, 0] = (b * b - a * a) / denominator;
            matrix[0, 1] = -2 * a * b / denominator;
            matrix[0, 2] = -2 * a * c / denominator;
            matrix[1, 0] = -2 * a * b / denominator;
            matrix[1, 1] = (a * a - b * b) / denominator;
            matrix[1, 2] = -2 * b * c / denominator;
            matrix[2, 0] = 0;
            matrix[2, 1] = 0;
            matrix[2, 2] = 1;
            return matrix;
        }

        public static double[,] MulMatrices(double[,] matrix1, double[,] matrix2)
        {
            int rows1 = matrix1.GetLength(0);
            int cols1 = matrix1.GetLength(1);
            int cols2 = matrix2.GetLength(1);
            double[,] result = new double[rows1, cols2];

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols2; j++)
                {
                    for (int k = 0; k < cols1; k++)
                    {
                        result[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }
            return result;
        }

        public static double[,] CalculateRotationMatrix(int centerX, int centerY, double angle)
        {
            // 平移到原点
            double[,] translationToOrigin = {
            { 1, 0, -centerX },
            { 0, 1, -centerY },
            { 0, 0, 1 }
            };

            // 角度转换为弧度
            double radian = angle * Math.PI / 180;
            double cosAngle = Math.Cos(radian);
            double sinAngle = Math.Sin(radian);

            // 旋转矩阵
            double[,] rotation = {
            { cosAngle, -sinAngle, 0 },
            { sinAngle, cosAngle, 0 },
            { 0, 0, 1 }
        };

            // 反向平移回原来的位置
            double[,] translationBack = {
            { 1, 0, centerX },
            { 0, 1, centerY },
            { 0, 0, 1 }
        };

            // 组合矩阵
            double[,] combinedMatrix = MulMatrices(translationToOrigin, rotation);
            combinedMatrix = MulMatrices(combinedMatrix, translationBack);

            return combinedMatrix;
        }

        public static PointF ApplyTransformation(double[,] matrix, PointF point)
        {
            double x = matrix[0, 0] * point.X + matrix[0, 1] * point.Y + matrix[0, 2];
            double y = matrix[1, 0] * point.X + matrix[1, 1] * point.Y + matrix[1, 2];
            return new PointF((float)x, (float)y);
        }

        public static Matrix InverseMatrix(double[,] matrixArray)
        {
            Matrix matrix = new Matrix(
                (float)matrixArray[0, 0], (float)matrixArray[0, 1],
                (float)matrixArray[1, 0], (float)matrixArray[1, 1],
                (float)matrixArray[0, 2], (float)matrixArray[1, 2]
            );

            // 对矩阵求逆
            Matrix inverseMatrix = (Matrix)matrix.Clone();
            try
            {
                inverseMatrix.Invert();

                //return new Matrix(
                //0, -1,
                //1, 0,
                //360, 0
                //);

            }
            catch (Exception ex )
            {

            }
            return inverseMatrix;
        }
    }
}
