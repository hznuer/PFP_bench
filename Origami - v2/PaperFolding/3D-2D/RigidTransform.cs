using System;
using Accord.Math;
using Accord.Math.Decompositions;

public class RigidTransform
{
    /// <summary>
    /// 通过3对三维对应点计算变换的4x4齐次矩阵（旋转+平移）
    /// </summary>
    /// <param name="sourcePoints">源点集（3个三维点）</param>
    /// <param name="targetPoints">目标点集（3个三维点）</param>
    /// <returns>4x4齐次变换矩阵</returns>
    public static double[,] Calculate3DHomogeneousMatrix(Vector3[] sourcePoints, Vector3[] targetPoints)
    {
        // 输入验证
        if (sourcePoints == null || targetPoints == null
            || sourcePoints.Length != 3 || targetPoints.Length != 3)
        {
            throw new ArgumentException("必须提供3对对应点（Vector3数组长度为3）");
        }

        // 1. 计算质心
        Vector3 sourceCentroid = CalculateCentroid(sourcePoints);
        Vector3 targetCentroid = CalculateCentroid(targetPoints);

        // 2. 去中心化
        Vector3[] sourceCentered = new Vector3[3];
        Vector3[] targetCentered = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            sourceCentered[i] = Subtract(sourcePoints[i], sourceCentroid);
            targetCentered[i] = Subtract(targetPoints[i], targetCentroid);
        }

        // 3. 构建3x3协方差矩阵H
        double[,] H = new double[3, 3];
        for (int i = 0; i < 3; i++)
        {
            H[0, 0] += sourceCentered[i].X * targetCentered[i].X;
            H[0, 1] += sourceCentered[i].X * targetCentered[i].Y;
            H[0, 2] += sourceCentered[i].X * targetCentered[i].Z;

            H[1, 0] += sourceCentered[i].Y * targetCentered[i].X;
            H[1, 1] += sourceCentered[i].Y * targetCentered[i].Y;
            H[1, 2] += sourceCentered[i].Y * targetCentered[i].Z;

            H[2, 0] += sourceCentered[i].Z * targetCentered[i].X;
            H[2, 1] += sourceCentered[i].Z * targetCentered[i].Y;
            H[2, 2] += sourceCentered[i].Z * targetCentered[i].Z;
        }

        // 4. 对H进行SVD分解
        SingularValueDecomposition svd = new SingularValueDecomposition(H,
            computeLeftSingularVectors: true,
            computeRightSingularVectors: true,
            autoTranspose: false);

        double[,] U = svd.LeftSingularVectors;
        double[,] V = svd.RightSingularVectors;
        double[,] VT = Matrix.Transpose(V); // V的转置

        // 5. 计算旋转矩阵R = V·U^T
        double[,] rotation = Matrix.Multiply(VT, Matrix.Transpose(U));

        // 处理反射情况（行列式为负）
        if (Matrix.Determinant(rotation) < 0)
        {
            // 将V的最后一列取反
            for (int i = 0; i < 3; i++)
                V[i, 2] = -V[i, 2];

            rotation = Matrix.Multiply(Matrix.Transpose(V), Matrix.Transpose(U));
        }

        // 6. 计算平移向量t = targetCentroid - R·sourceCentroid
        Vector3 rotatedSourceCentroid = TransformPoint(sourceCentroid, rotation);
        Vector3 translation = Subtract(targetCentroid, rotatedSourceCentroid);

        // 7. 构建4x4齐次变换矩阵
        double[,] homogeneous = Matrix.Identity(4);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                homogeneous[i, j] = rotation[i, j];

        homogeneous[0, 3] = translation.X;
        homogeneous[1, 3] = translation.Y;
        homogeneous[2, 3] = translation.Z;

