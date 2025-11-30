using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;

namespace Origami
{
    #region 枚举与配置类扩展
    /// <summary>
    /// 图片类别枚举（新增马赛克类别）
    /// </summary>
    [Flags]
    public enum ImageCategory
    {
        Geometric = 1 << 0,
        Character = 1 << 1,
        Lines = 1 << 2,
        FillTexture = 1 << 3,
        Arrow = 1 << 4,
        Blank = 1 << 5,
        Mosaic = 1 << 6, // 新增马赛克类别
        All = Geometric | Character | Lines | FillTexture | Arrow | Blank | Mosaic // 包含马赛克
    }

    /// <summary>
    /// 马赛克图片专用配置
    /// </summary>
    public class MosaicOptions
    {
        private int _gridSize = 6; // 6x6网格
        private float _fillRatio = 0.35f; // 填充比例≤35%
        private Color _fillColor = Color.Black;
        private Color _bgColor = Color.White;
        private int _minBlockSize = 2;
        private int _maxBlockSize = 5;

        /// <summary>网格尺寸（默认6x6）</summary>
        public int GridSize
        {
            get { return _gridSize; }
            set { _gridSize = Math.Max(3, Math.Min(10, value)); }
        }

        /// <summary>填充比例（10%-35%）</summary>
        public float FillRatio
        {
            get { return _fillRatio; }
            set { _fillRatio = Math.Max(0.1f, Math.Min(0.35f, value)); }
        }

        /// <summary>填充色</summary>
        public Color FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }

        /// <summary>背景色</summary>
        public Color BgColor
        {
            get { return _bgColor; }
            set { _bgColor = value; }
        }

        /// <summary>最小连通块大小</summary>
        public int MinBlockSize
        {
            get { return _minBlockSize; }
            set { _minBlockSize = Math.Max(2, value); }
        }

