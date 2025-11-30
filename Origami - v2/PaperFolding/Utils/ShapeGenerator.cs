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
   
        /// <summary>
        /// 折纸形状生成器，提供各种多边形点集的生成方法
        /// </summary>
        public static class ShapeGenerator
        {
            private const float CenterX = 180f;
            private const float CenterY = 180f;
            private const float MaxDimension = 360f;

            /// <summary>
            /// 生成正方形的点集
            /// </summary>
            /// <param name="size">正方形的大小（边长）</param>
            /// <returns>正方形的顶点集合</returns>
            public static List<PointF> CreateSquare(float size)
            {
                ValidateSize(size);
                float halfSize = size / 2;

                return new List<PointF>
            {
                new PointF(CenterX - halfSize, CenterY - halfSize), // 左上角
                new PointF(CenterX + halfSize, CenterY - halfSize), // 右上角
                new PointF(CenterX + halfSize, CenterY + halfSize), // 右下角
                new PointF(CenterX - halfSize, CenterY + halfSize)  // 左下角
            };
            }

            /// <summary>
            /// 生成正六边形的点集
            /// </summary>
            /// <param name="radius">六边形的外接圆半径</param>
            /// <returns>正六边形的顶点集合</returns>
            public static List<PointF> CreateHexagon(float radius)
            {
                ValidateSize(radius * 2);
                return CreateRegularPolygon(6, radius);
            }

            /// <summary>
            /// 生成正八边形的点集
            /// </summary>
            /// <param name="radius">八边形的外接圆半径</param>
            /// <returns>正八边形的顶点集合</returns>
            public static List<PointF> CreateOctagon(float radius)
            {
                ValidateSize(radius * 2);
                return CreateRegularPolygon(8, radius);
            }

            /// <summary>
            /// 生成梯形的点集
            /// </summary>
            /// <param name="topWidth">上底宽度</param>
            /// <param name="bottomWidth">下底宽度</param>
            /// <param name="height">高度</param>
            /// <returns>梯形的顶点集合</returns>
            public static List<PointF> CreateTrapezoid(float topWidth, float bottomWidth, float height)
            {
                ValidateSize(Math.Max(topWidth, bottomWidth), height);

                float halfTop = topWidth / 2;
                float halfBottom = bottomWidth / 2;
                float halfHeight = height / 2;

                return new List<PointF>
            {
                new PointF(CenterX - halfTop, CenterY - halfHeight), // 左上
                new PointF(CenterX + halfTop, CenterY - halfHeight), // 右上
                new PointF(CenterX + halfBottom, CenterY + halfHeight), // 右下
                new PointF(CenterX - halfBottom, CenterY + halfHeight)  // 左下
            };
            }

            /// <summary>
            /// 生成圆形的点集（使用多边形近似）
            /// </summary>
            /// <param name="radius">圆的半径</param>
            /// <param name="segments">分段数，越多越接近圆形</param>
            /// <returns>圆形的近似多边形顶点集合</returns>
            public static List<PointF> CreateCircle(float radius, int segments = 36)
            {
                ValidateSize(radius * 2);
                return CreateRegularPolygon(segments, radius);
            }

            /// <summary>
            /// 生成长方形的点集
            /// </summary>
            /// <param name="width">宽度</param>
            /// <param name="height">高度</param>
            /// <returns>长方形的顶点集合</returns>
            public static List<PointF> CreateRectangle(float width, float height)
            {
                ValidateSize(width, height);

                float halfWidth = width / 2;
                float halfHeight = height / 2;

                return new List<PointF>
            {
                new PointF(CenterX - halfWidth, CenterY - halfHeight), // 左上
                new PointF(CenterX + halfWidth, CenterY - halfHeight), // 右上
                new PointF(CenterX + halfWidth, CenterY + halfHeight), // 右下
                new PointF(CenterX - halfWidth, CenterY + halfHeight)  // 左下
            };
            }

            /// <summary>
            /// 生成三角形的点集
            /// </summary>
            /// <param name="size">三角形的大小（外接圆半径）</param>
            /// <returns>三角形的顶点集合</returns>
            public static List<PointF> CreateTriangle(float size)
            {
                ValidateSize(size * 2);
                return CreateRegularPolygon(3, size);
            }

            /// <summary>
            /// 生成钻石形（菱形）的点集
            /// </summary>
            /// <param name="width">宽度</param>
            /// <param name="height">高度</param>
            /// <returns>钻石形的顶点集合</returns>
            public static List<PointF> CreateDiamond(float width, float height)
            {
                ValidateSize(width, height);

                float halfWidth = width / 2;
                float halfHeight = height / 2;

                return new List<PointF>
            {
                new PointF(CenterX, CenterY - halfHeight),       // 顶部
                new PointF(CenterX + halfWidth, CenterY),        // 右侧
                new PointF(CenterX, CenterY + halfHeight),       // 底部
                new PointF(CenterX - halfWidth, CenterY)         // 左侧
            };
            }

            /// <summary>
            /// 生成平行四边形的点集
            /// </summary>
            /// <param name="width">宽度</param>
            /// <param name="height">高度</param>
            /// <param name="skew">倾斜量</param>
            /// <returns>平行四边形的顶点集合</returns>
            public static List<PointF> CreateParallelogram(float width, float height, float skew)
            {
                ValidateSize(width + Math.Abs(skew), height);

                float halfWidth = width / 2;
                float halfHeight = height / 2;

                return new List<PointF>
            {
                new PointF(CenterX - halfWidth - skew, CenterY - halfHeight), // 左上
                new PointF(CenterX + halfWidth - skew, CenterY - halfHeight), // 右上
                new PointF(CenterX + halfWidth, CenterY + halfHeight),        // 右下
                new PointF(CenterX - halfWidth, CenterY + halfHeight)         // 左下
            };
            }

            /// <summary>
            /// 生成五边形的点集
            /// </summary>
            /// <param name="radius">五边形的外接圆半径</param>
            /// <returns>五边形的顶点集合</returns>
            public static List<PointF> CreatePentagon(float radius)
            {
                ValidateSize(radius * 2);
                return CreateRegularPolygon(5, radius);
            }

            /// <summary>
            /// 生成不规则七边形的点集
            /// </summary>
            /// <param name="radius">七边形的大致半径</param>
            /// <param name="irregularity">不规则度 (0-1)</param>
            /// <returns>不规则七边形的顶点集合</returns>
            public static List<PointF> CreateIrregularHeptagon(float radius, float irregularity = 0.3f)
            {
                ValidateSize(radius * 2);
                return CreateIrregularPolygon(7, radius, irregularity);
            }

            /// <summary>
            /// 生成不规则星形的点集
            /// </summary>
            /// <param name="points">星角数量</param>
            /// <param name="outerRadius">外圆半径</param>
            /// <param name="innerRadius">内圆半径</param>
            /// <returns>不规则星形的顶点集合</returns>
            public static List<PointF> CreateStar(int points, float outerRadius, float innerRadius)
            {
                if (points < 3) throw new ArgumentException("星角数量必须至少为3");//, nameof(points));
                ValidateSize(Math.Max(outerRadius, innerRadius) * 2);

                List<PointF> result = new List<PointF>();
                float angleStep = (float)(Math.PI * 2 / points);

                for (int i = 0; i < points * 2; i++)
                {
                    // 交替使用外圆和内圆半径
                    float radius = i % 2 == 0 ? outerRadius : innerRadius;
                    float angle = (float)(i * angleStep / 2 - Math.PI / 2); // 从顶部开始

                    float x = CenterX + radius * (float)Math.Cos(angle);
                    float y = CenterY + radius * (float)Math.Sin(angle);

                    result.Add(new PointF(x, y));
                }

                return result;
            }

            /// <summary>
            /// 创建一个房子形状（三角形屋顶加矩形主体）
            /// </summary>
            /// <param name="width">房子宽度</param>
            /// <param name="height">房子高度（不包括屋顶）</param>
            /// <param name="roofHeight">屋顶高度</param>
            /// <returns>房子形状的顶点集合</returns>
            public static List<PointF> CreateHouse(float width, float height, float roofHeight)
            {
                ValidateSize(width, height + roofHeight);

                float halfWidth = width / 2;
                float halfHeight = height / 2;
                float roofTop = CenterY - halfHeight - roofHeight;

                return new List<PointF>
            {
                // 屋顶
                new PointF(CenterX, roofTop),
           //     new PointF(CenterX + halfWidth, CenterY - halfHeight),
           //     new PointF(CenterX - halfWidth, CenterY - halfHeight),
         //       new PointF(CenterX, roofTop),
                
                // 房子主体
                new PointF(CenterX - halfWidth, CenterY - halfHeight),
                new PointF(CenterX - halfWidth, CenterY + halfHeight),
                new PointF(CenterX + halfWidth, CenterY + halfHeight),
                new PointF(CenterX + halfWidth, CenterY - halfHeight)
            };
            }

            // 辅助方法：创建正多边形
            private static List<PointF> CreateRegularPolygon(int sides, float radius)
            {
                List<PointF> points = new List<PointF>();
                float angleStep = (float)(Math.PI * 2 / sides);

                for (int i = 0; i < sides; i++)
                {
                    float angle = (float)(i * angleStep - Math.PI / 2); // 从顶部开始
                    float x = CenterX + radius * (float)Math.Cos(angle);
                    float y = CenterY + radius * (float)Math.Sin(angle);
                    points.Add(new PointF(x, y));
                }

                return points;
            }

            // 辅助方法：创建不规则多边形
            private static List<PointF> CreateIrregularPolygon(int sides, float radius, float irregularity)
            {
                Random random = new Random(42); // 使用固定种子确保结果可重现
                List<PointF> points = new List<PointF>();
                float angleStep = (float)(Math.PI * 2 / sides);

                for (int i = 0; i < sides; i++)
                {
                    // 添加一些随机性
                    float angleOffset = (float)(random.NextDouble() * 2 - 1) * irregularity * angleStep / 2;
                    float angle = (float)(i * angleStep + angleOffset - Math.PI / 2);

                    // 半径也添加一些随机性
                    float randomRadius = radius * (1 + (float)(random.NextDouble() * 2 - 1) * irregularity / 2);

                    float x = CenterX + randomRadius * (float)Math.Cos(angle);
                    float y = CenterY + randomRadius * (float)Math.Sin(angle);
                    points.Add(new PointF(x, y));
                }

                return points;
            }

            // 验证尺寸是否在允许范围内
            private static void ValidateSize(float width, float height = float.NaN)
            {
                if (float.IsNaN(height)) height = width;

                if (width > MaxDimension || height > MaxDimension)
                {
                    throw new ArgumentException("尺寸超出允许范围");//（最大 {MaxDimension}x{MaxDimension}）");
                }

                if (width <= 0 || height <= 0)
                {
                    throw new ArgumentException("尺寸必须为正数");
                }
            }
        }

}
