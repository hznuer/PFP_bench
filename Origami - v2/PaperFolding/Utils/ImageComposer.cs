using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;

namespace Origami
{
    public static class ImageComposer
    {
        /// <summary>
        /// 生成合成图片：优化箭头绘制与布局对齐
        /// </summary>
        public static Bitmap Compose(List<Image> firstRowImages, List<Image>[] candidateImagesGroup,
                        int arrowWidth = 40,                   // 箭头宽度
                        int spacing = 20,                      // 图片间/元素内间距
                        int candidateQueueSpacing = 50,        // 候选答案队列之间的间距
                        int firstRowSize = 200,                // 第一行图片尺寸
                        int candidateSize = 180,               // 候选答案图片尺寸
                        bool firstRowBorder = true,            // 第一行是否加边框
                        bool candidateBorder = true,           // 候选答案是否加边框
                        int borderWidth = 4,                   // 边框宽度
                        int margin = 30,                       // 左右边距
                        bool showLabels = true,                // 是否显示标签（A、B、C...）
                        bool drawFirstRowArrows = false,        // 是否绘制第一行箭头
                        bool drawCandidateArrows = true,       // 是否绘制候选答案箭头
                        int noArrowSpacing = 30,               // 无箭头时的图片间距
                        int singleRowImageMinCount = 2,        // 触发单行的最小图片数量
                        int labelToContentSpacing = 15)        // 标签与候选内容的间距
        {
            // 参数校验
            if (firstRowImages == null) throw new ArgumentNullException("firstRowImages");
            if (candidateImagesGroup == null) throw new ArgumentNullException("candidateImagesGroup");
            if (candidateImagesGroup.Any(g => g == null || g.Count == 0))
                throw new ArgumentException("候选答案队列不能包含空列表或空图像", "candidateImagesGroup");

            // 计算第一行尺寸（考虑箭头开关）
            int firstRowWidth = CalculateImageQueueWidth(firstRowImages,
                firstRowSize, drawFirstRowArrows ? arrowWidth : 0,
                drawFirstRowArrows ? spacing : noArrowSpacing);
            int firstRowHeight = firstRowSize;

            // 计算标签尺寸
            Font labelFont = new Font("Arial", 24, FontStyle.Bold);
            SizeF maxLabelSize = new SizeF(0, 0);
            if (showLabels)
            {
                using (Bitmap tempBmp = new Bitmap(1, 1))
                using (Graphics tempG = Graphics.FromImage(tempBmp))
                {
                    maxLabelSize = tempG.MeasureString("Z", labelFont);
                }
            }
            float labelWidth = maxLabelSize.Width;
            float labelHeight = maxLabelSize.Height;

            // 计算每个候选答案的尺寸（考虑箭头开关）
            List<int> candidateQueueWidths = new List<int>();
            List<int> totalCandidateWidths = new List<int>();
            int candidateContentHeight = candidateSize;

            // 分析每个候选队列是否单独一行
            List<bool> isSingleRow = new List<bool>();
            foreach (var candidateQueue in candidateImagesGroup)
            {
                int contentWidth = CalculateImageQueueWidth(candidateQueue,
                    candidateSize, drawCandidateArrows ? arrowWidth : 0,
                    drawCandidateArrows ? spacing : noArrowSpacing);
                candidateQueueWidths.Add(contentWidth);
                int totalWidth = (int)(labelWidth + labelToContentSpacing + contentWidth);
                totalCandidateWidths.Add(totalWidth);
                isSingleRow.Add(candidateQueue.Count >= singleRowImageMinCount);
            }

            // 计算候选区布局
            List<int> candidateRowWidths = new List<int>();
            List<List<int>> rowCandidates = new List<List<int>>();

            List<int> currentRow = new List<int>();
            int currentRowWidth = 0;

            for (int i = 0; i < candidateImagesGroup.Length; i++)
            {
                if (isSingleRow[i])
                {
                    if (currentRow.Count > 0)
                    {
                        candidateRowWidths.Add(currentRowWidth);
                        rowCandidates.Add(new List<int>(currentRow));
                        currentRow.Clear();
                        currentRowWidth = 0;
                    }
                    candidateRowWidths.Add(totalCandidateWidths[i]);
                    rowCandidates.Add(new List<int> { i });
                }
                else
                {
                    int newWidth = currentRow.Count > 0
                        ? currentRowWidth + totalCandidateWidths[i] + candidateQueueSpacing
                        : totalCandidateWidths[i];
                    if (currentRow.Count < 2)
                    {
                        currentRow.Add(i);
                        currentRowWidth = newWidth;
                    }
                    else
                    {
                        candidateRowWidths.Add(currentRowWidth);
                        rowCandidates.Add(new List<int>(currentRow));
                        currentRow = new List<int> { i };
                        currentRowWidth = totalCandidateWidths[i];
                    }
                }
            }

            // 提交最后一行
            if (currentRow.Count > 0)
            {
                candidateRowWidths.Add(currentRowWidth);
                rowCandidates.Add(currentRow);
            }

            // 计算画布尺寸
            int maxRowWidth = 0;
            foreach (int width in candidateRowWidths)
                if (width > maxRowWidth) maxRowWidth = width;
            int canvasWidth = Math.Max(firstRowWidth, maxRowWidth) + 2 * margin;

            int rowCount = rowCandidates.Count;
            int rowHeight = (int)Math.Max(candidateContentHeight, labelHeight) + spacing;
            int candidateTotalHeight = rowCount * rowHeight + (rowCount - 1) * spacing * 2;
            int canvasHeight = margin + firstRowHeight + spacing * 3 + candidateTotalHeight + margin;

            // 绘制画布
            using (Bitmap canvas = new Bitmap(canvasWidth, canvasHeight))
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 1. 绘制第一行（问题行）
                int firstRowX = margin + (canvasWidth - 2 * margin - firstRowWidth) / 2;
                int firstRowY = margin;
                DrawImageQueue(g, firstRowImages, firstRowX, firstRowY,
                             firstRowSize, drawFirstRowArrows ? arrowWidth : 0,
                             drawFirstRowArrows ? spacing : noArrowSpacing,
                             firstRowBorder, borderWidth, drawFirstRowArrows);

                // 2. 绘制候选答案区
                int currentY = firstRowY + firstRowHeight + spacing * 3;
                int labelIndex = 0;

                for (int rowIndex = 0; rowIndex < rowCandidates.Count; rowIndex++)
                {
                    var rowCandidateIndices = rowCandidates[rowIndex];
                    int rowTotalWidth = candidateRowWidths[rowIndex];
                    int rowStartX = margin + (canvasWidth - 2 * margin - rowTotalWidth) / 2;
                    int currentX = rowStartX;

                    foreach (int candidateIndex in rowCandidateIndices)
                    {
                        var candidateQueue = candidateImagesGroup[candidateIndex];
                        int contentWidth = candidateQueueWidths[candidateIndex];
                        int contentX = currentX + (int)(labelWidth + labelToContentSpacing);
                        int contentY = currentY;

                        // 绘制候选边框
                        if (candidateBorder)
                        {
                            int borderX = contentX - 10;
                            int borderY = contentY - 10;
                            int borderW = contentWidth + 20;
                            int borderH = candidateContentHeight + 20;
                            DrawCandidateGroupBorder(g, borderX, borderY, borderW, borderH, borderWidth);
                        }

                        // 绘制候选内容
                        DrawImageQueue(g, candidateQueue, contentX, contentY,
                                     candidateSize, drawCandidateArrows ? arrowWidth : 0,
                                     drawCandidateArrows ? spacing : noArrowSpacing,
                                     candidateBorder, borderWidth, drawCandidateArrows);

                        // 绘制左侧标签
                        if (showLabels)
                        {
                            string label = ((char)('A' + labelIndex)).ToString();
                            float labelY = contentY + (candidateContentHeight - labelHeight) / 2;
                            g.DrawString(label, labelFont, Brushes.Black, currentX, labelY);
                        }

                        currentX += totalCandidateWidths[candidateIndex] + candidateQueueSpacing;
                        labelIndex++;
                    }

                    currentY += rowHeight + spacing * 2;
                }

                return new Bitmap(canvas);
            }
        }