        /// <summary>最大连通块大小</summary>
        public int MaxBlockSize
        {
            get { return _maxBlockSize; }
            set { _maxBlockSize = Math.Max(_minBlockSize, value); }
        }
    }

    /// <summary>
    /// 扩展生成参数（包含马赛克配置）
    /// </summary>
    public class ImageGenerationOptions
    {
        private int _size = 256;
        private ImageCategory _categories = ImageCategory.All & ~ImageCategory.Blank;
        private int _lineDensity = 2;
        private bool _allowDuplicates = false;
        private bool _geoUseRandomColors = true;
        private string _characterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private bool _allowBlankImages = false;
        private int _lineWidth = 4;
        private MosaicOptions _mosaicOptions = new MosaicOptions(); // 马赛克专用配置

        /// <summary>图片尺寸（默认256x256）</summary>
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>生成的图片类别</summary>
        public ImageCategory Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        /// <summary>线条数量（1-3，默认2）</summary>
        public int LineDensity
        {
            get { return _lineDensity; }
            set { _lineDensity = Math.Max(1, Math.Min(3, value)); }
        }

        /// <summary>是否允许重复图片（强制false）</summary>
        public bool AllowDuplicates
        {
            get { return _allowDuplicates; }
            set { _allowDuplicates = false; }
        }

        /// <summary>几何图形是否启用随机颜色</summary>
        public bool GeoUseRandomColors
        {
            get { return _geoUseRandomColors; }
            set { _geoUseRandomColors = value; }
        }

        /// <summary>字符集</summary>
        public string CharacterSet
        {
            get { return _characterSet; }
            set { _characterSet = value; }
        }

        /// <summary>允许生成空白图片（开关）</summary>
        public bool AllowBlankImages
        {
            get { return _allowBlankImages; }
            set
            {
                _allowBlankImages = value;
                if (value)
                    _categories |= ImageCategory.Blank;
                else
                    _categories &= ~ImageCategory.Blank;
            }
        }

        /// <summary>线条宽度（默认4px）</summary>
        public int LineWidth
        {
            get { return _lineWidth; }
            set { _lineWidth = Math.Max(2, Math.Min(8, value)); }
        }

        /// <summary>马赛克图片专用配置</summary>
        public MosaicOptions MosaicOptions
        {
            get { return _mosaicOptions; }
            set { _mosaicOptions = value ?? new MosaicOptions(); }
        }

        public ImageGenerationOptions()
        {
            _size = 256;
            _categories = ImageCategory.All & ~ImageCategory.Blank;
            _lineDensity = 2;
            _allowDuplicates = false;
            _geoUseRandomColors = true;
            _characterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            _allowBlankImages = false;
            _lineWidth = 4;
            _mosaicOptions = new MosaicOptions();
        }
    }
    #endregion

    #region 图片生成器核心类
    public static class RandomImageGenerator
    {
        private static readonly Random _rnd;
        private static readonly Dictionary<ImageCategory, Func<ImageGenerationOptions, Bitmap>> _generators;

        static RandomImageGenerator()
        {
            _rnd = new Random();
            _generators = new Dictionary<ImageCategory, Func<ImageGenerationOptions, Bitmap>>();

            _generators.Add(ImageCategory.Geometric, GenerateGeometricImage);
            _generators.Add(ImageCategory.Character, GenerateCharacterImage);
            _generators.Add(ImageCategory.Lines, GenerateLinesImage);
            _generators.Add(ImageCategory.FillTexture, GenerateFillTextureImage);
            _generators.Add(ImageCategory.Arrow, GenerateArrowImage);
            _generators.Add(ImageCategory.Blank, GenerateBlankImage);
            _generators.Add(ImageCategory.Mosaic, GenerateMosaicImage); // 注册马赛克生成器
        }

        #region 核心生成方法
        /// <summary>生成6张不同的图片（支持马赛克类别）</summary>
        public static Bitmap[] Generate6Images(ImageGenerationOptions options)
        {
            if (options == null)
                options = new ImageGenerationOptions();

            ValidateOptions(options);

            List<Bitmap> result = new List<Bitmap>();
            List<ImageCategory> usedCategories = GetEnabledCategories(options.Categories);

            int maxAttempts = 100;
            int attempts = 0;

            while (result.Count < 6 && attempts < maxAttempts)
            {
                attempts++;
                ImageCategory category = usedCategories[_rnd.Next(usedCategories.Count)];

                try
                {
                    Bitmap image = TimeoutHelper.RunWithTimeout(() =>
                        _generators[category](options),
                        timeoutMs: 1000);

                    if (result.Exists(delegate(Bitmap b) { return AreImagesIdentical(b, image); }))
                    {
                        image.Dispose();
                        continue;
                    }

                    result.Add(image);
                }
                catch (Exception ex)
                {
                    // Console.WriteLine( "生成图片失败");
                }
            }

            while (result.Count < 6)
            {
                result.Add(GenerateBlankImage(options));
            }

            return result.ToArray();
        }

        /// <summary>超时辅助类</summary>
        private static class TimeoutHelper
        {
            public static T RunWithTimeout<T>(Func<T> function, int timeoutMs)
            {
                T result = default(T);
                var thread = new Thread(() =>
                {
                    try { result = function(); }
                    catch { }
                });

                thread.IsBackground = true;
                thread.Start();

                if (!thread.Join(timeoutMs))
                {
                    thread.Abort();
                    throw new TimeoutException("图片生成超时");
                }

                return result;
            }
        }

        /// <summary>根据字符串生成6张字符图片</summary>
        public static Bitmap[] GenerateFromText(string text, ImageGenerationOptions options = null)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 6)
                throw new ArgumentException("输入字符串长度必须≥6", "text");

            options = options ?? new ImageGenerationOptions();
            options.Categories = ImageCategory.Character;
            options.AllowDuplicates = false;

            HashSet<char> selectedChars = new HashSet<char>();
            while (selectedChars.Count < 6)
            {
                char c = text[_rnd.Next(text.Length)];
                selectedChars.Add(c);
            }

            List<Bitmap> result = new List<Bitmap>();
            foreach (char c in selectedChars)
            {
                result.Add(GenerateCharacterImage(c, options));
            }

            return result.ToArray();
        }

        /// <summary>根据字符串生成6张字符图片</summary>
        public static Bitmap[] GenerateAllFromText(string text, ImageGenerationOptions options = null)
        {


            options = options ?? new ImageGenerationOptions();
            options.Categories = ImageCategory.Character;
            options.AllowDuplicates = false;



            List<Bitmap> result = new List<Bitmap>();
            foreach (char c in text)
            {
                result.Add(GenerateCharacterImage(c, options));
            }

            return result.ToArray();
        }

        /// <summary>生成6张马赛克图片（专用方法）</summary>
        public static Bitmap[] Generate6MosaicImages(ImageGenerationOptions options = null)
        {
            options = options ?? new ImageGenerationOptions();
            options.Categories = ImageCategory.Mosaic; // 强制马赛克类别
            return Generate6Images(options);
        }
        #endregion

        #region 类别生成方法（含马赛克）
        // 马赛克图片生成核心函数
        private static Bitmap GenerateMosaicImage(ImageGenerationOptions options)
        {
            MosaicOptions mosaicOpts = options.MosaicOptions;
            int size = options.Size;
            int gridSize = mosaicOpts.GridSize;
            int cellSize = size / gridSize;

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(mosaicOpts.BgColor);

                int maxFillCount = (int)(gridSize * gridSize * mosaicOpts.FillRatio);
                maxFillCount = Math.Max(2, maxFillCount);

                bool[,] grid = GenerateMosaicGrid(gridSize, maxFillCount, mosaicOpts);

                for (int y = 0; y < gridSize; y++)
                {
                    for (int x = 0; x < gridSize; x++)
                    {
                        if (grid[x, y])
                        {
                            Rectangle cellRect = new Rectangle(
                                x * cellSize,
                                y * cellSize,
                                cellSize,
                                cellSize
                            );
                            g.FillRectangle(new SolidBrush(mosaicOpts.FillColor), cellRect);
                            g.DrawRectangle(Pens.LightGray, cellRect); // 网格线
                        }
                    }
                }
            }
            return bmp;
        }

        // 生成马赛克网格（含连通块）
        private static bool[,] GenerateMosaicGrid(int gridSize, int maxFillCount, MosaicOptions options)
        {
            bool[,] grid = new bool[gridSize, gridSize];
            int filledCount = 0;
            Random rnd = new Random();

            // 俄罗斯方块式基础形状
            List<List<Point>> tetrominoShapes = new List<List<Point>>
            {
                new List<Point> { new Point(0,0), new Point(1,0), new Point(2,0) }, // 直线形
                new List<Point> { new Point(0,0), new Point(0,1), new Point(1,0) }, // L形
                new List<Point> { new Point(0,0), new Point(1,0), new Point(0,1), new Point(1,1) }, // 2x2方块
                new List<Point> { new Point(0,0), new Point(1,0), new Point(2,0), new Point(1,1) }, // T形
                new List<Point> { new Point(0,0), new Point(0,1), new Point(1,1), new Point(2,1) } // 倒L形
            };

            while (filledCount < maxFillCount)
            {
                var shape = tetrominoShapes[rnd.Next(tetrominoShapes.Count)];
                int shapeSize = shape.Count;

                if (filledCount + shapeSize > maxFillCount)
                {
                    shape = shape.Take(maxFillCount - filledCount).ToList();
                    shapeSize = shape.Count;
                    if (shapeSize < options.MinBlockSize) break;
                }

                int startX = rnd.Next(gridSize - 2);
                int startY = rnd.Next(gridSize - 2);

                bool canPlace = true;
                foreach (var p in shape)
                {
                    int x = startX + p.X;
                    int y = startY + p.Y;
                    if (x >= gridSize || y >= gridSize || grid[x, y])
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    foreach (var p in shape)
                    {
                        int x = startX + p.X;
                        int y = startY + p.Y;
                        grid[x, y] = true;
                        filledCount++;
                    }
                }
                else if (filledCount < options.MinBlockSize)
                {
                    PlaceFallbackBlock(grid, gridSize, ref filledCount);
                }
            }

            return grid;
        }

        // 放置基础连通块（ fallback ）
        private static void PlaceFallbackBlock(bool[,] grid, int gridSize, ref int filledCount)
        {
            Random rnd = new Random();
            int startX = rnd.Next(gridSize - 1);
            int startY = rnd.Next(gridSize - 1);

            if (rnd.Next(2) == 0)
            {
                grid[startX, startY] = true;
                grid[startX + 1, startY] = true; // 水平
            }
            else
            {
                grid[startX, startY] = true;
                grid[startX, startY + 1] = true; // 垂直
            }
            filledCount += 2;
        }

        // 箭头生成（保持不变）
        public enum ArrowDir
        {
            Up, Down, Left, Right,
            UpLeft, UpRight, DownLeft, DownRight
        }

        private static Bitmap GenerateArrowImage(ImageGenerationOptions options)
        {
            Bitmap bmp = new Bitmap(options.Size, options.Size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Pen pen = new Pen(Color.Black, options.LineWidth);
                Brush fill = new SolidBrush(Color.Black);
                int margin = options.Size / 8;
                int arrowSize = (options.Size - 2 * margin) / 3;
                int centerX = options.Size / 2;
                int centerY = options.Size / 2;

                ArrowDir[] allDirections = new ArrowDir[] {
                    ArrowDir.Up, ArrowDir.Down, ArrowDir.Left, ArrowDir.Right,
                    ArrowDir.UpLeft, ArrowDir.UpRight, ArrowDir.DownLeft, ArrowDir.DownRight
                };

                ArrowDir dir = allDirections[_rnd.Next(allDirections.Length)];

                switch (dir)
                {
                    case ArrowDir.Up:
                        g.DrawLine(pen, centerX, centerY + arrowSize, centerX, centerY - arrowSize);
                        DrawArrowHead(g, pen, fill, centerX, centerY - arrowSize, 0);
                        break;
                    case ArrowDir.Down:
                        g.DrawLine(pen, centerX, centerY - arrowSize, centerX, centerY + arrowSize);
                        DrawArrowHead(g, pen, fill, centerX, centerY + arrowSize, 180);
                        break;
                    case ArrowDir.Left:
                        g.DrawLine(pen, centerX + arrowSize, centerY, centerX - arrowSize, centerY);
                        DrawArrowHead(g, pen, fill, centerX - arrowSize, centerY, 270);
                        break;
                    case ArrowDir.Right:
                        g.DrawLine(pen, centerX - arrowSize, centerY, centerX + arrowSize, centerY);
                        DrawArrowHead(g, pen, fill, centerX + arrowSize, centerY, 90);
                        break;
                    case ArrowDir.UpLeft:
                        g.DrawLine(pen, centerX + arrowSize, centerY + arrowSize, centerX - arrowSize, centerY - arrowSize);
                        DrawArrowHead(g, pen, fill, centerX - arrowSize, centerY - arrowSize, 315);
                        break;
                    case ArrowDir.UpRight:
                        g.DrawLine(pen, centerX - arrowSize, centerY + arrowSize, centerX + arrowSize, centerY - arrowSize);
                        DrawArrowHead(g, pen, fill, centerX + arrowSize, centerY - arrowSize, 45);
                        break;
                    case ArrowDir.DownLeft:
                        g.DrawLine(pen, centerX + arrowSize, centerY - arrowSize, centerX - arrowSize, centerY + arrowSize);
                        DrawArrowHead(g, pen, fill, centerX - arrowSize, centerY + arrowSize, 225);
                        break;
                    case ArrowDir.DownRight:
                        g.DrawLine(pen, centerX - arrowSize, centerY - arrowSize, centerX + arrowSize, centerY + arrowSize);
                        DrawArrowHead(g, pen, fill, centerX + arrowSize, centerY + arrowSize, 135);
                        break;
                }
            }
            return bmp;
        }

        private static void DrawArrowHead(Graphics g, Pen pen, Brush fill, float x, float y, float angle)
        {
            int arrowSize = 15;
            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, new PointF(x, y));
            g.Transform = matrix;

            PointF[] points = new PointF[] {
                new PointF(x, y - arrowSize),
                new PointF(x - arrowSize/2, y),
                new PointF(x + arrowSize/2, y)
            };

            g.FillPolygon(fill, points);
            g.DrawPolygon(pen, points);
            g.ResetTransform();
        }

        // 线条图生成（保持不变）
        private static Bitmap GenerateLinesImage(ImageGenerationOptions options)
        {
            Bitmap bmp = new Bitmap(options.Size, options.Size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int lineWidth = options.LineWidth >= 5 ? options.LineWidth : 5;
                bool isThick = _rnd.Next(2) == 0;
                lineWidth = isThick ? lineWidth : Math.Max(3, lineWidth / 2);

                Pen pen = new Pen(Color.Black, lineWidth);
                int lineCount = _rnd.Next(1, 5);
                int positionType = _rnd.Next(3);

                switch (positionType)
                {
                    case 0: GenerateCenterLines(g, pen, lineCount, options.Size); break;
                    case 1: GenerateCornerLines(g, pen, lineCount, options.Size); break;
                    case 2: GenerateMixedLines(g, pen, lineCount, options.Size); break;
                }
            }
            return bmp;
        }

        private static void GenerateCenterLines(Graphics g, Pen pen, int lineCount, int size)
        {
            int margin = size / 4;
            int centerX = size / 2;
            int centerY = size / 2;

            for (int i = 0; i < lineCount; i++)
            {
                float angle = _rnd.Next(360);
                float length = _rnd.Next(margin, size - 2 * margin);
                float startX = centerX + (float)Math.Cos(angle * Math.PI / 180) * (length / 2);
                float startY = centerY + (float)Math.Sin(angle * Math.PI / 180) * (length / 2);
                float endX = centerX - (float)Math.Cos(angle * Math.PI / 180) * (length / 2);
                float endY = centerY - (float)Math.Sin(angle * Math.PI / 180) * (length / 2);
                g.DrawLine(pen, startX, startY, endX, endY);
            }
        }

        private static void GenerateCornerLines(Graphics g, Pen pen, int lineCount, int size)
        {
            Rectangle[] corners = new Rectangle[] {
                new Rectangle(0, 0, size/2, size/2),
                new Rectangle(size/2, 0, size/2, size/2),
                new Rectangle(0, size/2, size/2, size/2),
                new Rectangle(size/2, size/2, size/2, size/2)
            };

            for (int i = 0; i < lineCount; i++)
            {
                Rectangle corner = corners[_rnd.Next(corners.Length)];
                float x1 = corner.X + _rnd.Next(corner.Width);
                float y1 = corner.Y + _rnd.Next(corner.Height);
                float x2 = corner.X + _rnd.Next(corner.Width);
                float y2 = corner.Y + _rnd.Next(corner.Height);
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        private static void GenerateMixedLines(Graphics g, Pen pen, int lineCount, int size)
        {
            int centerLines = lineCount / 2;
            int cornerLines = lineCount - centerLines;
            GenerateCenterLines(g, pen, centerLines, size);
            GenerateCornerLines(g, pen, cornerLines, size);
        }

        // 字符图片生成（保持不变）
        private static Bitmap GenerateCharacterImage(ImageGenerationOptions options)
        {
            if (string.IsNullOrEmpty(options.CharacterSet))
                throw new ArgumentException("字符集不能为空", "options.CharacterSet");

            char randomChar = options.CharacterSet[_rnd.Next(options.CharacterSet.Length)];
            return GenerateCharacterImage(randomChar, options);
        }

        private static Bitmap GenerateCharacterImage(char c, ImageGenerationOptions options)
        {
            Bitmap bmp = new Bitmap(options.Size, options.Size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Font font = new Font("Consolas", options.Size / 2, FontStyle.Bold);
                SizeF charSize = g.MeasureString(c.ToString(), font);
                float x = (options.Size - charSize.Width) / 2;
                float y = (options.Size - charSize.Height) / 2;
                g.DrawString(c.ToString(), font, Brushes.Black, x, y);
            }
            return bmp;
        }

        // 空白图片生成（保持不变）
        private static Bitmap GenerateBlankImage(ImageGenerationOptions options)
        {
            Bitmap bmp = new Bitmap(options.Size, options.Size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }
            return bmp;
        }

        // 几何图形生成（保持不变）
        private static Bitmap GenerateGeometricImage(ImageGenerationOptions options)
        {
            if (options.GeoUseRandomColors)
            {
                return CubeNetDrawer.GenerateRandomGeoTexture(options.Size);
            }
            else
            {
                CubeNetDrawer.GeoShape[] shapes = (CubeNetDrawer.GeoShape[])Enum.GetValues(typeof(CubeNetDrawer.GeoShape));
                CubeNetDrawer.GeoShape randomShape = shapes[_rnd.Next(shapes.Length)];
                return CubeNetDrawer.GenerateGeoTexture(
                    shape: randomShape,
                    size: options.Size,
                    style: CubeNetDrawer.GeoStyle.Filled | CubeNetDrawer.GeoStyle.Border
                );
            }
        }

        // 填充纹理生成（保持不变）
        private static Bitmap GenerateFillTextureImage(ImageGenerationOptions options)
        {
            CubeNetDrawer.FillStyle[] styles = (CubeNetDrawer.FillStyle[])Enum.GetValues(typeof(CubeNetDrawer.FillStyle));
            CubeNetDrawer.FillStyle randomStyle = styles[_rnd.Next(styles.Length)];
            return CubeNetDrawer.GenerateFillTexture(randomStyle, options.Size);
        }
        #endregion

        #region 辅助方法（保持不变）
        private static bool AreImagesIdentical(Bitmap b1, Bitmap b2)
        {
            if (b1.Size != b2.Size || b1.PixelFormat != b2.PixelFormat)
                return false;

            if (b1.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                return CompareNon32bpp(b1, b2);

            Rectangle rect = new Rectangle(0, 0, b1.Width, b1.Height);
            System.Drawing.Imaging.BitmapData data1 = b1.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, b1.PixelFormat);
            System.Drawing.Imaging.BitmapData data2 = b2.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, b2.PixelFormat);

            try
            {
                unsafe
                {
                    byte* ptr1 = (byte*)data1.Scan0;
                    byte* ptr2 = (byte*)data2.Scan0;
                    int stride = data1.Stride;
                    int height = b1.Height;
                    int width = b1.Width * 4;

                    for (int y = 0; y < height; y++)
                    {
                        byte* row1 = ptr1 + y * stride;
                        byte* row2 = ptr2 + y * stride;
                        for (int x = 0; x < width; x++)
                        {
                            if (row1[x] != row2[x])
                                return false;
                        }
                    }
                }
                return true;
            }
            finally
            {
                b1.UnlockBits(data1);
                b2.UnlockBits(data2);
            }
        }

        private static bool CompareNon32bpp(Bitmap b1, Bitmap b2)
        {
            using (Bitmap conv1 = new Bitmap(b1.Width, b1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (Bitmap conv2 = new Bitmap(b2.Width, b2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(conv1))
                    g.DrawImage(b1, 0, 0);
                using (Graphics g = Graphics.FromImage(conv2))
                    g.DrawImage(b2, 0, 0);
                return AreImagesIdentical(conv1, conv2);
            }
        }

        private static Color GetRandomDarkColor()
        {
            return Color.FromArgb(
                255,
                _rnd.Next(30, 100),
                _rnd.Next(30, 100),
                _rnd.Next(30, 150)
            );
        }

        private static List<ImageCategory> GetEnabledCategories(ImageCategory categories)
        {
            List<ImageCategory> enabled = new List<ImageCategory>();

            foreach (ImageCategory cat in Enum.GetValues(typeof(ImageCategory)))
            {
                if (cat != ImageCategory.All && (categories & cat) != 0)
                    enabled.Add(cat);
            }

            if (enabled.Count == 0)
                throw new ArgumentException("至少需要启用一个图片类别", "categories");

            return enabled;
        }

        private static void ValidateOptions(ImageGenerationOptions options)
        {
            if (options.Size < 32 || options.Size > 1024)
                throw new ArgumentOutOfRangeException("options.Size", "图片尺寸必须在32-1024之间");

            if (options.LineDensity < 1 || options.LineDensity > 3)
                throw new ArgumentOutOfRangeException("options.LineDensity", "线条数量必须在1-3之间");
        }
        #endregion
    }
    #endregion
}