        return homogeneous;
    }

    // 辅助方法：计算点集的质心
    private static Vector3 CalculateCentroid(Vector3[] points)
    {
        float sumX = 0, sumY = 0, sumZ = 0;
        foreach (var p in points)
        {
            sumX += p.X;
            sumY += p.Y;
            sumZ += p.Z;
        }

        int count = points.Length;
        return new Vector3(sumX / count, sumY / count, sumZ / count);
    }

    // 计算从 norm1 到 norm2 的旋转矩阵
    public static double[,] CalculateRotationMatrix(Vector3 norm1, Vector3 norm2)
    {
        norm1 = Normalize(norm1);
        norm2 = Normalize(norm2);

        float dot = Vector3.Dot(norm1, norm2);

        // 处理平行法线的特殊情况
        if (Math.Abs(dot + 1) < 1e-6)
        {
            // norm1 和 norm2 方向相反，需要180度旋转
            Vector3 rotationAxis = Vector3.Cross(norm1, CreateUnitX());
            if (GetLength(rotationAxis) < 1e-6)
                rotationAxis = Vector3.Cross(norm1, CreateUnitY());

            rotationAxis = Normalize(rotationAxis);
            return CreateRotationMatrixFromAxisAngle(rotationAxis, Math.PI);
        }

        if (Math.Abs(dot - 1) < 1e-6)
        {
            // norm1 和 norm2 方向相同，无需旋转
            return Matrix.Identity(3);
        }

        // 计算旋转轴和角度
        Vector3 rotationAxis2 = Vector3.Cross(norm1, norm2);
        float rotationAngle = (float)Math.Acos(dot);

        // 使用罗德里格斯公式构建旋转矩阵
        return CreateRotationMatrixFromAxisAngle(rotationAxis2, rotationAngle);
    }

    // 创建基于轴角的旋转矩阵
    private static double[,] CreateRotationMatrixFromAxisAngle(Vector3 axis, double angle)
    {
        axis = Normalize(axis);
        double x = axis.X, y = axis.Y, z = axis.Z;
        double c = Math.Cos(angle);
        double s = Math.Sin(angle);
        double t = 1 - c;

        // 罗德里格斯旋转公式实现
        return new double[,] {
            { t * x * x + c,     t * x * y - s * z, t * x * z + s * y },
            { t * x * y + s * z, t * y * y + c,     t * y * z - s * x },
            { t * x * z - s * y, t * y * z + s * x, t * z * z + c     }
        };
    }

    // 计算完整的齐次变换矩阵
    public static double[,] CalculateHomogeneousMatrix(Vector3 norm1, Vector3 pt1, Vector3 norm2, Vector3 pt2)
    {
        // 计算旋转矩阵
        double[,] rotation = CalculateRotationMatrix(norm1, norm2);

        // 计算平移向量
        Vector3 rotatedPt1 = TransformPoint(pt1, rotation);
        Vector3 translation = Subtract(pt2, rotatedPt1);

        // 构建4x4齐次变换矩阵
        double[,] homogeneous = Matrix.Identity(4);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                homogeneous[i, j] = rotation[i, j];

        homogeneous[0, 3] = translation.X;
        homogeneous[1, 3] = translation.Y;
        homogeneous[2, 3] = translation.Z;

        return homogeneous;
    }

    // 计算逆变换矩阵
    public static double[,] CalculateInverseMatrix(double[,] matrix)
    {
        // 提取旋转矩阵并求逆（转置）
        double[,] rotation = Matrix.Submatrix(matrix, 0, 2, 0, 2);
        double[,] rotationInverse = Matrix.Transpose(rotation);

        // 提取平移向量
        Vector3 translation = new Vector3(
            (float)matrix[0, 3],
            (float)matrix[1, 3],
            (float)matrix[2, 3]
        );

        // 计算逆平移向量
        Vector3 inverseTranslation = Multiply(-1, TransformPoint(translation, rotationInverse));

        // 构建逆矩阵
        double[,] inverse = Matrix.Identity(4);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                inverse[i, j] = rotationInverse[i, j];

        inverse[0, 3] = inverseTranslation.X;
        inverse[1, 3] = inverseTranslation.Y;
        inverse[2, 3] = inverseTranslation.Z;

        return inverse;
    }

    // 使用矩阵变换点
    private static Vector3 TransformPoint(Vector3 point, double[,] matrix)
    {
        double x = point.X * matrix[0, 0] + point.Y * matrix[0, 1] + point.Z * matrix[0, 2];
        double y = point.X * matrix[1, 0] + point.Y * matrix[1, 1] + point.Z * matrix[1, 2];
        double z = point.X * matrix[2, 0] + point.Y * matrix[2, 1] + point.Z * matrix[2, 2];

        return new Vector3((float)x, (float)y, (float)z);
    }

    // 辅助方法：向量归一化
    private static Vector3 Normalize(Vector3 vector)
    {
        float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        if (length < 1e-6) return new Vector3(0, 0, 0);
        return new Vector3(vector.X / length, vector.Y / length, vector.Z / length);
    }

    // 辅助方法：获取向量长度
    private static float GetLength(Vector3 vector)
    {
        return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
    }

    // 辅助方法：创建单位向量
    private static Vector3 CreateUnitX()
    {
        return new Vector3(1, 0, 0);
    }

    // 辅助方法：创建单位向量
    private static Vector3 CreateUnitY()
    {
        return new Vector3(0, 1, 0);
    }

    // 辅助方法：向量减法
    private static Vector3 Subtract(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    // 辅助方法：向量数乘
    private static Vector3 Multiply(float scalar, Vector3 vector)
    {
        return new Vector3(scalar * vector.X, scalar * vector.Y, scalar * vector.Z);
    }
}