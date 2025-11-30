using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace Origami
{
    public static class ImageComposerV2
    {
        /// <summary>
        /// 组合图像（第一排可空，第二排无箭头，支持第一排图像数量少时自动放大）
        /// </summary>
        public static Bitmap ComposeImages(
            List<Image> firstRowImages,
            List<Image> secondRowImages,
            bool firstRowHasArrows = true,
            int arrowWidth = 40,
            int spacing = 60,
            int firstRowSize = 480,  // 默认第一排尺寸
            int secondRowSize = 360,
            bool firstRowBorder = true,
            bool secondRowBorder = true,
            int borderWidth = 4,
            int margin = 30,
            bool showLabels = true)
        {
            // 校验第二排图像（不可null/空）
            if (secondRowImages == null || secondRowImages.Count == 0)
                return null;

            // 调整第一排图像尺寸：1张图放大1.5倍，2张图放大1.2倍
            int adjustedFirstRowSize = firstRowSize; // 初始为默认尺寸
            if (firstRowImages != null && firstRowImages.Count > 0)
            {
                switch (firstRowImages.Count)
                {
                    case 1:
                        adjustedFirstRowSize = (int)(firstRowSize * 1.5); // 1张图放大50%
                        break;
                    case 2:
                        adjustedFirstRowSize = (int)(firstRowSize * 1.2); // 2张图放大20%
                        break;
                    // 3张及以上使用默认尺寸
                    default:
                        adjustedFirstRowSize = firstRowSize;
                        break;
                }
            }

            // 计算第一排尺寸（使用调整后的尺寸）
            int firstRowWidth = 0;
            int firstRowHeight = 0;
            if (firstRowImages != null && firstRowImages.Count > 0)
            {
                firstRowHeight = adjustedFirstRowSize; // 使用调整后的高度
                // 第一排宽度 = 所有图像宽度 + 箭头总宽度（若有） + 间距总宽度
                firstRowWidth = firstRowImages.Count * adjustedFirstRowSize
                    + (firstRowHasArrows ? (firstRowImages.Count - 1) * arrowWidth : 0)
                    + (firstRowImages.Count - 1) * spacing;
            }

            // 计算第二排尺寸（无箭头，仅图像+间距）
            int secondRowHeight = secondRowSize;
            int secondRowWidth = secondRowImages.Count * secondRowSize
                + (secondRowImages.Count - 1) * spacing;

            // 计算画布总尺寸（使用调整后的第一排宽度）
            int canvasWidth = Math.Max(firstRowWidth, secondRowWidth) + 2 * margin;
            int canvasHeight = margin; // 顶部边距

            // 累加第一排高度（若存在，使用调整后的高度）
            if (firstRowImages != null && firstRowImages.Count > 0)
            {
                canvasHeight += firstRowHeight + spacing * 3; // 第一排高度 + 与第二排的间距
            }

            // 累加第二排高度（含标签高度）
            canvasHeight += secondRowHeight + (showLabels ? 40 : 0) + margin; // 第二排高度 + 标签高度 + 底部边距

            // 创建画布
            using (Bitmap canvas = new Bitmap(canvasWidth, canvasHeight))
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 1. 绘制第一排（若存在，使用调整后的尺寸）
                int currentY = margin;
                if (firstRowImages != null && firstRowImages.Count > 0)
                {
                    // 第一排水平居中（基于调整后的宽度）
                    int firstRowX = margin + (canvasWidth - 2 * margin - firstRowWidth) / 2;
                    DrawImageRow(
                        g,
                        firstRowImages,
                        firstRowX,
                        currentY,
                        adjustedFirstRowSize, // 传入调整后的尺寸
                        firstRowHasArrows,
                        arrowWidth,
                        spacing,
                        firstRowBorder,
                        borderWidth);
                    currentY += firstRowHeight + spacing * 3; // 移动到第二排起始Y坐标
                }

                // 2. 绘制第二排（无箭头）
                int secondRowX = margin + (canvasWidth - 2 * margin - secondRowWidth) / 2;
                DrawImageRow(
                    g,
                    secondRowImages,
                    secondRowX,
                    currentY,
                    secondRowSize,
                    hasArrows: false, // 第二排无箭头
                    arrowWidth: 0,
                    spacing: spacing,
                    drawBorder: secondRowBorder,
                    borderWidth: borderWidth);

                // 3. 绘制第二排标签（A/B/C...）
                if (showLabels)
                {
                    Font labelFont = new Font("Arial", 18, FontStyle.Bold);
                    for (int i = 0; i < secondRowImages.Count; i++)
                    {
                        string label = ((char)('A' + i)).ToString();
                        // 标签位置：图像下方居中
                        int labelX = secondRowX + i * (secondRowSize + spacing) + secondRowSize / 2 - 10;
                        int labelY = currentY + secondRowSize + 10;
                        g.DrawString(label, labelFont, Brushes.Black, labelX, labelY);
                    }
                }

                return new Bitmap(canvas); // 返回画布副本（避免using释放）
            }
        }

        /// <summary>
        /// 绘制一排图像（支持箭头控制）
        /// </summary>
        private static void DrawImageRow(
            Graphics g,
            List<Image> images,
            int startX,
            int startY,
            int imageSize,
            bool hasArrows,
            int arrowWidth,
            int spacing,
            bool drawBorder,
            int borderWidth)
        {
            int currentX = startX;
            for (int i = 0; i < images.Count; i++)
            {
                Image img = images[i];
                // 缩放图像并居中绘制
                using (Image resizedImg = ResizeToFit(img, new Size(imageSize, imageSize)))
                {
                    // 绘制边框
                    if (drawBorder)
                    {
                        g.DrawRectangle(
                            new Pen(Color.Black, borderWidth),
                            currentX,
                            startY,
                            imageSize,
                            imageSize);
                    }
                    // 绘制图像（居中）
                    g.DrawImage(
                        resizedImg,
                        currentX,
                        startY,
                        imageSize,
                        imageSize);
                }

                // 绘制箭头（若启用且不是最后一个图像）
                if (hasArrows && i < images.Count - 1)
                {
                    int arrowStartX = currentX + imageSize;
                    int arrowY = startY + imageSize / 2;
                    int arrowEndX = arrowStartX + arrowWidth;
                    DrawArrow(g, arrowStartX, arrowY, arrowEndX, arrowY);
                    currentX = arrowEndX + spacing; // 移动到下一个图像起始X
                }
                else
                {
                    // 无箭头时直接移动到下一个图像（仅加间距）
                    currentX += imageSize + (i < images.Count - 1 ? spacing : 0);
                }
            }
        }

        /// <summary>
        /// 缩放图像至目标尺寸（保持比例，空白处填充白色）
        /// </summary>
        private static Image ResizeToFit(Image image, Size targetSize)
        {
            Bitmap resized = new Bitmap(targetSize.Width, targetSize.Height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.Clear(Color.White);
                float scale = Math.Min(
                    (float)targetSize.Width / image.Width,
                    (float)targetSize.Height / image.Height);
                int scaledWidth = (int)(image.Width * scale);
                int scaledHeight = (int)(image.Height * scale);
                // 图像居中绘制
                g.DrawImage(
                    image,
                    (targetSize.Width - scaledWidth) / 2,
                    (targetSize.Height - scaledHeight) / 2,
                    scaledWidth,
                    scaledHeight);
            }
            return resized;
        }

        /// <summary>
        /// 绘制箭头
        /// </summary>
        private static void DrawArrow(Graphics g, int x1, int y1, int x2, int y2)
        {
            using (Pen pen = new Pen(Color.Blue, 2))
            {
                g.DrawLine(pen, x1, y1, x2, y2);
            }
            // 绘制箭头三角形
            int arrowHeadSize = 10;
            Point[] arrowHead = new Point[] {
                new Point(x2, y2),
                new Point(x2 - arrowHeadSize, y2 - arrowHeadSize / 2),
                new Point(x2 - arrowHeadSize, y2 + arrowHeadSize / 2)
            };
            g.FillPolygon(Brushes.Blue, arrowHead);
        }
    }
}