        // 计算图片队列宽度（考虑箭头开关）
        private static int CalculateImageQueueWidth(List<Image> images, int imageSize, int arrowWidth, int spacing)
        {
            if (images.Count == 0) return 0;
            // 当不绘制箭头时，箭头宽度视为0
            return images.Count * imageSize
                   + (images.Count - 1) * arrowWidth
                   + (images.Count - 1) * spacing;
        }

        // 绘制图片队列（优化箭头与间距逻辑）
        private static void DrawImageQueue(Graphics g, List<Image> images, int startX, int startY,
                                         int imageSize, int arrowWidth, int spacing,
                                         bool drawBorder, int borderWidth, bool drawArrows)
        {
            int currentX = startX;

            for (int i = 0; i < images.Count; i++)
            {
                using (Image resizedImg = ResizeToFit(images[i], new Size(imageSize, imageSize)))
                {
                    if (drawBorder)
                        DrawBorder(g, currentX, startY, imageSize, imageSize, borderWidth);
                    g.DrawImage(resizedImg, currentX, startY);
                }

                if (i < images.Count - 1)
                {
                    if (drawArrows)
                    {
                        currentX += imageSize;
                        int arrowY = startY + imageSize / 2;
                        DrawArrow(g, currentX + 10, arrowY, currentX + arrowWidth - 10, arrowY);
                        currentX += arrowWidth + spacing;
                    }
                    else
                    {
                        // 不绘制箭头时，只添加无箭头间距
                        currentX += imageSize + spacing;
                    }
                }
                else
                {
                    currentX += imageSize;
                }
            }
        }

