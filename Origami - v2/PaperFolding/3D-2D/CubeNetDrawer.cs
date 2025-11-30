using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D; // 添加这一行
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing.Imaging; // 用于 BitmapData 和 PixelFormat
using System.Runtime.InteropServices; // 用于 Marshal 类

namespace Origami
{
    public class CubeNetDrawer
    {
        
        private static readonly Random _rnd = new Random();
        // 绘制单个展开图
        public static Bitmap DrawCubeNet(CubeNet net, int squareSize = 100, bool drawName= true)
        {
            int rows = net.Layout.GetLength(0);
            int cols = net.Layout.GetLength(1);

            // 创建图像
            Bitmap bitmap = new Bitmap(cols * squareSize + 20, rows * squareSize + 20);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                // 绘制每个正方形
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        int faceValue = net.Layout[i, j];
                        if (faceValue == 0) continue;

                        Rectangle rect = new Rectangle(
                            j * squareSize + 10,
                            i * squareSize + 10,
                            squareSize, squareSize);

                        // 绘制正方形边框
                        g.DrawRectangle(Pens.Black, rect);

                        // 填充正方形
                        using (Brush brush = new SolidBrush(Color.FromArgb(100, 200, 200, 255)))
                        {
                            g.FillRectangle(brush, rect);
                        }

                        // 在正方形内绘制编号
                        using (Font font = new Font("Arial", 16))
                        {
                            g.DrawString(faceValue.ToString(), font, Brushes.Black,
                                rect.X + squareSize / 2 - 8,
                                rect.Y + squareSize / 2 - 10);
                        }
                    }
                }

