using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

// 外层图形类型（不含三角形）
public enum OuterShape { Pentagon, Diamond, Hexagon, Circle, Square }
// 内部图元类型（包含45度正方形和十字架）
public enum InnerShape
{
    Triangle, Circle, Square, Cross, RotatedSquare, RotatedCross
}

// 范围辅助类
public class Range<T> where T : IComparable<T>
{
    public T Min { get; set; }
    public T Max { get; set; }
    public Range(T min, T max) { Min = min; Max = max; }
}

// 外框内接区域信息
public class InscribedArea
{
    public float Radius { get; set; } // 内接圆半径
    public RectangleF Bounds { get; set; } // 内接矩形
}

// 生成选项配置
public class ShapeGenerationOptions
{
    private int _size = 300; // 增大画布尺寸便于布局
    private Range<int> _outerLineWidth = new Range<int>(5, 13);
    private Range<int> _innerCount = new Range<int>(2, 5); // 增加内部图元数量以便测试布局
    private Range<float> _innerSizeRatio = new Range<float>(0.1f, 0.25f); // 缩小比例减少超出风险
    private bool _allowInnerFilledRandom = true;
    private List<OuterShape> _availableOuterShapes = new List<OuterShape>
    {
        OuterShape.Pentagon, OuterShape.Diamond, OuterShape.Hexagon,
        OuterShape.Circle, OuterShape.Square
    };
    private List<InnerShape> _availableInnerShapes = new List<InnerShape>
    {
        InnerShape.Triangle, InnerShape.Circle, InnerShape.Square, 
        InnerShape.Cross, InnerShape.RotatedSquare, InnerShape.RotatedCross
    };
    private Range<float> _outerSizeRatio = new Range<float>(0.85f, 0.95f);
    private Range<int> _outerExistsProbability = new Range<int>(85, 100);
    private Range<float> _crossLineThicknessRatio = new Range<float>(0.2f, 0.3f);
    private int _minInnerOutlineWidth = 7;
    private float _innerSpacingRatio = 0.1f; // 内部图元间距比例

    /// <summary>内部图元之间的最小间距比例（相对于图元平均尺寸）</summary>
    public float InnerSpacingRatio
    {
        get { return _innerSpacingRatio; }
        set { _innerSpacingRatio = Math.Max(0.05f, Math.Min(value, 0.3f)); }
    }

    // 其他属性保持不变
    public int Size { get { return _size; } set { _size = value; } }
    public Range<int> OuterLineWidth { get { return _outerLineWidth; } set { _outerLineWidth = value; } }
    public Range<int> InnerCount { get { return _innerCount; } set { _innerCount = value; } }
    public Range<float> InnerSizeRatio { get { return _innerSizeRatio; } set { _innerSizeRatio = value; } }
    public bool AllowInnerFilledRandom { get { return _allowInnerFilledRandom; } set { _allowInnerFilledRandom = value; } }
    public List<OuterShape> AvailableOuterShapes { get { return _availableOuterShapes; } set { _availableOuterShapes = value; } }
    public List<InnerShape> AvailableInnerShapes { get { return _availableInnerShapes; } set { _availableInnerShapes = value; } }
    public Range<float> OuterSizeRatio { get { return _outerSizeRatio; } set { _outerSizeRatio = value; } }
    public Range<int> OuterExistsProbability { get { return _outerExistsProbability; } set { _outerExistsProbability = value; } }
    public Range<float> CrossLineThicknessRatio { get { return _crossLineThicknessRatio; } set { _crossLineThicknessRatio = value; } }
    public int MinInnerOutlineWidth { get { return _minInnerOutlineWidth; } set { _minInnerOutlineWidth = Math.Max(value, 7); } }
}

// 随机数扩展
public static class RandomExtensions
{
    public static float NextFloat(this Random random, Range<float> range)
    {
        return (float)random.NextDouble() * (range.Max - range.Min) + range.Min;
    }
    public static int NextInt(this Random random, Range<int> range)
    {
        return random.Next(range.Min, range.Max + 1);
    }
    // 生成指定范围内的随机角度（弧度）
    public static float NextAngle(this Random random, float minRadians = 0, float maxRadians = (float)(2 * Math.PI))
    {
        return (float)random.NextDouble() * (maxRadians - minRadians) + minRadians;
    }
}

// 图形生成器核心类
public class ShapeRenderer
{
    private readonly Random _random;

    public ShapeRenderer() { _random = new Random(); }
    public ShapeRenderer(int seed) { _random = new Random(seed); }

