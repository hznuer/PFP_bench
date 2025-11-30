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
        SymmetricGeometric = 1 << 7, // 新增对称的Geometric类别
        All = Geometric | Character | Lines | FillTexture | Arrow | Blank | Mosaic | SymmetricGeometric // 包含新类别
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
            _generators.Add(ImageCategory.SymmetricGeometric, GenerateSymmetricGeometricImage); // 注册对称的Geometric生成器
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

        #region 类别生成方法（含新类别）
        // 对称的Geometric图片生成核心函数
        // 对称几何图形生成方法
        // 自定义结构体来存储形状信息
        public struct ShapeInfo
        {
            public int ShapeType;
            public int Size;
            public bool IsFilled;

            public ShapeInfo(int shapeType, int size, bool isFilled)
            {
                ShapeType = shapeType;
                Size = size;
                IsFilled = isFilled;
            }
        }

        private static List<int> GetSzList(int size, int num, int minSz = 12, int minSpan = 24)
        {
            List<int> retList = new List<int>();
            if (num == 1)
            {
                int tmp = _rnd.Next(size / 3) + size * 2 / 3;
                retList.Add(tmp);
            }
            else if (num == 2)
            {
                int tmp1 = _rnd.Next(size / 4) + size / 4;
                retList.Add(tmp1);

                if (_rnd.NextDouble() > 0.7)
                {
                    retList.Add(tmp1 + _rnd.Next(24) + 24);
                }
                else
                {
                    int tmp2 = _rnd.Next(size / 3) + size * 2 / 3;
                    tmp2 = Math.Max(tmp1 + minSpan, tmp2);
                    retList.Add(tmp2);
                }
            }
            else if (num == 3)
            {
                int tmp1 = _rnd.Next(size / 3) + 2*minSz;
                retList.Add(tmp1);

                int tmp2 = _rnd.Next(size / 3) + size / 3;
                tmp2 = Math.Max(tmp1 + minSpan, tmp2);
                retList.Add(tmp2);

                if (_rnd.NextDouble() > 0.7)
                {
                    retList.Add(tmp2 + _rnd.Next(24) + 24);
                }
                else
                {
                    int tmp3 = _rnd.Next(size / 3) + size * 2 / 3;
                    tmp3 = Math.Max(tmp2 + minSpan, tmp3);
                    retList.Add(tmp3);
                }
            }
            return retList;
        }

        private enum ShapeType { Triangle, Square, Diamond, Pentagon, Hexagon, Circle }
        private static void DrawSymmetric1x1(Graphics g, List<Rectangle> rtList)
        {
            Size sz = rtList[0].Size;
            Rectangle rt = rtList[0];

            int maxNum = 2;
            if (sz.Width > 200)
                maxNum = 3;

            int targetCount = _rnd.Next(2) + 2;




            //    using (Pen bluePen = new Pen(Color.Blue, 2))
            using (SolidBrush blueBrush = new SolidBrush(Color.Blue))
            {
                List<int> szList = GetSzList((int)(rt.Width * 0.4), targetCount);
                int id = 0;
                if (szList.Count == 1)
                {
                    int err = 0;
                }
                if (szList.Last() < 40)
                {
                    int err = 0;
                }
                foreach (int objSz in szList)
                {
                    ShapeType typeId = (ShapeType)_rnd.Next(6);

                    if (id == szList.Count-1)
                    {
                        if (typeId == ShapeType.Triangle)
                            typeId = (ShapeType)(_rnd.Next(4) + 2);
 
                    }
                    bool isFill = false;
                    if (id == 0)
                        isFill = _rnd.NextDouble() > 0.75;


                    Pen bluePen = new Pen(Color.Blue, _rnd.Next(7) + 7);
                    g.ResetTransform();

                    bool needDrawCross = false;
                    needDrawCross = _rnd.NextDouble() > 0.7;
                    bool isAngle45 = false;
                    isAngle45 = _rnd.NextDouble() > 0.5;
                    int angle = _rnd.Next(360);

                    switch (typeId)
                    {
                        case ShapeType.Circle:

                            foreach (Rectangle tmpRt in rtList)
                            {
                                Point cenPt = new Point(tmpRt.X + tmpRt.Width / 2, tmpRt.Y + tmpRt.Height / 2);
                                g.TranslateTransform(cenPt.X, cenPt.Y);
                                if (isFill)
                                    g.FillEllipse(blueBrush, new Rectangle(-objSz, -objSz, objSz * 2, objSz * 2));
                                else
                                    g.DrawEllipse(bluePen, new Rectangle(-objSz, -objSz, objSz * 2, objSz * 2));

                            }
                            break;
                        case ShapeType.Square:

                            foreach (Rectangle tmpRt in rtList)
                            {
                                Point cenPt = new Point(tmpRt.X + tmpRt.Width / 2, tmpRt.Y + tmpRt.Height / 2);
                                g.TranslateTransform(cenPt.X, cenPt.Y);
                                if (isFill)
                                    g.FillRectangle(blueBrush, new Rectangle(-objSz, -objSz, objSz * 2, objSz * 2));
                                else
                                    g.DrawRectangle(bluePen, new Rectangle(-objSz, -objSz, objSz * 2, objSz * 2));

                            }
                            break;
                        case ShapeType.Diamond:
                            {
                                int newSz = (int)(Math.Sqrt(2) * objSz/2 + 12);
                                PointF[] diamondPoints = new PointF[]
                                {
                                    new PointF(0, -newSz), // 上
                                    new PointF(newSz, 0), // 右
                                    new PointF(0, newSz), // 下
                                    new PointF(-newSz, 0)  // 左
                                };
                                foreach (Rectangle tmpRt in rtList)
                                {
                                    Point cenPt = new Point(tmpRt.X + tmpRt.Width / 2, tmpRt.Y + tmpRt.Height / 2);
                                    g.TranslateTransform(cenPt.X, cenPt.Y);
                                    if (isFill)
                                        g.FillPolygon(blueBrush, diamondPoints);
                                    else
                                        g.DrawPolygon(bluePen, diamondPoints);

                                }
                            }
                            break;
                        case ShapeType.Pentagon:
                        case ShapeType.Hexagon:
                        case ShapeType.Triangle:
                            {
                                List<PointF> ptList = new List<PointF>();
                                int numPt = 3;
                                angle = _rnd.Next(6) * 60;
                                if (typeId == ShapeType.Pentagon)
                                {
                                    numPt = 5;
                                    angle = 0;
                                }
                                if (typeId == ShapeType.Hexagon)
                                   numPt = 6;

                                float anglespan = 360 / numPt;

                                for (int i = 0; i < numPt; i++)
                                {
                                    double curAngle = (angle + anglespan * i) * Math.PI / 180;
                                    double x = objSz * Math.Cos(curAngle);
                                    double y = objSz * Math.Sin(curAngle);
                                    ptList.Add(new PointF((float)(x), (float)(y)));

                                }


                                PointF[] diamondPoints = ptList.ToArray();
                                foreach (Rectangle tmpRt in rtList)
                                {
                                    Point cenPt = new Point(tmpRt.X + tmpRt.Width / 2, tmpRt.Y + tmpRt.Height / 2);
                                    g.TranslateTransform(cenPt.X, cenPt.Y);
                                    if (isFill)
                                        g.FillPolygon(blueBrush, diamondPoints);
                                    else
                                        g.DrawPolygon(bluePen, diamondPoints);
                                }
                            }
                            break;

                    }
                    bool isDouble = _rnd.Next() > 0.5;

                    if(szList.Count<3)
                    foreach (Rectangle tmpRt in rtList)
                    {
                        if (needDrawCross && !isFill)
                        {
                            int crosslen = ( objSz + 20)/2;
                            if (isDouble)
                            {
                                if (isAngle45)
                                {
                                    Pen pen = new Pen(Brushes.Blue, bluePen.Width * 2);

                                    g.DrawLine(pen, new Point(crosslen, crosslen), new Point(-crosslen, -crosslen));
                                    g.DrawLine(pen, new Point(crosslen, -crosslen), new Point(-crosslen, crosslen));
                                    pen.Dispose();
                                }
                                else
                                {
                                    g.DrawLine(bluePen, new Point(-20, crosslen), new Point(-20, -crosslen));
                                    g.DrawLine(bluePen, new Point(20, crosslen), new Point(20, -crosslen));
                                    g.DrawLine(bluePen, new Point(crosslen, -20), new Point(-crosslen, -20));
                                    g.DrawLine(bluePen, new Point(crosslen, 20), new Point(-crosslen, 20));
                                }
                            }
                            else
                            {
                                if (isAngle45)
                                {
                                    g.DrawLine(bluePen, new Point(crosslen, objSz + 20), new Point(-crosslen, -crosslen));
                                    g.DrawLine(bluePen, new Point(crosslen, -crosslen), new Point(-crosslen, crosslen));
                                }
                                else
                                {
                                    g.DrawLine(bluePen, new Point(0, crosslen), new Point(0, -crosslen));
                                    g.DrawLine(bluePen, new Point(crosslen, 0), new Point(-crosslen, 0));
                                }
                            }
                        }
                        
                    }
                    bluePen.Dispose();
                    id++;
                }
            }


        }

        private static void DrawSymmetricOuterInner(Graphics g, Rectangle rt)
        {
            Size sz = rt.Size;

            int maxNum = 1;
            if (sz.Width > 200)
                maxNum = 3;
            else if (sz.Width > 100)
                maxNum = 2;
            int targetCount = _rnd.Next(maxNum) + 1;




        }


        private static Bitmap GenerateSymmetricGeometricImage2(ImageGenerationOptions options)
        {
            var options2 = new ShapeGenerationOptions
            {
                Size = options.Size, // 图片尺寸
                OuterLineWidth = new Range<int>(5, 10), // 外框线宽5-10
                InnerCount = new Range<int>(1, 3),      // 内部图元1-3个
                InnerSizeRatio = new Range<float>(0.15f, 0.25f), // 内部尺寸比例
                AllowInnerFilledRandom = true           // 允许内部随机填充
            };

            // 生成器（可传入固定种子实现复现）
            var renderer = new ShapeRenderer(Environment.TickCount);
            var bmp = renderer.Generate(options2);
            return bmp;
        }

        private static Bitmap GenerateSymmetricGeometricImage(ImageGenerationOptions options)
        {

            Bitmap bmp = new Bitmap(options.Size, options.Size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Random rnd = new Random();
                bool isOneByOne = rnd.Next(2) == 0;
                isOneByOne = true;
                if (isOneByOne)
                {
                    Rectangle rt = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    List<Rectangle> rtList = new List<Rectangle>();
                    rtList.Add(rt);
                    DrawSymmetric1x1(g, rtList);

                }
                else
                {
                    // 2X2 区域
                    int subSize = options.Size / 2;

                    // 生成子区域的形状信息
                    int shapeCount = rnd.Next(0, 3); // 子区域最多有 2 个 shape
                    List<ShapeInfo> shapes = new List<ShapeInfo>();
                    for (int i = 0; i < shapeCount; i++)
                    {
                        int shapeType = rnd.Next(4); // 0: 圆, 1: 正方形, 2: 棱形, 3: 十字架
                        int size = rnd.Next(subSize / 8, subSize / 4);
                        bool isFilled = rnd.Next(2) == 0;
                        shapes.Add(new ShapeInfo(shapeType, size, isFilled));
                    }

                    // 随机决定是否绘制分割线
                    bool drawDivider = rnd.Next(2) == 0;
                    if (drawDivider)
                    {
                        Pen dividerPen = options.GeoUseRandomColors ? new Pen(GetRandomColor()) : new Pen(Color.Black);
                        g.DrawLine(dividerPen, options.Size / 2, 0, options.Size / 2, options.Size);
                        g.DrawLine(dividerPen, 0, options.Size / 2, options.Size, options.Size / 2);
                        dividerPen.Dispose();
                    }

                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            int subCenterX = x * subSize + subSize / 2;
                            int subCenterY = y * subSize + subSize / 2;

                            // 绘制子区域的形状
                            foreach (ShapeInfo shape in shapes)
                            {
                                Brush brush = options.GeoUseRandomColors ? new SolidBrush(GetRandomColor()) : new SolidBrush(Color.Black);
                                Pen pen = options.GeoUseRandomColors ? new Pen(GetRandomColor()) : new Pen(Color.Black);

                                switch (shape.ShapeType)
                                {
                                    case 0: // 圆
                                        if (shape.IsFilled)
                                        {
                                            g.FillEllipse(brush, subCenterX - shape.Size / 2, subCenterY - shape.Size / 2, shape.Size, shape.Size);
                                        }
                                        else
                                        {
                                            g.DrawEllipse(pen, subCenterX - shape.Size / 2, subCenterY - shape.Size / 2, shape.Size, shape.Size);
                                        }
                                        break;
                                    case 1: // 正方形
                                        if (shape.IsFilled)
                                        {
                                            g.FillRectangle(brush, subCenterX - shape.Size / 2, subCenterY - shape.Size / 2, shape.Size, shape.Size);
                                        }
                                        else
                                        {
                                            g.DrawRectangle(pen, subCenterX - shape.Size / 2, subCenterY - shape.Size / 2, shape.Size, shape.Size);
                                        }
                                        break;
                                    case 2: // 棱形
                                        Point[] diamondPoints = new Point[]
                                {
                                    new Point(subCenterX, subCenterY - shape.Size / 2),
                                    new Point(subCenterX + shape.Size / 2, subCenterY),
                                    new Point(subCenterX, subCenterY + shape.Size / 2),
                                    new Point(subCenterX - shape.Size / 2, subCenterY)
                                };
                                        if (shape.IsFilled)
                                        {
                                            g.FillPolygon(brush, diamondPoints);
                                        }
                                        else
                                        {
                                            g.DrawPolygon(pen, diamondPoints);
                                        }
                                        break;
                                    case 3: // 十字架
                                        int crossSize = shape.Size / 3;
                                        if (shape.IsFilled)
                                        {
                                            g.FillRectangle(brush, subCenterX - crossSize / 2, subCenterY - shape.Size / 2, crossSize, shape.Size);
                                            g.FillRectangle(brush, subCenterX - shape.Size / 2, subCenterY - crossSize / 2, shape.Size, crossSize);
                                        }
                                        else
                                        {
                                            g.DrawRectangle(pen, subCenterX - crossSize / 2, subCenterY - shape.Size / 2, crossSize, shape.Size);
                                            g.DrawRectangle(pen, subCenterX - shape.Size / 2, subCenterY - crossSize / 2, shape.Size, crossSize);
                                        }
                                        break;
                                }
                                brush.Dispose();
                                pen.Dispose();
                            }
                        }
                    }
                }
            }
            return bmp;
        }

        // 辅助方法：获取随机颜色
        private static Color GetRandomColor()
        {
            Random rnd = new Random();
            return Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        }

        private static void DrawCircle(Graphics g, int centerX, int centerY, int radius)
        {
            g.DrawEllipse(Pens.Blue, centerX - radius, centerY - radius, 2 * radius, 2 * radius);
        }

        private static void DrawSquare(Graphics g, int centerX, int centerY, int halfSize)
        {
            g.DrawRectangle(Pens.Blue, centerX - halfSize, centerY - halfSize, 2 * halfSize, 2 * halfSize);
        }

        private static void DrawRotatedSquare(Graphics g, int centerX, int centerY, int halfSize)
        {
            Matrix matrix = new Matrix();
            matrix.RotateAt(45, new PointF(centerX, centerY));
            g.Transform = matrix;

            g.DrawRectangle(Pens.Blue, centerX - halfSize, centerY - halfSize, 2 * halfSize, 2 * halfSize);

            g.ResetTransform();
        }

        private static void DrawCross(Graphics g, int centerX, int centerY, int halfSize)
        {
            g.DrawLine(Pens.Blue, centerX - halfSize, centerY, centerX + halfSize, centerY);
            g.DrawLine(Pens.Blue, centerX, centerY - halfSize, centerX, centerY + halfSize);
        }

        private static void DrawRotatedCross(Graphics g, int centerX, int centerY, int halfSize)
        {
            Matrix matrix = new Matrix();
            matrix.RotateAt(45, new PointF(centerX, centerY));
            g.Transform = matrix;

            g.DrawLine(Pens.Blue, centerX - halfSize, centerY, centerX + halfSize, centerY);
            g.DrawLine(Pens.Blue, centerX, centerY - halfSize, centerX, centerY + halfSize);

            g.ResetTransform();
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