                // 添加标题
                if(drawName)
                using (Font font = new Font("Arial", 14, FontStyle.Bold))
                {
                    g.DrawString(net.Name, font, Brushes.Black, 10, 10);
                }
            }

            return bitmap;
        }

        // 其他方法保持不变...
        // 绘制单个展开图，支持传入6个图片并显示在对应方格中
        public static Bitmap DrawCubeNet(CubeNet net, Bitmap[] images, int squareSize = 100, bool drawName = true)
        {
            int rows = net.Layout.GetLength(0);
            int cols = net.Layout.GetLength(1);

            // 创建图像
            Bitmap bitmap = new Bitmap(cols * squareSize + 20, rows * squareSize + 20);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                // 绘制每个正方形
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        int faceValue = net.Layout[i, j];
                        if (faceValue == 0) continue;

                        Rectangle rect = new Rectangle(
                            j * squareSize + 10,
                            i * squareSize + 10,
                            squareSize, squareSize);

                        // 计算图片显示区域（内边距为10%）
                        int padding = (int)(squareSize * 0.1);
                        Rectangle imageRect = new Rectangle(
                            rect.X + padding,
                            rect.Y + padding,
                            squareSize - padding * 2,
                            squareSize - padding * 2);

                        // 绘制图片（如果有）
                        if (images != null && faceValue > 0 && faceValue <= images.Length && images[faceValue - 1] != null)
                        {
                            {
                                int angle = net.IdMap[faceValue - 1];
                                DrawRotatedImage(g, images[faceValue - 1], imageRect, angle);
                            //    g.DrawImage(images[faceValue - 1], imageRect);
                            }
                        }
                        else
                        {
                            // 没有图片时，使用默认填充
                            using (Brush brush = new SolidBrush(Color.FromArgb(100, 200, 200, 255)))
                            {
                                g.FillRectangle(brush, rect);
                            }
                        }
                    }
                }

                // 最后绘制所有边框（确保边框不被图片覆盖）
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        int faceValue = net.Layout[i, j];
                        if (faceValue == 0) continue;

                        Rectangle rect = new Rectangle(
                            j * squareSize + 10,
                            i * squareSize + 10,
                            squareSize, squareSize);

                        // 绘制正方形边框
                        g.DrawRectangle(Pens.Black, rect);
                    }
                }

                // 添加标题
                if (drawName)
                {
                    using (Font font = new Font("Arial", 14, FontStyle.Bold))
                    {
                        g.DrawString(net.Name, font, Brushes.Black, 10, 10);
                    }
                }
            }

            return bitmap;
        }
        private static void DrawRotatedImage(Graphics g, Image image, Rectangle rect, int angle)
        {
            // 保存当前变换状态
            var state = g.Save();

            try
            {
                // 将坐标系原点移动到矩形中心
                g.TranslateTransform(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                // 旋转指定角度
                g.RotateTransform(angle);

                // 将坐标系原点移回矩形左上角
                g.TranslateTransform(-rect.Width / 2, -rect.Height / 2);

                // 在旋转后的坐标系中绘制图像
                g.DrawImage(image, new Rectangle(Point.Empty, rect.Size));
            }
            finally
            {
                // 恢复原始变换状态
                g.Restore(state);
            }
        }
        /// <summary>生成字符贴图（支持数字、字母、符号）</summary>
        public static Bitmap GenerateCharTexture(char c, int size = 256)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                Font font = new Font("Consolas", size / 2, FontStyle.Bold);
                g.DrawString(c.ToString(), font, Brushes.Black,
                             new PointF(size / 2, size / 2),
                             new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            return bmp;
        }

        // 扩展几何形状枚举
        public enum GeoShape { Triangle, Quad, Pentagon, Hexagon, Circle, Star, Cross, Diamond }

        // 扩展样式选项
        [Flags]
        public enum GeoStyle
        {
            None = 0,
            Filled = 1,           // 填充内部
            Border = 2,           // 绘制边框
            CenterMark = 4,       // 添加中心标记
            RandomColor = 8       // 随机颜色
        }

        /// <summary>随机生成几何纹理参数</summary>
        public static Bitmap GenerateRandomGeoTexture(int size = 256)
        {
            Random rnd = new Random();

            // 随机选择形状
            GeoShape[] allShapes = (GeoShape[])Enum.GetValues(typeof(GeoShape));
            GeoShape randomShape = allShapes[rnd.Next(allShapes.Length)];

            // 随机样式（至少包含填充或边框）
            GeoStyle randomStyle = GeoStyle.None;
            if (rnd.Next(2) == 0) randomStyle |= GeoStyle.Filled;
            if (rnd.Next(2) == 0 || randomStyle == GeoStyle.None) randomStyle |= GeoStyle.Border;
            if (rnd.Next(3) == 0) randomStyle |= GeoStyle.CenterMark;
            if (rnd.Next(2) == 0) randomStyle |= GeoStyle.RandomColor;

            // 随机边框宽度（1-8px，根据尺寸调整）
            float borderWidth = 1f + (float)rnd.NextDouble() * Math.Min(8f, size / 30f);

            // 随机颜色（如果未选择随机模式）
            Color? fillColor = null;
            Color? borderColor = null;

            if ((randomStyle & GeoStyle.RandomColor) == 0)
            {
                // 生成协调的填充色和边框色
                int hue = rnd.Next(360);
                float saturation = 0.6f + (float)rnd.NextDouble() * 0.4f;
                float lightness = 0.4f + (float)rnd.NextDouble() * 0.4f;

                fillColor = HslToRgb(hue, saturation, lightness, 180); // 半透明
                borderColor = HslToRgb(hue, saturation, Math.Max(0f, lightness - 0.3f), 255); // 较暗的边框
            }

            // 添加中心标记的概率
            bool addCenterMark = (randomStyle & GeoStyle.CenterMark) != 0;

            // 生成最终纹理
            return GenerateGeoTexture(
                randomShape,
                size,
                borderWidth,
                fillColor,
                borderColor,
                randomStyle,
                addCenterMark
            );
        }

        // 辅助方法：HSL颜色转RGB（用于生成协调的随机颜色）
        private static Color HslToRgb(int hue, float saturation, float lightness, int alpha = 255)
        {
            float c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
            float x = c * (1 - Math.Abs((hue / 60f) % 2 - 1));
            float m = lightness - c / 2;

            float r, g, b;
            if (hue >= 0 && hue < 60) { r = c; g = x; b = 0; }
            else if (hue >= 60 && hue < 120) { r = x; g = c; b = 0; }
            else if (hue >= 120 && hue < 180) { r = 0; g = c; b = x; }
            else if (hue >= 180 && hue < 240) { r = 0; g = x; b = c; }
            else if (hue >= 240 && hue < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromArgb(
                alpha,
                (int)((r + m) * 255),
                (int)((g + m) * 255),
                (int)((b + m) * 255)
            );
        }

        /// <summary>生成高级几何图形贴图（支持多种形状、样式和颜色）</summary>
        public static Bitmap GenerateGeoTexture(
            GeoShape shape,
            int size = 256,
            float borderWidth = 3f,
            Color? fillColor = null,
            Color? borderColor = null,
            GeoStyle style = GeoStyle.Filled | GeoStyle.Border,
            bool addCenterMark = false)
        {
            // 默认颜色
            if (!fillColor.HasValue) fillColor = Color.FromArgb(150, 0, 120, 255);
            if (!borderColor.HasValue) borderColor = Color.Black;

            // 随机颜色支持
            if ((style & GeoStyle.RandomColor) != 0)
            {
                Random rnd = new Random();
                fillColor = Color.FromArgb(150, rnd.Next(256), rnd.Next(256), rnd.Next(256));
                borderColor = Color.FromArgb(255, 255 - fillColor.Value.R, 255 - fillColor.Value.G, 255 - fillColor.Value.B);
            }

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                // 计算中心和半径
                float centerX = size / 2f;
                float centerY = size / 2f;
                float radius = (size - Math.Max(borderWidth, 10)) / 2f;

                // 创建画笔和画刷
                using (Pen borderPen = new Pen(borderColor.Value, borderWidth))
                using (Brush fillBrush = new SolidBrush(fillColor.Value))
                {
                    // 根据形状绘制
                    switch (shape)
                    {
                        case GeoShape.Triangle:
                            DrawPolygon(g, 3, centerX, centerY, radius, fillBrush, borderPen, style);
                            break;
                        case GeoShape.Quad:
                            DrawPolygon(g, 4, centerX, centerY, radius, fillBrush, borderPen, style);
                            break;
                        case GeoShape.Pentagon:
                            DrawPolygon(g, 5, centerX, centerY, radius, fillBrush, borderPen, style);
                            break;
                        case GeoShape.Hexagon:
                            DrawPolygon(g, 6, centerX, centerY, radius, fillBrush, borderPen, style);
                            break;
                        case GeoShape.Circle:
                            if ((style & GeoStyle.Filled) != 0)
                                g.FillEllipse(fillBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
                            if ((style & GeoStyle.Border) != 0)
                                g.DrawEllipse(borderPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
                            break;
                        case GeoShape.Star:
                            DrawStar(g, 5, centerX, centerY, radius, fillBrush, borderPen, style);
                            break;
                        case GeoShape.Cross:
                            DrawCross(g, centerX, centerY, radius, fillBrush, borderPen, style);
                            break;
                        case GeoShape.Diamond:
                            DrawPolygon(g, 4, centerX, centerY, radius, fillBrush, borderPen, style, 45); // 旋转45度的四边形=菱形
                            break;
                    }

                    // 添加中心标记
                    if ((style & GeoStyle.CenterMark) != 0 || addCenterMark)
                    {
                        float markSize = radius / 8f;
                        using (Brush markBrush = new SolidBrush(Color.FromArgb(200, 255, 0, 0)))
                        {
                            g.FillEllipse(markBrush, centerX - markSize, centerY - markSize, markSize * 2, markSize * 2);
                        }
                        using (Pen markPen = new Pen(Color.FromArgb(255, 100, 0, 0), 2f))
                        {
                            g.DrawEllipse(markPen, centerX - markSize, centerY - markSize, markSize * 2, markSize * 2);
                        }
                    }
                }
            }
            return bmp;
        }

        // 辅助方法：绘制正多边形
        private static void DrawPolygon(Graphics g, int sides, float centerX, float centerY, float radius,
                                       Brush fillBrush, Pen borderPen, GeoStyle style, float rotation = 0)
        {
            PointF[] points = new PointF[sides];
            float angleStep = (float)(2 * Math.PI / sides);

            for (int i = 0; i < sides; i++)
            {
                float angle = (float)(i * angleStep + rotation * Math.PI / 180);
                points[i] = new PointF(
                    centerX + radius * (float)Math.Cos(angle),
                    centerY + radius * (float)Math.Sin(angle)
                );
            }

            if ((style & GeoStyle.Filled) != 0)
                g.FillPolygon(fillBrush, points);
            if ((style & GeoStyle.Border) != 0)
                g.DrawPolygon(borderPen, points);
        }

        // 辅助方法：绘制五角星
        private static void DrawStar(Graphics g, int points, float centerX, float centerY, float radius,
                                    Brush fillBrush, Pen borderPen, GeoStyle style)
        {
            PointF[] starPoints = new PointF[points * 2];
            float outerRadius = radius;
            float innerRadius = radius / 2.5f;
            float angleStep = (float)(Math.PI / points);

            for (int i = 0; i < points * 2; i++)
            {
                float r = (i % 2 == 0) ? outerRadius : innerRadius;
                float angle = (float)(i * angleStep - Math.PI / 2);
                starPoints[i] = new PointF(
                    centerX + r * (float)Math.Cos(angle),
                    centerY + r * (float)Math.Sin(angle)
                );
            }

            if ((style & GeoStyle.Filled) != 0)
                g.FillPolygon(fillBrush, starPoints);
            if ((style & GeoStyle.Border) != 0)
                g.DrawPolygon(borderPen, starPoints);
        }

        // 辅助方法：绘制十字
        private static void DrawCross(Graphics g, float centerX, float centerY, float radius,
                                     Brush fillBrush, Pen borderPen, GeoStyle style)
        {
            float width = radius * 0.3f;
            float height = radius * 1.2f;

            RectangleF vertical = new RectangleF(centerX - width / 2, centerY - height / 2, width, height);
            RectangleF horizontal = new RectangleF(centerX - height / 2, centerY - width / 2, height, width);

            if ((style & GeoStyle.Filled) != 0)
            {
                g.FillRectangle(fillBrush, vertical);
                g.FillRectangle(fillBrush, horizontal);
            }

            if ((style & GeoStyle.Border) != 0)
            {
                g.DrawRectangle(borderPen, vertical.X, vertical.Y, vertical.Width, vertical.Height);
                g.DrawRectangle(borderPen, horizontal.X, horizontal.Y, horizontal.Width, horizontal.Height);
            }
        }

        /// <summary>生成随机线条贴图（可控制密度和颜色）</summary>
        public static Bitmap GenerateRandomLines(int size, int density, Color lineColor)
        {

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                Random rnd = new Random();
                Pen pen = new Pen(lineColor, 2);
                for (int i = 0; i < density; i++)
                {
                    g.DrawLine(pen,
                        rnd.Next(size), rnd.Next(size),
                        rnd.Next(size), rnd.Next(size));
                }
            }
            return bmp;
        }


        /// <summary>生成填充纹理</summary>
        // 1. 优化填充样式枚举（减少复杂类型）
        public enum FillStyle
        {
            LinearGradient,  // 保留线性渐变（高效）
            RadialGradientSimplified, // 简化版径向渐变
            SolidColor,      // 纯色（高效）
            // 移除复杂条纹样式，减少计算量
        }

        // 2. 优化颜色生成（避免过浅/白色）
        private static Color GetRandomColor()
        {
            Random rnd = new Random();
            // 限制RGB值在50-200之间（避免过亮接近白色）
            return Color.FromArgb(
                255,  // 不透明
                rnd.Next(50, 200),  // R
                rnd.Next(50, 200),  // G
                rnd.Next(50, 200)   // B
            );
        }

        // 3. 优化渐变生成逻辑（提升效率）
        public static Bitmap GenerateFillTexture(FillStyle style, int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            // 直接锁定位图数据，减少GDI+上下文切换耗时
            Rectangle rect = new Rectangle(0, 0, size, size);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                rect,
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat
            );

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * size;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            try
            {
                Color color1 = GetRandomColor();
                Color color2 = GetComplementaryColor(color1); // 确保颜色差异明显

                switch (style)
                {
                    case FillStyle.LinearGradient:
                        // 简化线性渐变计算（直接操作像素数组）
                        LinearGradient(rgbValues, size, bmpData.Stride, color1, color2);
                        break;
                    case FillStyle.RadialGradientSimplified:
                        // 简化径向渐变（减少计算量）
                        RadialGradientSimple(rgbValues, size, bmpData.Stride, color1, color2);
                        break;
                    case FillStyle.SolidColor:
                        // 纯色填充（高效）
                        SolidFill(rgbValues, color1);
                        break;
                }

                // 将处理后的像素数据写回位图
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            }
            finally
            {
                // 解锁位图（必须执行）
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // 辅助方法：生成互补色
        private static Color GetComplementaryColor(Color color)
        {
            // 简单实现：计算互补色
            return Color.FromArgb(
                255,
                255 - color.R,
                255 - color.G,
                255 - color.B
            );
        }
        // 线性渐变核心算法（直接操作像素数组）
        private static void LinearGradient(byte[] pixels, int size, int stride, Color c1, Color c2)
        {
            int pixelSize = 4; // 32位ARGB
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 计算渐变比例（x方向渐变）
                    float ratio = (float)x / size;
                    // 混合颜色（避免浮点运算冗余）
                    byte r = (byte)(c1.R * (1 - ratio) + c2.R * ratio);
                    byte g = (byte)(c1.G * (1 - ratio) + c2.G * ratio);
                    byte b = (byte)(c1.B * (1 - ratio) + c2.B * ratio);
                    byte a = 255; // 不透明

                    // 写入像素数组（注意Stride可能有对齐字节）
                    int index = y * stride + x * pixelSize;
                    pixels[index] = b;     // B
                    pixels[index + 1] = g; // G
                    pixels[index + 2] = r; // R
                    pixels[index + 3] = a; // A
                }
            }
        }

        // 简化版径向渐变（减少三角函数计算）
        private static void RadialGradientSimple(byte[] pixels, int size, int stride, Color c1, Color c2)
        {
            int pixelSize = 4;
            int centerX = size / 2;
            int centerY = size / 2;
            float maxDist = (float)Math.Sqrt(centerX * centerX + centerY * centerY); // 最大距离

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 简化距离计算（用平方代替开方，提升效率）
                    int dx = x - centerX;
                    int dy = y - centerY;
                    float distSq = dx * dx + dy * dy;
                    float ratio = (float)Math.Min(distSq / (maxDist * maxDist), 1.0); // 归一化

                    // 混合颜色
                    byte r = (byte)(c1.R * (1 - ratio) + c2.R * ratio);
                    byte g = (byte)(c1.G * (1 - ratio) + c2.G * ratio);
                    byte b = (byte)(c1.B * (1 - ratio) + c2.B * ratio);
                    byte a = 255;

                    int index = y * stride + x * pixelSize;
                    pixels[index] = b;
                    pixels[index + 1] = g;
                    pixels[index + 2] = r;
                    pixels[index + 3] = a;
                }
            }
        }

        // 纯色填充（高效）
        private static void SolidFill(byte[] pixels, Color color)
        {
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;
            byte a = 255;

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = b;
                pixels[i + 1] = g;
                pixels[i + 2] = r;
                pixels[i + 3] = a;
            }
        }

        // 4. 调整填充纹理生成权重（优先高效类型）
        private static Bitmap GenerateFillTextureImage(ImageGenerationOptions options)
        {
            FillStyle[] styles = (FillStyle[])Enum.GetValues(typeof(FillStyle));
            List<FillStyle> weightedStyles = new List<FillStyle>();

            foreach (var style in styles)
            {
                int weight = style == FillStyle.RadialGradientSimplified ? 1 : 2; // 复杂类型权重低
                for (int i = 0; i < weight; i++)
                {
                    weightedStyles.Add(style);
                }
            }

            FillStyle randomStyle = weightedStyles[_rnd.Next(weightedStyles.Count)];
            return CubeNetDrawer.GenerateFillTexture(randomStyle, options.Size);
        }
        /// <summary>生成箭头贴图（上下左右四个方向）</summary>
        public static Bitmap GenerateArrow(ArrowDir dir, int size = 256)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                Pen pen = new Pen(Color.Black, 5);
                Brush fill = new SolidBrush(Color.Black);
                int arrowSize = size / 3;

                switch (dir)
                {
                    case ArrowDir.Up:
                        g.DrawLine(pen, size / 2, size / 2 + arrowSize, size / 2, size / 2 - arrowSize);
                        g.DrawPolygon(pen, new Point[] {
                    new Point(size/2-arrowSize/2, size/2-arrowSize),
                    new Point(size/2+arrowSize/2, size/2-arrowSize),
                    new Point(size/2, size/2-2*arrowSize)
                });
                        break;
                    // 同理实现 Down、Left、Right 方向...
                }
            }
            return bmp;
        }

        public enum ArrowDir { Up, Down, Left, Right }
    }
}