        // 绘制候选答案组边框
        private static void DrawCandidateGroupBorder(Graphics g, int x, int y, int width, int height, int borderWidth)
        {
            using (Pen pen = new Pen(Color.Black, borderWidth))
            {
                g.DrawRectangle(pen, x, y, width, height);
                using (Pen shadowPen = new Pen(Color.FromArgb(100, Color.Gray), borderWidth / 2))
                {
                    g.DrawRectangle(shadowPen, x + 2, y + 2, width, height);
                }
            }
        }


        // 合成主方法 - 移除圆角并修复边框问题
        public static Bitmap Compose(List<Image> firstRowImages, List<Image> candidateImages,
                              int arrowWidth = 40,            // 箭头宽度
                              int spacing = 60,             // 间距
                              int firstRowSize = 200,       // 第一排图片大小
                              int candidateSize = 180,      // 第二排图片大小
                              bool firstRowBorder = true,   // 是否为第一排添加边框
                              int borderWidth = 4,          // 边框宽度
                              int margin = 30,              // 左右边距
                              bool showLabels = true)       // 是否显示标签
        {
            // 计算第一排布局
            int firstRowHeight = firstRowSize;
            int firstRowWidth = CalculateFirstRowWidth(firstRowImages, arrowWidth, spacing);

            // 计算第二排布局
            int secondRowHeight = candidateSize;
            int secondRowWidth = candidateImages.Count * (candidateSize + spacing) - spacing;

            // 最终画布尺寸
            int width = Math.Max(firstRowWidth, secondRowWidth) + margin * 2;
            int height = firstRowHeight + secondRowHeight + spacing * 4 + (showLabels ? 30 : 0);

            Bitmap canvas = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 计算第一排水平居中位置
                int firstRowX = margin + (width - 2 * margin - firstRowWidth) / 2;

                // 绘制第一排（图片 + 箭头 + 问号）
                DrawFirstRow(g, firstRowImages, firstRowX, spacing, firstRowHeight,
                             spacing, arrowWidth, firstRowBorder, borderWidth);

                // 计算第二排水平居中位置
                int secondRowX = margin + (width - 2 * margin - secondRowWidth) / 2;

                // 绘制第二排（候选图片 + 边框 + 标签）
                DrawCandidateRow(g, candidateImages, secondRowX, candidateSize,
                                 spacing, firstRowHeight + spacing * 2,
                                 borderWidth, showLabels);
            }
            return canvas;
        }

        // 计算第一排总宽度
        private static int CalculateFirstRowWidth(List<Image> images, int arrowWidth, int spacing)
        {
            return images.Count * 200 + (images.Count - 1) * (arrowWidth + spacing) + 50;
        }

        // 绘制第一排 - 修复边框问题
        private static void DrawFirstRow(Graphics g, List<Image> images, int startX, int spacing,
                                        int targetHeight, int topPadding, int arrowWidth,
                                        bool drawBorder, int borderWidth)
        {
            int x = startX;
            int imageCount = images.Count;

            // 绘制所有图片和箭头
            for (int i = 0; i < imageCount; i++)
            {
                Image img = images[i];
                Image resized = ResizeToFit(img, new Size(targetHeight, targetHeight));

                int y = topPadding;

                // 绘制边框
                if (drawBorder)
                    DrawBorder(g, x, y, targetHeight, targetHeight, borderWidth);

                g.DrawImage(resized, x, y);
                resized.Dispose();

                // 如果不是最后一张图片，绘制箭头
                if (i < imageCount - 1)
                {
                    x += targetHeight;
                    DrawArrow(g, x + 15, y + targetHeight / 2, x + arrowWidth, y + targetHeight / 2);
                    x += arrowWidth + spacing;
                }
            }

            //// 绘制问号图片（单独处理）
            //x += targetHeight; // 移动到问号位置
            //using (Image questionMark = LoadQuestionMarkImage(targetHeight))
            //{
            //    int y = topPadding;
            //    // 为问号添加边框
            //    if (drawBorder)
            //        DrawBorder(g, x, y, targetHeight, targetHeight, borderWidth);

            //    g.DrawImage(questionMark, x, y);
            //}
        }

        // 加载问号图片
        private static Image LoadQuestionMarkImage(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                Font font = new Font("Arial", size * 0.6f, FontStyle.Bold);
                g.DrawString("?", font, Brushes.Blue, size / 2 - 10, size / 4);
            }
            return bmp;
        }

        // 绘制候选行 - 添加标签支持
        private static void DrawCandidateRow(Graphics g, List<Image> candidates, int startX,
                                            int size, int spacing, int y,
                                            int borderWidth, bool showLabels)
        {
            int x = startX;
            for (int i = 0; i < candidates.Count; i++)
            {
                using (Image resized = ResizeToFit(candidates[i], new Size(size, size)))
                {
                    // 绘制边框（矩形）
                    DrawBorder(g, x, y, size, size, borderWidth);
                    g.DrawImage(resized, x, y);

                    // 绘制标签 (A, B, C, ...)
                    if (showLabels)
                    {
                        string label = ((char)('A' + i)).ToString();
                        Font labelFont = new Font("Arial", 16, FontStyle.Bold);
                        SizeF labelSize = g.MeasureString(label, labelFont);

                        g.DrawString(label, labelFont, Brushes.Black,
                            x + size / 2 - labelSize.Width / 2,
                            y + size + 10);
                    }

                    x += size + spacing;
                }
            }
        }

        // 绘制边框 - 改为矩形边框
        private static void DrawBorder(Graphics g, int x, int y, int width, int height, int borderWidth)
        {
            using (Pen pen = new Pen(Color.Black, borderWidth))
            {
                g.DrawRectangle(pen, x, y, width, height);
            }
        }

        // 缩放图片到指定尺寸
        private static Image ResizeToFit(Image image, Size target)
        {
            float scaleW = (float)target.Width / image.Width;
            float scaleH = (float)target.Height / image.Height;
            float scale = Math.Min(scaleW, scaleH);

            int width = (int)(image.Width * scale);
            int height = (int)(image.Height * scale);

            Bitmap resized = new Bitmap(target.Width, target.Height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.Clear(Color.White);
                g.DrawImage(image,
                    (target.Width - width) / 2,
                    (target.Height - height) / 2,
                    width, height);
            }
            return resized;
        }

        // 绘制箭头
        private static void DrawArrow(Graphics g, int x1, int y1, int x2, int y2)
        {
            Pen pen = new Pen(Color.Blue, 2);
            g.DrawLine(pen, x1, y1, x2, y2);

            int arrowSize = 10;
            Point[] points = {
            new Point(x2, y2),
            new Point(x2 - arrowSize, y2 - arrowSize / 2),
            new Point(x2 - arrowSize, y2 + arrowSize / 2)
        };
            g.FillPolygon(Brushes.Blue, points);
        }
    }

}