    /// <summary>生成指定配置的图形</summary>
    public Bitmap Generate(ShapeGenerationOptions options)
    {
        Bitmap bmp = new Bitmap(options.Size, options.Size);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            // 1. 基础参数设置
            bool outerExists = _random.Next(100) < _random.NextInt(options.OuterExistsProbability);
            int outerLineWidth = _random.NextInt(options.OuterLineWidth);
            OuterShape outerShape = options.AvailableOuterShapes[_random.Next(options.AvailableOuterShapes.Count)];
            int innerCount = _random.NextInt(options.InnerCount);
            innerCount = Math.Max(innerCount, outerExists ? 1 : 2); // 无外框至少2个
            float outerSize = options.Size * _random.NextFloat(options.OuterSizeRatio);
            PointF center = new PointF(options.Size / 2f, options.Size / 2f);

            // 2. 计算外框及其内接区域（核心改进：精确限制内部空间）
            PointF[] outerPolygonPoints = null;
            InscribedArea inscribedArea = new InscribedArea();
            if (outerExists)
            {
                using (Pen outerPen = new Pen(Color.Black, outerLineWidth))
                {
                    outerPen.Alignment = PenAlignment.Inset;
                    outerPolygonPoints = DrawOuterShape(g, outerPen, outerShape, center, outerSize);
                }
                inscribedArea = CalculateInscribedArea(outerShape, center, outerSize, outerPolygonPoints);
            }
            else
            {
                // 无外框时使用画布内接区域
                inscribedArea.Radius = options.Size * 0.45f;
                inscribedArea.Bounds = new RectangleF(
                    center.X - inscribedArea.Radius,
                    center.Y - inscribedArea.Radius,
                    inscribedArea.Radius * 2,
                    inscribedArea.Radius * 2
                );
            }

            // 3. 计算内部图元尺寸（基于内接区域限制）
            float baseInnerSize = inscribedArea.Radius * 0.6f; // 最大尺寸不超过内接圆60%
            float innerSize = Math.Max(
                options.MinInnerOutlineWidth,
                baseInnerSize * _random.NextFloat(options.InnerSizeRatio)
            );
            float minSpacing = innerSize * options.InnerSpacingRatio; // 最小间距

            // 4. 生成内部图元位置（确保在內接区域内且不重叠）
            List<PointF> innerPositions = GenerateNonOverlappingPositions(
                center, inscribedArea, innerCount, innerSize, minSpacing
            );

            // 5. 绘制内部图元
            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (Pen outlinePen = new Pen(Color.Black, options.MinInnerOutlineWidth))
            {
                float crossThickness = Math.Max(innerSize * _random.NextFloat(options.CrossLineThicknessRatio), options.MinInnerOutlineWidth);
                foreach (var pos in innerPositions)
                {
                    // 随机选择内部图元类型
                    InnerShape innerShape = options.AvailableInnerShapes[_random.Next(options.AvailableInnerShapes.Count)];
                    bool filled = options.AllowInnerFilledRandom && _random.Next(2) == 0;

                    DrawInnerShape(g, brush, outlinePen, innerShape, pos, innerSize, filled, crossThickness);
                }
            }
        }
        return bmp;
    }

    /// <summary>计算外框的内接区域（内接圆和内接矩形）</summary>
    private InscribedArea CalculateInscribedArea(OuterShape shape, PointF center, float outerSize, PointF[] polygonPoints)
    {
        var area = new InscribedArea();
        float halfSize = outerSize / 2f;

        switch (shape)
        {
            case OuterShape.Circle:
                // 圆形内接圆即为自身（减去线宽）
                area.Radius = halfSize * 0.9f;
                area.Bounds = new RectangleF(
                    center.X - area.Radius,
                    center.Y - area.Radius,
                    area.Radius * 2,
                    area.Radius * 2
                );
                break;

            case OuterShape.Square:
                // 正方形内接圆半径 = 边长/2 * √2/2 ≈ 0.3535*边长
                area.Radius = halfSize * (float)Math.Sqrt(2) / 2 * 0.9f;
                area.Bounds = new RectangleF(
                    center.X - area.Radius,
                    center.Y - area.Radius,
                    area.Radius * 2,
                    area.Radius * 2
                );
                break;

            case OuterShape.Diamond:
                // 菱形（旋转正方形）内接圆半径 = 对角线/4
                area.Radius = halfSize * 0.5f * 0.9f;
                area.Bounds = new RectangleF(
                    center.X - area.Radius,
                    center.Y - area.Radius,
                    area.Radius * 2,
                    area.Radius * 2
                );
                break;

            case OuterShape.Hexagon:
                // 正六边形内接圆半径 = 边长
                area.Radius = halfSize * 0.9f;
                area.Bounds = new RectangleF(
                    center.X - area.Radius,
                    center.Y - area.Radius * 0.866f, // 六边形高度是半径的√3/2 ≈0.866倍
                    area.Radius * 2,
                    area.Radius * 1.732f
                );
                break;

            case OuterShape.Pentagon:
                // 正五边形内接圆半径约为外接圆的0.809倍
                area.Radius = halfSize * 0.809f * 0.9f;
                area.Bounds = new RectangleF(
                    center.X - area.Radius,
                    center.Y - area.Radius * 0.951f, // 五边形高度系数
                    area.Radius * 2,
                    area.Radius * 1.902f
                );
                break;
        }

        return area;
    }

    /// <summary>生成不重叠且在內接区域内的内部图元位置</summary>
    private List<PointF> GenerateNonOverlappingPositions(
        PointF center, InscribedArea inscribedArea, int count, float elementSize, float minSpacing)
    {
        List<PointF> positions = new List<PointF>();
        float safeRadius = inscribedArea.Radius - elementSize / 2; // 确保图元边缘不超出
        float minDistance = elementSize + minSpacing; // 最小间距（包含自身尺寸）

        // 如果只有一个元素，放在中心
        if (count == 1)
        {
            positions.Add(center);
            return positions;
        }

        // 尝试生成位置（最多尝试100次避免死循环）
        for (int i = 0; i < count; i++)
        {
            bool foundValidPosition = false;
            for (int attempt = 0; attempt < 100; attempt++)
            {
                // 在內接圆内随机生成位置（带一定随机性的放射状分布）
                float angle = _random.NextAngle();
                float distance = _random.NextFloat(new Range<float>(safeRadius * 0.3f, safeRadius));
                PointF candidate = new PointF(
                    center.X + (float)Math.Cos(angle) * distance,
                    center.Y + (float)Math.Sin(angle) * distance
                );

                // 检查是否在內接矩形内（双重保险）
                if (!inscribedArea.Bounds.Contains(
                    new RectangleF(candidate.X - elementSize / 2, candidate.Y - elementSize / 2,
                                  elementSize, elementSize)))
                {
                    continue;
                }

                // 检查是否与已有位置重叠
                bool overlap = false;
                foreach (var existing in positions)
                {
                    if (Distance(candidate, existing) < minDistance)
                    {
                        overlap = true;
                        break;
                    }
                }

                if (!overlap)
                {
                    positions.Add(candidate);
                    foundValidPosition = true;
                    break;
                }
            }

            // 如果尝试多次仍未找到位置，放宽条件（确保能生成）
            if (!foundValidPosition)
            {
                float angle = (float)(i * 2 * Math.PI / count);
                float distance = safeRadius * 0.5f;
                PointF fallbackPos = new PointF(
                    center.X + (float)Math.Cos(angle) * distance,
                    center.Y + (float)Math.Sin(angle) * distance
                );
                positions.Add(fallbackPos);
            }
        }

        return positions;
    }

    #region 绘制方法与辅助函数
    private PointF[] DrawOuterShape(Graphics g, Pen pen, OuterShape shape, PointF center, float size)
    {
        switch (shape)
        {
            case OuterShape.Pentagon:
                var pentagon = GetPentagonPoints(center, size);
                g.DrawPolygon(pen, pentagon);
                return pentagon;
            case OuterShape.Diamond:
                var diamond = GetDiamondPoints(center, size);
                g.DrawPolygon(pen, diamond);
                return diamond;
            case OuterShape.Hexagon:
                var hexagon = GetHexagonPoints(center, size);
                g.DrawPolygon(pen, hexagon);
                return hexagon;
            case OuterShape.Circle:
                float circleOffset = pen.Width / 2;
                g.DrawEllipse(pen,
                            center.X - size / 2 + circleOffset,
                            center.Y - size / 2 + circleOffset,
                            size - pen.Width,
                            size - pen.Width);
                return null;
            case OuterShape.Square:
                float squareOffset = pen.Width / 2;
                var square = new PointF[] {
                    new PointF(center.X - size/2 + squareOffset, center.Y - size/2 + squareOffset),
                    new PointF(center.X + size/2 - squareOffset, center.Y - size/2 + squareOffset),
                    new PointF(center.X + size/2 - squareOffset, center.Y + size/2 - squareOffset),
                    new PointF(center.X - size/2 + squareOffset, center.Y + size/2 - squareOffset)
                };
                g.DrawPolygon(pen, square);
                return square;
            default:
                return null;
        }
    }

    private void DrawInnerShape(Graphics g, SolidBrush brush, Pen outlinePen, InnerShape shape,
                              PointF center, float size, bool filled, float crossThickness)
    {
        // 计算图元的边界半径（用于碰撞检测的最大距离）
        float boundsRadius = size / 2f;

        switch (shape)
        {
            case InnerShape.Triangle:
                PointF[] triPoints = GetTrianglePoints(center, size);
                if (filled) g.FillPolygon(brush, triPoints);
                else g.DrawPolygon(outlinePen, triPoints);
                break;
            case InnerShape.Circle:
                RectangleF circleRect = new RectangleF(center.X - size / 2, center.Y - size / 2, size, size);
                if (filled) g.FillEllipse(brush, circleRect);
                else g.DrawEllipse(outlinePen, circleRect);
                break;
            case InnerShape.Square:
                RectangleF squareRect = new RectangleF(center.X - size / 2, center.Y - size / 2, size, size);
                if (filled) g.FillRectangle(brush, squareRect);
                else g.DrawRectangle(outlinePen, squareRect.X, squareRect.Y, squareRect.Width, squareRect.Height);
                break;
            case InnerShape.Cross:
                using (Pen crossPen = new Pen(brush.Color, crossThickness))
                {
                    g.DrawLine(crossPen, center.X - size / 2, center.Y, center.X + size / 2, center.Y);
                    g.DrawLine(crossPen, center.X, center.Y - size / 2, center.X, center.Y + size / 2);
                }
                break;
            case InnerShape.RotatedSquare:
                PointF[] rotatedSquare = GetDiamondPoints(center, size * 0.8f);
                if (filled) g.FillPolygon(brush, rotatedSquare);
                else g.DrawPolygon(outlinePen, rotatedSquare);
                break;
            case InnerShape.RotatedCross:
                using (Pen crossPen = new Pen(brush.Color, crossThickness))
                {
                    float halfDiagonal = size / 2f * (float)Math.Cos(Math.PI / 4);
                    g.DrawLine(crossPen, center.X - halfDiagonal, center.Y - halfDiagonal,
                                       center.X + halfDiagonal, center.Y + halfDiagonal);
                    g.DrawLine(crossPen, center.X + halfDiagonal, center.Y - halfDiagonal,
                                       center.X - halfDiagonal, center.Y + halfDiagonal);
                }
                break;
        }
    }

    // 计算两点距离
    private float Distance(PointF p1, PointF p2)
    {
        float dx = p1.X - p2.X;
        float dy = p1.Y - p2.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    // 多边形顶点生成方法（保持不变）
    private PointF[] GetPentagonPoints(PointF center, float size)
    {
        float radius = size * 0.45f;
        PointF[] points = new PointF[5];
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f * (float)Math.PI / 180f;
            points[i] = new PointF(
                center.X + radius * (float)Math.Cos(angle),
                center.Y + radius * (float)Math.Sin(angle)
            );
        }
        return points;
    }

    private PointF[] GetDiamondPoints(PointF center, float size)
    {
        float halfSize = size / 2f * 0.95f;
        return new PointF[] {
            new PointF(center.X, center.Y - halfSize),
            new PointF(center.X + halfSize, center.Y),
            new PointF(center.X, center.Y + halfSize),
            new PointF(center.X - halfSize, center.Y)
        };
    }

    private PointF[] GetHexagonPoints(PointF center, float size)
    {
        float radius = size * 0.45f;
        PointF[] points = new PointF[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * (float)Math.PI / 180f;
            points[i] = new PointF(
                center.X + radius * (float)Math.Cos(angle),
                center.Y + radius * (float)Math.Sin(angle)
            );
        }
        return points;
    }

    private PointF[] GetTrianglePoints(PointF center, float size)
    {
        float radius = size * 0.45f;
        PointF[] points = new PointF[3];
        for (int i = 0; i < 3; i++)
        {
            float angle = (i * 120f - 90f) * (float)Math.PI / 180f;
            points[i] = new PointF(
                center.X + radius * (float)Math.Cos(angle),
                center.Y + radius * (float)Math.Sin(angle)
            );
        }
        return points;
    }
    #endregion
}
