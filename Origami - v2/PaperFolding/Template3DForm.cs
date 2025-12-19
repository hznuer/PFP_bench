using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpGL;
using SharpGL.Enumerations;
using Accord.Math;
using Tao.OpenGl;
using Tao.Platform.Windows;
using Tao.FreeGlut;


namespace Origami
{
    public partial class Template3DForm : Form
    {
        private double[,] currentTransform = Matrix.Identity(4);  // 当前变换矩阵
        private float rotationX = 0.0f;     // X轴旋转角度
        private float rotationY = 0.0f;     // Y轴旋转角度
        private float opacity = 0.8f;       // 立方体透明度
        private Point lastMousePosition;    // 鼠标最后位置
        private bool isDragging = false;    // 是否正在拖动
        private TextureGenerator textureGenerator;

        private bool _isExporting = false;
        private bool _stopExport = false;
        private Export3DSetting _exportSetting = new Export3DSetting();
        private int[] _faceTexIdx = new int[] { 0, 1, 2, 3, 4, 5 };
        string[] _exts = new string[] { ".png", ".jpg", ".bmp" };


        // 立方体的12种不同位姿（使用Accord.NET矩阵）
        private static readonly List<double[,]> cubePositions = new List<double[,]>
        {
            // 6个面分别朝前的位姿// 每个面朝前时，上下翻转的位姿
            CreateRotationMatrix(0, 0, 0),         // 前面朝前  1
            CreateRotationMatrix(0, 0, 90),       // 前面朝前 +
            CreateRotationMatrix(0, 0, 180),       // 前面朝前 + 上下翻转
            CreateRotationMatrix(0, 0, 270),       // 前面朝前 +

            CreateRotationMatrix(0, -90, 180),     // 左面朝前   2
            CreateRotationMatrix(0, 90, 0),      //  
            CreateRotationMatrix(90, 0, 270),      //  
            CreateRotationMatrix(-90, 0, 90),      //  


            CreateRotationMatrix(0, 180, 0),       // 后面朝前  3
            CreateRotationMatrix(0, 180, 90),     //  后面朝前 +  
            CreateRotationMatrix(0, 180, 180),     // 后面朝前 + 
            CreateRotationMatrix(0, 180, 270),     // 后面朝前 +  

            CreateRotationMatrix(0, -90, 0),       // 右面朝前   4
            CreateRotationMatrix(0, 90, 180),      //  
            CreateRotationMatrix(90, 0, 90),      //  
            CreateRotationMatrix(-90, 0, 270),      //  

            CreateRotationMatrix(0, -90, 270),     // 上面朝前 +  5
            CreateRotationMatrix(0, 90, 90),      //    
            CreateRotationMatrix(90, 0, 0),        //  
            CreateRotationMatrix(-90, 0, 180),      //  


            CreateRotationMatrix(0, -90, 90),        // 下面朝前   6
            CreateRotationMatrix(0, 90, 270),      //  
            CreateRotationMatrix(90, 0, 180),      //  
            CreateRotationMatrix(-90, 0, 0),       // 

        };

        // 创建旋转矩阵的辅助方法（修正版）
        private static double[,] CreateRotationMatrix(double x, double y, double z)
        {
            // 将角度转换为弧度
            double radX = x * Math.PI / 180.0;
            double radY = y * Math.PI / 180.0;
            double radZ = z * Math.PI / 180.0;

            // 创建绕X轴的旋转矩阵
            double[,] rotX = Matrix.Identity(4);
            rotX[1, 1] = Math.Cos(radX);
            rotX[1, 2] = -Math.Sin(radX);
            rotX[2, 1] = Math.Sin(radX);
            rotX[2, 2] = Math.Cos(radX);

            // 创建绕Y轴的旋转矩阵
            double[,] rotY = Matrix.Identity(4);
            rotY[0, 0] = Math.Cos(radY);
            rotY[0, 2] = -Math.Sin(radY);
            rotY[2, 0] = Math.Sin(radY);
            rotY[2, 2] = Math.Cos(radY);

            // 创建绕Z轴的旋转矩阵
            double[,] rotZ = Matrix.Identity(4);
            rotZ[0, 0] = Math.Cos(radZ);
            rotZ[0, 1] = Math.Sin(radZ);
            rotZ[1, 0] = -Math.Sin(radZ);
            rotZ[1, 1] = Math.Cos(radZ);

            // 修正矩阵乘法顺序：Y * X * Z（与OpenGL的glRotate顺序一致）
            return Matrix.Dot(Matrix.Dot(rotY, rotX), rotZ);
        }

        // 将Accord.NET矩阵应用到OpenGL（修正版）
        private void ApplyMatrix(double[,] matrix)
        {
            float[] glMatrix = new float[16]
            {
                (float)matrix[0, 0], (float)matrix[1, 0], (float)matrix[2, 0], (float)matrix[3, 0],
                (float)matrix[0, 1], (float)matrix[1, 1], (float)matrix[2, 1], (float)matrix[3, 1],
                (float)matrix[0, 2], (float)matrix[1, 2], (float)matrix[2, 2], (float)matrix[3, 2],
                (float)matrix[0, 3], (float)matrix[1, 3], (float)matrix[2, 3], (float)matrix[3, 3]
            };

            Gl.glMultMatrixf(glMatrix);
        }

        public Template3DForm()
        {
            InitializeComponent();
        }

        private Export3DSetting GetExportSetting()
        {
            Export3DSetting setting = new Export3DSetting();

            if (checkExportPure2D.Checked)
            {
                _3DExportParam param = new _3DExportParam();
                param._exportNum = GlobalUtils.IntParse(textNum2D.Text);
                param._exportType = _3DExportType._2D;
                setting._templateList.Add(param);
            }

            if (checkExportPure3D.Checked)
            {
                _3DExportParam param = new _3DExportParam();
                param._exportNum = GlobalUtils.IntParse(textNum3D.Text);
                param._exportType = _3DExportType._3D;
                setting._templateList.Add(param);
            }

            if (checkExport2DTo3D.Checked)
            {
                _3DExportParam param = new _3DExportParam();
                param._exportNum = GlobalUtils.IntParse(textNum2DTo3D.Text);
                param._exportType = _3DExportType._2DTo3D_Y;
                setting._templateList.Add(param);
            }

            if (checkExport2DTo3D2.Checked)
            {
                _3DExportParam param = new _3DExportParam();
                param._exportNum = GlobalUtils.IntParse(textNum2DTo3D2.Text);
                param._exportType = _3DExportType._2DTo3D_N;
                setting._templateList.Add(param);
            }
            if (checkExport3DTo2D.Checked)
            {
                _3DExportParam param = new _3DExportParam();
                param._exportNum = GlobalUtils.IntParse(textNum3DTo2D.Text);
                param._exportType = _3DExportType._3DTo2D_Y;
                setting._templateList.Add(param);
            }
            if (checkExport3DTo2D2.Checked)
            {
                _3DExportParam param = new _3DExportParam();
                param._exportNum = GlobalUtils.IntParse(textNum3DTo2D2.Text);
                param._exportType = _3DExportType._3DTo2D_N;
                setting._templateList.Add(param);
            }

            if (checkImageBasic.Checked)
                setting._faceImgTypeList.Add(_3DFaceImageType.Basic);
            if(checkImageChar.Checked)
                setting._faceImgTypeList.Add(_3DFaceImageType.Char);
            if (checkImageLine.Checked)
                setting._faceImgTypeList.Add(_3DFaceImageType.Line);
            if (checkImageMosaic.Checked)
                setting._faceImgTypeList.Add(_3DFaceImageType.Mosaic);
            if (checkImagePattern.Checked)
                setting._faceImgTypeList.Add(_3DFaceImageType.Pattern);
            if (checkImageShape.Checked)
                setting._faceImgTypeList.Add(_3DFaceImageType.Shape);


            if (checkImageSymmetric.Checked)
                setting._faceImgTypeList.Add(_3DFaceImageType.Symmetric);

            setting._opacity = GlobalUtils.FloatParse(textOpacity.Text);
            setting._supportMulti3D = checkMulti3D.Checked;

            setting._path = textPath.Text;
            setting._ext = comboFileExt.SelectedItem.ToString();
            return setting;
 
        }

        private void Template3DForm_Load(object sender, EventArgs e)
        {
            splitContainer2.SplitterDistance = splitContainer2.Parent.Height * 4 / 5;
            trackBar1.Value = (int)(opacity * 100);

            for (int i = 0; i < cubePositions.Count; i++)
                comboBox1.Items.Add(i + 1);
            comboBox1.SelectedIndex = 0;

            InitializeGLControl();

            Bitmap[] bmps = GenerateRandBmps();

            InitTexture(bmps);
            foreach (Bitmap bmp in bmps)
                bmp.Dispose();

            for (int i = 0; i < CubeNet.GroupNets.Count; i++)
            {
                comboBox2.Items.Add(CubeNet.GroupNets[i][0].Name);
                comboBox3.Items.Add(CubeNet.GroupNets[i][0].Name);
            }
            comboBox3.Items.Add("混合布局");
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;


            foreach (string ext in _exts)
                comboFileExt.Items.Add(ext);
            comboFileExt.SelectedIndex = 0;

            string path = Application.StartupPath + "\\Datasets";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "\\_3D";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "\\" + DateTime.Now.ToString("yyyy-MM-dd");//_HH-mm-ss");
            DateTime now = DateTime.Now;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            textPath.Text = path;

        }

        private void InitializeGLControl()
        {
            glControl1.InitializeContexts();
            // 绑定事件
            glControl1.Paint += glControl1_Paint;
            glControl1.Load += glControl1_Load;
            glControl1.Resize += glControl1_Resize;
            glControl1.MouseDown += glControl1_MouseDown;
            glControl1.MouseMove += glControl1_MouseMove;
            glControl1.MouseUp += glControl1_MouseUp;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl1.IsHandleCreated) return;

            // 清除颜色缓冲区和深度缓冲区
            Gl.glClearColor(1, 1, 1, 1);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            //if (true)
            {
                // 设置模型视图矩阵
                Gl.glMatrixMode(Gl.GL_MODELVIEW);
                Gl.glLoadIdentity();

                // 设置观察点（使用Glu库的LookAt）
                Glu.gluLookAt(0, 0, 5, 0, 0, 0, 0, 1, 0);

                // 应用斜角投影变换
                ApplyObliqueProjection();

                // 应用用户旋转
                Gl.glRotatef(rotationX, 1, 0, 0);
                Gl.glRotatef(rotationY, 0, 1, 0);

                // 应用当前变换矩阵
                ApplyMatrix(currentTransform);

                // 绘制透明立方体
                //     DrawTransparentCube();
             //   Bitmap[] bmps = GenerateRandBmps();// RandomImageGenerator.GenerateAllFromText("123456");
                SmartDrawTransparentCube();
                //foreach (Bitmap bmp in bmps)
                //    bmp.Dispose();

            }
            // 刷新绘图命令
            Gl.glFlush();
            glControl1.SwapBuffers();  // 交换缓冲区
        }

        // 初始化OpenGL环境
        private void glControl1_Load(object sender, EventArgs e)
        {
            // 初始化上下文
            glControl1.InitializeContexts();

            // 设置清屏颜色（矢车菊蓝）
            Gl.glClearColor(0.39f, 0.58f, 0.93f, 1.0f);

            // 启用深度测试和混合（用于透明效果）
            // Gl.glEnable(Gl.GL_DEPTH_TEST);
            // Gl.glEnable(Gl.GL_BLEND);
            // Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

            textureGenerator = new TextureGenerator();
        }

        // 鼠标按下事件
        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;
            isDragging = true;
        }

        // 鼠标移动事件
        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                float deltaX = e.Location.X - lastMousePosition.X;
                float deltaY = e.Location.Y - lastMousePosition.Y;
                rotationY += deltaX;  // 水平拖动绕Y轴旋转
                rotationX += deltaY;  // 垂直拖动绕X轴旋转
                glControl1.Invalidate();  // 触发重绘
                lastMousePosition = e.Location;
            }
        }
        // 鼠标释放事件
        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        // 窗口大小改变事件
        private void glControl1_Resize(object sender, EventArgs e)
        {
            if (glControl1.Width == 0 || glControl1.Height == 0) return;

            // 设置视口
            Gl.glViewport(0, 0, glControl1.Width, glControl1.Height);

            // 设置投影矩阵
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            // 创建平行投影
            float aspectRatio = (float)glControl1.Width / glControl1.Height;
            Gl.glOrtho(-2.0 * aspectRatio, 2.0 * aspectRatio, -2.0, 2.0, -10.0, 10.0);
        }


        //private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        //{
        //     清除颜色缓冲区和深度缓冲区
        //    gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

        //     设置模型视图矩阵
        //    gl.MatrixMode(MatrixMode.Modelview);
        //    gl.LoadIdentity();

        //     设置观察点
        //    gl.LookAt(0, 0, 5, 0, 0, 0, 0, 1, 0);

        //     应用斜角投影变换
        //    ApplyObliqueProjection(gl);

        //     应用用户旋转
        //    gl.Rotate(rotationX, 1, 0, 0);
        //    gl.Rotate(rotationY, 0, 1, 0);


        //     应用当前变换矩阵
        //    ApplyMatrix(currentTransform);
        //     绘制透明立方体
        //    DrawTransparentCube(gl);

        //     刷新绘图命令
        //    gl.Flush();
        //}



        private void ApplyObliqueProjection()
        {
            // 构建斜角投影变换 (Z轴与Y轴成45度角，Z轴长度压缩为0.5)
            float angle = (float)Math.PI / 4.0f;  // 45度角
            float depth = 0.5f;                   // Z轴压缩比例

            // 创建变换矩阵
            float[] obliqueMatrix = new float[16]
            {
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                -depth * (float)Math.Cos(angle), -depth * (float)Math.Sin(angle), 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            };

            // 应用变换矩阵
            Gl.glMultMatrixf(obliqueMatrix);
        }

        private float _bkOpacity = 0.8f;

        private void BackupOpacity()
        {
            _bkOpacity = opacity;
        }

        private void RestoreOpacity()
        {
            opacity = _bkOpacity;
        }

        

        private void SmartDrawTransparentCube( bool useDefaultWhiteColor = true, bool enableTransparency = true)
        {
            Gl.glClearDepth(0);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            // 定义立方体的6个面的颜色
            float[][] faceColors = new float[][]
    {
        new float[] { 1.0f, 1.0f, 1.0f, 1.0f },  // 前面 - 白色
                new float[] { 1.0f, 1.0f, 1.0f, 1.0f },   // 右面 - 白色
        new float[] { 1.0f, 1.0f, 1.0f, 1.0f },  // 后面 - 白色
                new float[] { 1.0f, 1.0f, 1.0f, 1.0f },  // 左面 - 白色
        new float[] { 1.0f, 1.0f, 1.0f, 1.0f },  // 上面 - 白色
        new float[] { 1.0f, 1.0f, 1.0f, 1.0f },  // 下面 - 白色


    };

            if (!useDefaultWhiteColor)
            {
                faceColors = new float[][]
        {
            new float[] { 1.0f, 0.0f, 0.0f, 1.0f },  // 前面 - 红色
                      new float[] { 0.0f, 1.0f, 1.0f, 1.0f } ,  // 右面 - 青色
            new float[] { 0.0f, 1.0f, 0.0f, 1.0f },  // 后面 - 绿色
                            new float[] { 1.0f, 0.0f, 1.0f, 1.0f },  // 左面 - 紫色
            new float[] { 0.0f, 0.0f, 1.0f, 1.0f },  // 上面 - 蓝色
            new float[] { 1.0f, 1.0f, 0.0f, 1.0f },  // 下面 - 黄色

  
        };
            }

            // 立方体的顶点
            float[][] vertices = new float[][]
    {
        new float[] { -0.5f, -0.5f, -0.5f },  // 0
        new float[] { 0.5f, -0.5f, -0.5f },   // 1
        new float[] { 0.5f, 0.5f, -0.5f },    // 2
        new float[] { -0.5f, 0.5f, -0.5f },   // 3
        new float[] { -0.5f, -0.5f, 0.5f },  // 4
        new float[] { 0.5f, -0.5f, 0.5f },    // 5
        new float[] { 0.5f, 0.5f, 0.5f },     // 6
        new float[] { -0.5f, 0.5f, 0.5f }     // 7
    };

            // 立方体的6个面，每个面由两个三角形组成
            int[][] faces = new int[][]
    {
                new int[] { 7, 6, 5, 4 },  // 前面
                new int[] { 6, 2, 1, 5 },  // 左面
                new int[] { 2, 3, 0, 1 },        // 后面
                new int[] { 3, 7, 4, 0 },    //右面
                new int[] { 3, 2, 6, 7 }, // 上面
                new int[] { 4, 5, 1, 0 },  // 下面


    };

            // 纹理坐标
            float[][] texCoords = new float[][]
    {
        new float[] { 0, 0 },  // 左下
        new float[] { 1, 0 },  // 右下
        new float[] { 1, 1 },  // 右上
        new float[] { 0, 1 }   // 左上
    };

            // 保存当前的OpenGL状态
            Gl.glPushAttrib(Gl.GL_ALL_ATTRIB_BITS);

            // 启用深度测试
            Gl.glEnable(Gl.GL_DEPTH_TEST);

            // 第一次绘制：以最远距离为基准，绘制所有的面，透明度为1
            Gl.glDepthFunc(Gl.GL_GREATER); // 设置深度测试函数为小于
            Gl.glDepthMask(Gl.GL_TRUE); // 启用深度缓冲写入

            for (int i = 0; i < 6; i++)
            {
                Gl.glColor4f(faceColors[i][0], faceColors[i][1], faceColors[i][2], 1.0f); // 透明度为1

            //    if (bmps != null && bmps[i] != null)
                if(_textureIds.Count>=6)
                {
                    Gl.glEnable(Gl.GL_TEXTURE_2D);

                    Gl.glBindTexture(Gl.GL_TEXTURE_2D, _textureIds[_faceTexIdx[i]]);

                    //        CreateAndBindTexture(bmps[i]);
                }
                else
                {
                    Gl.glDisable(Gl.GL_TEXTURE_2D);
                }

                Gl.glBegin(Gl.GL_QUADS);
                for (int j = 0; j < 4; j++)
                {
                    if (_textureIds.Count >= 6)
                    {
                        Gl.glTexCoord2f(texCoords[j][0], texCoords[j][1]);
                    }
                    int vertexIndex = faces[i][j];
                    Gl.glVertex3f(vertices[vertexIndex][0], vertices[vertexIndex][1], vertices[vertexIndex][2]);
                }
                Gl.glEnd();
            }

            // 绘制立方体边界线
            DrawCubeEdges(vertices, faces);

            // 第二次绘制：以最近距离为基准，绘制所有的面，使用设置的opacity
            if (enableTransparency)
            {
                Gl.glEnable(Gl.GL_BLEND);
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            }
            Gl.glDepthFunc(Gl.GL_LESS);
            //      Gl.glDepthFunc(Gl.GL_GREATER); // 设置深度测试函数为大于
            //     Gl.glDepthMask(Gl.GL_FALSE); // 禁用深度缓冲写入

            for (int i = 0; i < 6; i++)
            {
                // 使用原始的颜色和透明度
                Gl.glColor4f(faceColors[i][0], faceColors[i][1], faceColors[i][2], opacity);

                if (_textureIds.Count >= 6)
                {
                    Gl.glEnable(Gl.GL_TEXTURE_2D);
                    Gl.glBindTexture(Gl.GL_TEXTURE_2D, _textureIds[_faceTexIdx[i]]);
                    //CreateAndBindTexture(bmps[i]);
                }
                else
                {
                    Gl.glDisable(Gl.GL_TEXTURE_2D);
                }

                Gl.glBegin(Gl.GL_QUADS);
                for (int j = 0; j < 4; j++)
                {
                    if (_textureIds.Count >= 6)
                    {
                        Gl.glTexCoord2f(texCoords[j][0], texCoords[j][1]);
                    }
                    int vertexIndex = faces[i][j];
                    Gl.glVertex3f(vertices[vertexIndex][0], vertices[vertexIndex][1], vertices[vertexIndex][2]);
                }
                Gl.glEnd();
            }

            // 恢复深度测试设置
            Gl.glDepthFunc(Gl.GL_LESS);
            Gl.glDepthMask(Gl.GL_TRUE);

            // 绘制立方体边界线
            DrawCubeEdges(vertices, faces);

            // 恢复OpenGL状态
            Gl.glPopAttrib();

            // 释放纹理资源

        }
        // 纹理ID列表，用于管理创建的纹理
        private List<uint> _textureIds = new List<uint>();

        private void ResetFaceTextIdx()
        {
            _faceTexIdx = new int[] { 0, 1, 2, 3, 4, 5 };
        }

        private void InitTexture(Bitmap[] bmps)
        {
            ReleaseTextures();
            _textureIds.Clear();
            foreach (Bitmap bmp in bmps)
            {
                _textureIds.Add(CreateAndBindTexture(bmp));
            }
        }

        // 创建并绑定纹理的函数
        private uint CreateAndBindTexture(Bitmap bitmap)
        {
            // 生成一个纹理ID
            uint textureId;
            Gl.glGenTextures(1, out textureId);
       //     _textureIds.Add(textureId);

            // 绑定纹理
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureId);

            // 设置纹理参数
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

            // 将位图数据上传到纹理
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, bitmap.Width, bitmap.Height, 0,
                            Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, bitmapData.Scan0);

            bitmap.UnlockBits(bitmapData);

            return textureId;
        }

        // 释放纹理的函数
        private void ReleaseTextures()
        {
            if (_textureIds.Count > 0)
            {
                uint[] ids = _textureIds.ToArray();
                Gl.glDeleteTextures(ids.Length, ids);
                _textureIds.Clear();
            }
        }

        

        private void DrawTransparentCube()
        {
            // 定义立方体的6个面的颜色（使用当前透明度）
            opacity = 1;
            float[][] faceColors = new float[][]
            {
                new float[] { 1.0f, 0.0f, 0.0f, opacity },  // 前面 - 红色
                new float[] { 0.0f, 1.0f, 0.0f, opacity },  // 后面 - 绿色
                new float[] { 0.0f, 0.0f, 1.0f, opacity },  // 上面 - 蓝色
                new float[] { 1.0f, 1.0f, 0.0f, opacity },  // 下面 - 黄色
                new float[] { 1.0f, 0.0f, 1.0f, opacity },  // 左面 - 紫色
                new float[] { 0.0f, 1.0f, 1.0f, opacity }   // 右面 - 青色
            };

            // 立方体的顶点
            float[][] vertices = new float[][]
            {
                new float[] { -0.5f, -0.5f, -0.5f },  // 0
                new float[] { 0.5f, -0.5f, -0.5f },   // 1
                new float[] { 0.5f, 0.5f, -0.5f },    // 2
                new float[] { -0.5f, 0.5f, -0.5f },   // 3
                new float[] { -0.5f, -0.5f, 0.5f },  // 4
                new float[] { 0.5f, -0.5f, 0.5f },    // 5
                new float[] { 0.5f, 0.5f, 0.5f },     // 6
                new float[] { -0.5f, 0.5f, 0.5f }     // 7
            };

            // 立方体的6个面，每个面由两个三角形组成
            int[][] faces = new int[][]
            {
                new int[] { 0, 1, 2, 3 },  // 前面
                new int[] { 5, 4, 7, 6 },  // 后面
                new int[] { 3, 2, 6, 7 },  // 上面
                new int[] { 4, 5, 1, 0 },  // 下面
                new int[] { 4, 0, 3, 7 },  // 左面
                new int[] { 1, 5, 6, 2 }   // 右面
            };

            // 纹理坐标
            float[][] texCoords = new float[][]
            {
                new float[] { 0, 0 },  // 左下
                new float[] { 1, 0 },  // 右下
                new float[] { 1, 1 },  // 右上
                new float[] { 0, 1 }   // 左上
            };

            float[] originalProjMatrix = new float[16];
            float[] originalModelMatrix = new float[16];
            Gl.glGetFloatv(Gl.GL_PROJECTION_MATRIX, originalProjMatrix);
            Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, originalModelMatrix);
            // 为每个面设置不同的颜色
            for (int i = 0; i < 6; i++)
            {
                Gl.glColor4f(faceColors[i][0], faceColors[i][1], faceColors[i][2], faceColors[i][3]);

                // 启用纹理并绑定当前面的纹理
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                textureGenerator.BindTexture(i);

                Gl.glBegin(Gl.GL_QUADS);
                for (int j = 0; j < 4; j++)
                {
                    Gl.glTexCoord2f(texCoords[j][0], texCoords[j][1]);
                    Gl.glVertex3f(vertices[faces[i][j]][0], vertices[faces[i][j]][1], vertices[faces[i][j]][2]);
                }
                Gl.glEnd();
            }
        }

        private void DrawCubeEdges(float[][] vertices, int[][] faces)
        {
            Gl.glColor4f(0, 0, 0, 1); // 设置边界线颜色为黑色，不透明
            Gl.glLineWidth(2); // 设置线宽

            Gl.glBegin(Gl.GL_LINES);
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int currentVertexIndex = faces[i][j];
                    int nextVertexIndex = faces[i][(j + 1) % 4];

                    Gl.glVertex3f(vertices[currentVertexIndex][0], vertices[currentVertexIndex][1], vertices[currentVertexIndex][2]);
                    Gl.glVertex3f(vertices[nextVertexIndex][0], vertices[nextVertexIndex][1], vertices[nextVertexIndex][2]);
                }
            }
            Gl.glEnd();
        }

        private void btnCubeLayout_Click(object sender, EventArgs e)
        {

            List<Image> imgList = new List<Image>();
            Bitmap[] bmps = RandomImageGenerator.GenerateAllFromText("123456");

            for (int i = 0; i < CubeNet.StandardNets.Count; i++)
                imgList.Add(CubeNetDrawer.DrawCubeNet(CubeNet.StandardNets[i], bmps));
            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;
        }

        private void imageListControl1_Load(object sender, EventArgs e)
        {

        }

        private void imageListControl1_SelectedImageChanged(object sender, EventArgs e)
        {
            try
            {
                picSrc.Image = imageListControl1.SelectedImage;
            }
            catch
            { }

        }

        private void openGLControl_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;
            isDragging = true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            opacity = trackBar1.Value / 100.0f;
            glControl1.Invalidate();  // 触发重绘
        }

        private void openGLControl_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            currentTransform = cubePositions[comboBox1.SelectedIndex];
            rotationX = 0;
            rotationY = 0;
            glControl1.Invalidate();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentTransform = cubePositions[comboBox1.SelectedIndex];
            rotationX = 0;
            rotationY = 0;
            glControl1.Invalidate();
        }

        private void Template3DForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isExporting)
            {
                e.Cancel = true;
                MessageBox.Show("Please stop the data export first!");
            }
            // 清理资源
            if (textureGenerator != null)
            {
                textureGenerator.Dispose();
                textureGenerator = null;
            }
            ReleaseTextures();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            DoUpdate(1);

        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            DoUpdate(-1);
        }

        private void DoUpdate(int step = -1)
        {
            int id = comboBox1.SelectedIndex;
            id += step;
            id = (comboBox1.Items.Count + id) % comboBox1.Items.Count;
            comboBox1.SelectedIndex = id;


        }

        /// <summary>
        /// 将当前OpenGL渲染内容保存为图片
        /// </summary>
        public Bitmap RenderFrameBuffer(int width, int height)
        {
            if (!glControl1.IsHandleCreated) return null;

            // 保存当前视口和矩阵状态（关键！）
            int[] originalViewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, originalViewport);

            float[] originalProjMatrix = new float[16];
            float[] originalModelMatrix = new float[16];
            Gl.glGetFloatv(Gl.GL_PROJECTION_MATRIX, originalProjMatrix);
            Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, originalModelMatrix);

            uint[] fbos = new uint[1];
            uint[] textures = new uint[1];
            Bitmap bmp = null;
            uint[] depthRenderBuffers = new uint[1];
            try
            {
                // 检查FBO扩展支持
                //string extensions = Gl.glGetString(Gl.GL_EXTENSIONS);
                //if (!extensions.Contains("GL_EXT_framebuffer_object"))
                //    return null;

                // 创建帧缓冲
                Gl.glGenFramebuffersEXT(1, fbos);
                uint fbo = fbos[0];
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, fbo);

                // 创建纹理附件
                Gl.glGenTextures(1, textures);
                uint textureId = textures[0];
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureId);
                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, width, height, 0,
                               Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, IntPtr.Zero);

                // 设置纹理参数
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

                // 附加纹理到帧缓冲
                Gl.glFramebufferTexture2DEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_COLOR_ATTACHMENT0_EXT,
                                              Gl.GL_TEXTURE_2D, textureId, 0);

                // 4. 创建深度缓冲区（关键！用于深度测试）
                Gl.glGenRenderbuffersEXT(1, depthRenderBuffers);
                uint depthRb = depthRenderBuffers[0];
                Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, depthRb);
                // 设置深度缓冲区格式（24位深度精度，足够大多数场景）
                Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_DEPTH_COMPONENT24, width, height);
                // 将深度缓冲区附加到FBO的深度通道
                Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_DEPTH_ATTACHMENT_EXT,
                                               Gl.GL_RENDERBUFFER_EXT, depthRb);

                // 检查帧缓冲完整性
                int status = Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT);
                if (status != Gl.GL_FRAMEBUFFER_COMPLETE_EXT)
                    return null;

                // 设置FBO的视口和投影矩阵
                Gl.glViewport(0, 0, width, height);
                Gl.glEnable(Gl.GL_DEPTH_TEST);
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadIdentity();
                float aspectRatio = (float)width / height;
                Gl.glOrtho(-2.0 * aspectRatio, 2.0 * aspectRatio, -2.0, 2.0, -10.0, 10.0);

                Gl.glMatrixMode(Gl.GL_MODELVIEW);
                Gl.glLoadIdentity();

                // 重新渲染
                glControl1_Paint(this, null);

                // 读取像素数据
                byte[] pixelData = new byte[width * height * 4];
                Gl.glReadPixels(0, 0, width, height, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixelData);
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    byte temp = pixelData[i];       // 保存红色通道
                    pixelData[i] = pixelData[i + 2]; // 蓝色通道移到红色位置
                    pixelData[i + 2] = temp;       // 红色通道移到蓝色位置
                }

                // 创建Bitmap
                bmp = new Bitmap(width, height);
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
                bmp.UnlockBits(bmpData);

                // 垂直翻转图像
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            catch
            {
                return null;
            }
            finally
            {
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadMatrixf(originalProjMatrix);


                //Gl.glMatrixMode(Gl.GL_MODELVIEW);
                //Gl.glLoadMatrixf(originalModelMatrix);

                // 恢复视口和清理FBO资源
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, 0);
                Gl.glViewport(originalViewport[0], originalViewport[1],
                             originalViewport[2], originalViewport[3]);

                if (textures[0] != 0) Gl.glDeleteTextures(1, textures);
                if (fbos[0] != 0) Gl.glDeleteFramebuffersEXT(1, fbos);
                if (depthRenderBuffers[0] != 0) Gl.glDeleteRenderbuffersEXT(1, depthRenderBuffers);
            }
            return bmp;
        }

        private void btnRender_Click(object sender, EventArgs e)
        {

            Bitmap bmp = RenderFrameBuffer(800, 600);
            picSrc.Image = bmp;
            pictureBox1.Image = bmp;
            glControl1.Invalidate();
        }

        private void glControl1_VisibleChanged(object sender, EventArgs e)
        {
            if (glControl1.Visible)
            {
                glControl1_Resize(null, null);
                glControl1.Invalidate();
            }
        }

        private void btnGenBmps_Click(object sender, EventArgs e)
        {
            List<Image> imgList = new List<Image>();
            imgList.AddRange(Folding3DUtils.GenerateDefaultImages());

            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;

            //// 示例1：生成包含空白图片的混合类别图片
            //var options = new ImageGenerationOptions
            //{
            //    Size = 200,
            //    AllowBlankImages = true, // 允许空白图片
            //    LineWidth = 5, // 线条加粗到5px
            //    LineDensity = 2 // 2条线条
            //};
            //Bitmap[] mixedImages = RandomImageGenerator.Generate6Images(options);

            //// 示例2：从字符串生成6张不同的字符图片
            //string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            //// 示例3：生成优化后的线条图（仅线条类别）
            //var lineOptions = new ImageGenerationOptions
            //{
            //    Categories = ImageCategory.Lines,
            //    LineWidth = 6,
            //    Size = 150
            //};
            //Bitmap[] lineImages = RandomImageGenerator.Generate6Images(lineOptions);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length < 6)
            {
                MessageBox.Show("字符数量少于6");
                return;
            }
            List<Image> imgList = new List<Image>();
            imgList.AddRange(RandomImageGenerator.GenerateAllFromText(textBox1.Text));


            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 示例3：生成优化后的线条图（仅线条类别）
            var lineOptions = new ImageGenerationOptions
            {
                Categories = ImageCategory.Lines,
                LineWidth = 6,
                Size = 150
            };
            Bitmap[] lineImages = RandomImageGenerator.Generate6Images(lineOptions);
            List<Image> imgList = new List<Image>();
            imgList.AddRange(lineImages);


            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 1. 配置生成参数（仅启用箭头类别）
            ImageGenerationOptions options = new ImageGenerationOptions();
            options.Categories = ImageCategory.Arrow; // 只生成箭头
            options.Size = 200; // 图片尺寸200x200
            options.LineWidth = 5; // 箭头线条宽度5px（加粗显示）

            // 2. 生成6张不同方向的箭头图片
            Bitmap[] arrowImages = RandomImageGenerator.Generate6Images(options);
            List<Image> imgList = new List<Image>();
            imgList.AddRange(arrowImages);


            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ImageGenerationOptions options = new ImageGenerationOptions();
            options.Categories = ImageCategory.Geometric; // 只生成几何图形
            // 默认：随机颜色、尺寸256x256、不允许重复

            // 2. 生成6张不同的几何图片
            Bitmap[] geometricImages = RandomImageGenerator.Generate6Images(options);
            List<Image> imgList = new List<Image>();
            imgList.AddRange(geometricImages);


            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // 步骤1：配置生成参数（指定填充纹理类别，默认包含渐变色）
            ImageGenerationOptions options = new ImageGenerationOptions();
            options.Categories = ImageCategory.FillTexture; // 仅生成填充纹理（含渐变色）
            options.Size = 200; // 图片尺寸200x200像素（可选，默认256）
            // 可选：如需提高渐变色比例，可在GenerateFillTextureImage中调整权重（见后文）

            // 步骤2：生成6张包含渐变色的图片
            Bitmap[] gradientImages = RandomImageGenerator.Generate6Images(options);
            List<Image> imgList = new List<Image>();
            imgList.AddRange(gradientImages);


            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            // 步骤1：配置生成参数（指定填充纹理类别，默认包含渐变色）
            ImageGenerationOptions options = new ImageGenerationOptions();
            options.Categories = ImageCategory.Mosaic; // 仅生成填充纹理（含渐变色）
            options.Size = 200; // 图片尺寸200x200像素（可选，默认256）
            // 可选：如需提高渐变色比例，可在GenerateFillTextureImage中调整权重（见后文）

            // 步骤2：生成6张包含渐变色的图片
            Bitmap[] gradientImages = RandomImageGenerator.Generate6Images(options);
            List<Image> imgList = new List<Image>();
            imgList.AddRange(gradientImages);


            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex >= 0)
            {

                List<Image> imgList = new List<Image>();
                Bitmap[] bmps = RandomImageGenerator.GenerateAllFromText("123456");

                foreach (CubeNet net in CubeNet.GroupNets[comboBox2.SelectedIndex])
                    imgList.Add(CubeNetDrawer.DrawCubeNet(net, bmps));
                imageListControl1.BindImages(imgList);
                tabControl1.SelectedIndex = 0;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<Image> imgList = new List<Image>();
            Bitmap[] bmps = RandomImageGenerator.GenerateAllFromText("123456");

            List<CubeNet> groupList = CubeNet.CreateUnSimilarArray(CubeNet.refNet);
            for (int i = 0; i < groupList.Count; i++)
                imgList.Add(CubeNetDrawer.DrawCubeNet(groupList[i], bmps));
            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;
        }

        private void btnPure2D_Click(object sender, EventArgs e)
        {
            DoGenPure2DQues();
        }

        private void btnPure3D_Click(object sender, EventArgs e)
        {
            DoGenPure3DQues();
        }

        Random _rand = new Random(Environment.TickCount);

        private Bitmap[] GenerateRandBmps()
        {
            Bitmap[] bmps = null;
            double rate = _rand.NextDouble();
            if (rate > 0.7)
            {
                List<Bitmap> bmpList = new List<Bitmap>();
                for (int i = 0; i < 6; i++)
                    bmpList.Add(OrigamiConfuseScheme.GenRandomBitmap(256));

                bmps = bmpList.ToArray();
            }
            else if (rate > 0.5)
            {
                int id = _rand.Next();
                if (id % 3 == 0)
                {
                    bmps = RandomImageGenerator.GenerateAllFromText("123456");
                }
                else if (id % 3 == 1)
                {
                    bmps = RandomImageGenerator.GenerateAllFromText("ABCDEF");
                }
                else
                {
                    bmps = RandomImageGenerator.GenerateAllFromText("HZNUER");
                }
            }
            else if (rate > 0.3)
            {
                ImageGenerationOptions options = new ImageGenerationOptions();
                options.Categories = ImageCategory.Geometric; // 只生成几何图形
                // 默认：随机颜色、尺寸256x256、不允许重复
                // 2. 生成6张不同的几何图片
                bmps = RandomImageGenerator.Generate6Images(options);
            }
            else
            {
                // 步骤1：配置生成参数（指定填充纹理类别，默认包含渐变色）
                ImageGenerationOptions options = new ImageGenerationOptions();
                options.Categories = ImageCategory.Mosaic; // 仅生成填充纹理（含渐变色）
                options.Size = 200; // 图片尺寸200x200像素（可选，默认256）
                // 可选：如需提高渐变色比例，可在GenerateFillTextureImage中调整权重（见后文）


                bmps = RandomImageGenerator.Generate6Images(options);
            }
            return bmps;
        }

        private void DoGenPure2DQues()
        {
            Bitmap[] bmps = GenerateRandBmps();

            List<Image> imgList = new List<Image>();
            int answerId = _rand.Next() % 4;
            if (comboBox3.SelectedIndex == comboBox3.Items.Count - 1)
            {
                List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(CubeNet.GroupNets.Count - 1, 4);



                for (int i = 0; i < idList.Count; i++)
                {
                    CubeNet cubenet = CubeNet.GroupNets[idList[i]][_rand.Next() % CubeNet.GroupNets[idList[i]].Length];
                    if (i != answerId)
                        imgList.Add(CubeNetDrawer.DrawCubeNet(cubenet, bmps));
                    else
                    {
                        imgList.Add(CubeNetDrawer.DrawCubeNet(CubeNet.CreateRandomUnSimlar(cubenet), bmps));
                    }
                }
            }
            else if (comboBox3.SelectedIndex >= 0)
            {
                CubeNet[] cubeGroup = CubeNet.GroupNets[comboBox3.SelectedIndex];
                List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(cubeGroup.Length - 1, 4);



                for (int i = 0; i < idList.Count; i++)
                {
                    CubeNet cubenet = cubeGroup[idList[i]];
                    if (i != answerId)
                        imgList.Add(CubeNetDrawer.DrawCubeNet(cubenet, bmps));
                    else
                    {
                        imgList.Add(CubeNetDrawer.DrawCubeNet(CubeNet.CreateRandomUnSimlar(cubenet), bmps));
                    }
                }
            }
            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;
            //    CubeNet.GroupNets
            labelAnswer.Text = ((Char)('A' + answerId)).ToString();
        }

        private void DoGenPure3DQues()
        {
            Bitmap[] bmps = GenerateRandBmps();
            InitTexture(bmps);

            List<Image> imgList = new List<Image>();
            List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(24 - 1, 4);

            int answerId = _rand.Next() % 4;

            BackupOpacity();
            opacity = 0.8f;
            for (int i = 0; i < 4; i++)
            {

                currentTransform = cubePositions[idList[i]];
                rotationX = 0;
                rotationY = 0;
                glControl1.Invalidate();
                ResetFaceTextIdx();
                if (i == answerId)
                {
                    ConfuseFaceIdx(idList[i] );

                }
                else
                {

                }

                Bitmap subBmp = RenderFrameBuffer(800, 600);

               imgList.Add(Folding3DUtils.GetSubBitmap(subBmp, new Rectangle(276, 177, 248, 248)));


            }

            RestoreOpacity();
            ResetFaceTextIdx();
            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;
            //    CubeNet.GroupNets
            labelAnswer.Text = ((Char)('A' + answerId)).ToString();

            foreach (Bitmap bmp in bmps)
                bmp.Dispose();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap[] bmps = GenerateRandBmps();

                InitTexture(bmps);
                foreach (Bitmap bmp in bmps)
                    bmp.Dispose();

                glControl1.Invalidate();
            }
            catch
            { }
        }

        private List<int[]> GetConfuseFaceIdx(int num =4)
        {
            List<int[]> confuseList = new List<int[]>();
            int[,] arranges = Folding3DUtils._cubeArrangements;
            int[] faceMap = Folding3DUtils._faceMap;

            List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(arranges.GetLength(0) - 1, 4);


         
            foreach(int newId in idList)
            {


                Dictionary<int, int> dicM = new Dictionary<int, int>();
                for (int i = 0; i < arranges.GetLength(1); i++)
                {
                    int id = arranges[newId, i];

                    dicM.Add(faceMap[i], faceMap[id - 1]);

                }
                int[] newFaceIdx = new int[6];
                for (int i = 0; i < 6; i++)
                    newFaceIdx[i] = dicM[i];

              //  _faceTexIdx = newFaceIdx;

                confuseList.Add(newFaceIdx);

            }

            return confuseList;
        }


        private List<int[]> GetConfuseFaceIdx(int faceId, int num)
        {
            List<int[]> confuseList = new List<int[]>();
            int[,] arranges = Folding3DUtils._cubeArrangements;
            int[] faceMap = new int[6];
            for(int i=0; i<6; i++)
                faceMap[i] = Folding3DUtils._faceMapGroup[faceId, i];

            List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(arranges.GetLength(0) - 1, 4);



            foreach (int newId in idList)
            {


                Dictionary<int, int> dicM = new Dictionary<int, int>();
                for (int i = 0; i < arranges.GetLength(1); i++)
                {
                    int id = arranges[newId, i];

                    dicM.Add(faceMap[i], faceMap[id - 1]);

                }
                int[] newFaceIdx = new int[6];
                for (int i = 0; i < 6; i++)
                    newFaceIdx[i] = dicM[i];

                //  _faceTexIdx = newFaceIdx;

                confuseList.Add(newFaceIdx);

            }

            return confuseList;
        }

        //依据group,1,2,3,4,5,6， 表示group所指向的面不变化
        private void ConfuseFaceIdx(int group)
        {
            int[,] arranges = Folding3DUtils._cubeArrangements;


            int[] faceMap = new int[6];
            for (int i = 0; i < 6; i++)
            {
                faceMap[i] = Folding3DUtils._faceMapGroup[group, i];
            }

            int newId = _rand.Next(arranges.GetLength(0));

            {


                Dictionary<int, int> dicM = new Dictionary<int, int>();
                for (int i = 0; i < arranges.GetLength(1); i++)
                {
                    int id = arranges[newId, i];

                    dicM.Add(faceMap[i], faceMap[id - 1]);

                }
                int[] newFaceIdx = new int[6];
                for (int i = 0; i < 6; i++)
                    newFaceIdx[i] = dicM[i];

                _faceTexIdx = newFaceIdx;



            }
        }

        private void ConfuseFaceIdx()
        {
            int[,] arranges = Folding3DUtils._cubeArrangements;
            int[] faceMap = Folding3DUtils._faceMap;

            int newId = _rand.Next(arranges.GetLength(0));

            {


                Dictionary<int, int> dicM = new Dictionary<int, int>();
                for (int i = 0; i < arranges.GetLength(1); i++)
                {
                    int id = arranges[newId, i];

                    dicM.Add(faceMap[i], faceMap[id - 1]);

                }
                int[] newFaceIdx = new int[6];
                for (int i = 0; i < 6; i++)
                    newFaceIdx[i] = dicM[i];

                _faceTexIdx = newFaceIdx;

 

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ConfuseFaceIdx();
            glControl1.Invalidate();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ResetFaceTextIdx();
            glControl1.Invalidate();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
    {
        MessageBox.Show("渲染图片为空！", "保存失败", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }

    using (SaveFileDialog saveDialog = new SaveFileDialog())
    {
        // 设置对话框属性
        saveDialog.Filter = "BMP图片|*.bmp|所有文件|*.*";
        saveDialog.Title = "保存图片";
        saveDialog.DefaultExt = "bmp";
        saveDialog.AddExtension = true;
        saveDialog.CheckPathExists = true;
        
        // 显示对话框并获取用户选择
        DialogResult result = saveDialog.ShowDialog();
        
        if (result == DialogResult.OK)
        {
            try
            {
                // 保存图片为BMP格式
                pictureBox1.Image.Save(saveDialog.FileName);

            }
            catch (Exception ex)
            {

            }
        }
    }
            }
            catch
            { }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DoGen2DTo3DQues();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            DoGen3DTo2DQues();
        }

        private void DoGen2DTo3DQues()
        {
            Bitmap[] bmps = GenerateRandBmps();

            InitTexture(bmps);

            List<Image> imgList = new List<Image>();
            int answerId = _rand.Next() % 4;

            int randGroup = _rand.Next(CubeNet.GroupNets.Count);
            int randId = _rand.Next(CubeNet.GroupNets[randGroup].Length);
            CubeNet randNet = CubeNet.GroupNets[randGroup][randId];

            imgList.Add(CubeNetDrawer.DrawCubeNet(randNet, bmps));
            List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(comboBox1.Items.Count - 1, 4);


            BackupOpacity();
            opacity = 0.8f;
            if (checkBox1.Checked) //选择不符合条件的一个。
            {
                for (int i = 0; i < 4; i++)
                {

                    currentTransform = cubePositions[idList[i]];
                    rotationX = 0;
                    rotationY = 0;
                    glControl1.Invalidate();
                    ResetFaceTextIdx();
                    if (i == answerId)
                    {
                        ConfuseFaceIdx();

                    }
                    else
                    {

                    }

                    Bitmap subBmp = RenderFrameBuffer(800, 600);

                    imgList.Add(Folding3DUtils.GetSubBitmap(subBmp, new Rectangle(276, 177, 248, 248)));


                }
            }
            else
            {

                List<int[]> confuseList = GetConfuseFaceIdx(4);
                for (int i = 0; i < 4; i++)
                {

                    currentTransform = cubePositions[idList[i]];
                    rotationX = 0;
                    rotationY = 0;
                    glControl1.Invalidate();
                    ResetFaceTextIdx();
                    if (i != answerId)
                    {
                        _faceTexIdx = confuseList[i];
                     //   ConfuseFaceIdx();

                    }
                    else
                    {

                    }

                    Bitmap subBmp = RenderFrameBuffer(800, 600);

                    imgList.Add(Folding3DUtils.GetSubBitmap(subBmp, new Rectangle(276, 177, 248, 248)));


                }
            }

            RestoreOpacity();
            ResetFaceTextIdx();
            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;
            //    CubeNet.GroupNets
            labelAnswer.Text = ((Char)('A' + answerId)).ToString();

            foreach (Bitmap bmp in bmps)
                bmp.Dispose();
        }

        private void DoGen3DTo2DQues()
        {
            Bitmap[] bmps = GenerateRandBmps();

            InitTexture(bmps);

            List<Image> imgList = new List<Image>();
            int answerId = _rand.Next() % 4;

            //int randGroup = _rand.Next(CubeNet.GroupNets.Count);
            //int randId = _rand.Next(CubeNet.GroupNets[randGroup].Length);
            




        
            List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(CubeNet.GroupNets.Count - 1, 4);

            
            for (int i = 0; i < 4; i++)
            {
                CubeNet randNet = CubeNet.GroupNets[idList[i]][_rand.Next(CubeNet.GroupNets[idList[i]].Length)];

                bool needConfuse = true;  //是否混淆
                
                if (checkBox1.Checked )
                {
                    needConfuse = i == answerId;
                }
                else
                {
                    needConfuse = i != answerId;
                }
                if (needConfuse)
                {
                    imgList.Add(CubeNetDrawer.DrawCubeNet(CubeNet.CreateRandomUnSimlar(randNet), bmps));
                }
                else
                {
                    imgList.Add(CubeNetDrawer.DrawCubeNet(randNet, bmps));

                }

                
            }

       


            BackupOpacity();
            opacity = 0.8f;
       //     if (checkBox1.Checked) //选择不符合条件的一个。
            {
         //       for (int i = 0; i < 4; i++)
                {

                    currentTransform = cubePositions[_rand.Next(cubePositions.Count)];
                    rotationX = 0;
                    rotationY = 0;
                    glControl1.Invalidate();
                    ResetFaceTextIdx();


                    Bitmap subBmp = RenderFrameBuffer(800, 600);

                    imgList.Insert(0, Folding3DUtils.GetSubBitmap(subBmp, new Rectangle(276, 177, 248, 248)));


                }

            }

            RestoreOpacity();
            ResetFaceTextIdx();
            imageListControl1.BindImages(imgList);
            tabControl1.SelectedIndex = 0;
            //    CubeNet.GroupNets
            labelAnswer.Text = ((Char)('A' + answerId)).ToString();

            foreach (Bitmap bmp in bmps)
                bmp.Dispose();
        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                splitContainer1.Panel2Collapsed = true;
                splitContainer2.Panel2Collapsed = true;
            }
            else
            {
                splitContainer1.Panel2Collapsed = false;
                splitContainer2.Panel2Collapsed = false;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;

            _exportSetting = GetExportSetting();


            if (_exportSetting._templateList.Count == 0)
            {
                MessageBox.Show("没有选择任何模版！");
                return;
            }
            if (_exportSetting._faceImgTypeList.Count == 0)
            {
                MessageBox.Show("没有选择任何图片类型！");    
                return;
            }


            _stopExport = false;

            (new Thread(DoExport)).Start();
            labelExportInf.Visible = true;
            btnExport.Enabled = false;
            progressBar1.Value = 0;
        }

        private float CalPercent(int exportId, int imgTypeId, int typeId, int exportNum)
        {
            try
            {
                float nTemplateCount = _exportSetting._templateList.Count;
                float nImgTypeCount = _exportSetting._faceImgTypeList.Count;
                return (((float)exportId / exportNum + imgTypeId) / nImgTypeCount + typeId) / nTemplateCount;
            }
            catch{}
            return 0;
        }


        private void DoExport()
        {
            _isExporting = true;
            int id = 0;
            int totalNum = 0;
            Image unknowImg = null;

            try
            {
                // 初始化未知图片并使用using确保释放
                unknowImg = Bitmap.FromFile(Application.StartupPath + "\\unknown.bmp");
                string saveDir = _exportSetting._path;

                int exportId = 0;
                int nTemplateCount = _exportSetting._templateList.Count;
                int nImgTypeCount = _exportSetting._faceImgTypeList.Count;

                foreach (_3DExportParam exportParam in _exportSetting._templateList)
                {
                    int exportNum = exportParam._exportNum;
                    string savePath = Path.Combine(saveDir, exportParam._exportType.ToString());
                    Directory.CreateDirectory(savePath); // 简化目录创建（Exists判断可省略）

                    string savePath1 = Path.Combine(savePath, "easy");
                    string savePath2 = Path.Combine(savePath, "hard");
                    Directory.CreateDirectory(savePath1);
                    Directory.CreateDirectory(savePath2);

                    

                    string jsonlPath1 = Path.Combine(savePath1, "answer.jsonl");
                    string jsonlPath2 = Path.Combine(savePath2, "answer.jsonl");

                    savePath1 += "\\imgs";
                    savePath2 += "\\imgs";
                    if(!Directory.Exists(savePath1))
                        Directory.CreateDirectory(savePath1);

                    if (!Directory.Exists(savePath2))
                        Directory.CreateDirectory(savePath2);

                    // 使用using自动释放StreamWriter
                    using (StreamWriter writer1 = new StreamWriter(jsonlPath1))
                    using (StreamWriter writer2 = new StreamWriter(jsonlPath2))
                    {
                        int faceImgId = 0;
                        switch (exportParam._exportType)
                        {
                            case _3DExportType._2D:
                                {
                                    foreach (_3DFaceImageType imgType in _exportSetting._faceImgTypeList)
                                    {
                                        for (int i = 0; i < exportParam._exportNum; i++)
                                        {
                                            progressBar1.Value = (int)(100 * CalPercent(i, faceImgId, exportId, exportParam._exportNum));
                                            if (_stopExport) break;

                                            // Easy模式 - 封装为方法确保资源释放
                                            totalNum += ProcessExport(imgType, i, savePath1, writer1, false, exportParam._exportType);

                                            // Hard模式
                                            totalNum += ProcessExport(imgType, i, savePath2, writer2, true, exportParam._exportType);
                                        }
                                        faceImgId++;
                                    }
                                }
                                break;

                            case _3DExportType._3D:
                                {
                                    foreach (_3DFaceImageType imgType in _exportSetting._faceImgTypeList)
                                    {
                                        for (int i = 0; i < exportParam._exportNum; i++)
                                        {
                                            progressBar1.Value = (int)(100 * CalPercent(i, faceImgId, exportId, exportParam._exportNum));
                                            if (_stopExport) break;

                                            totalNum += Process3DExport(imgType, i, savePath1, writer1, false);
                                            totalNum += Process3DExport(imgType, i, savePath2, writer2, true);
                                        }
                                        faceImgId++;
                                    }
                                }
                                break;

                            case _3DExportType._2DTo3D_Y:
                            case _3DExportType._2DTo3D_N:
                                {
                                    foreach (_3DFaceImageType imgType in _exportSetting._faceImgTypeList)
                                    {
                                        for (int i = 0; i < exportParam._exportNum; i++)
                                        {
                                            progressBar1.Value = (int)(100 * CalPercent(i, faceImgId, exportId, exportParam._exportNum));
                                            if (_stopExport) break;

                                            bool isYType = exportParam._exportType == _3DExportType._2DTo3D_Y;
                                            totalNum += Process2DTo3DExport(imgType, i, savePath1, writer1, isYType, false);
                                            totalNum += Process2DTo3DExport(imgType, i, savePath2, writer2, isYType, true);
                                        }
                                        faceImgId++;
                                    }
                                }
                                break;

                            case _3DExportType._3DTo2D_Y:
                            case _3DExportType._3DTo2D_N:
                                {
                                    foreach (_3DFaceImageType imgType in _exportSetting._faceImgTypeList)
                                    {
                                        for (int i = 0; i < exportParam._exportNum; i++)
                                        {
                                            progressBar1.Value = (int)(100 * CalPercent(i, faceImgId, exportId, exportParam._exportNum));
                                            if (_stopExport) break;

                                            bool isYType = exportParam._exportType == _3DExportType._3DTo2D_Y;
                                            totalNum += Process3DTo2DExport(imgType, i, savePath1, writer1, isYType, false);
                                            totalNum += Process3DTo2DExport(imgType, i, savePath2, writer2, isYType, true);
                                        }
                                        faceImgId++;
                                    }
                                }
                                break;
                        }
                        exportId++;
                        if (_stopExport) break;
                    } // StreamWriter自动释放
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                // 确保未知图片释放
                if (unknowImg != null)
                {
                    unknowImg.Dispose();
                    unknowImg = null;
                }
                _isExporting = false;
            }

            // 更新UI状态
            progressBar1.Value = _stopExport ? progressBar1.Value : 100;
            labelExportInf.Visible = false;
            Invoke(new Action(() =>
            {
                btnExport.Enabled = true;
                string message = _stopExport
                    ? "Export project terminated!"
                    : $"Export project completed! Export count: {totalNum}";
                MessageBox.Show(this, message, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }

        // 辅助方法：处理2D导出，封装资源释放逻辑
        private int ProcessExport(_3DFaceImageType imgType, int index, string savePath, StreamWriter writer, bool isHard, _3DExportType exportType)
        {
            int newId = 0;
            List<Image> imgList = null;
            Bitmap result = null;

            try
            {
                imgList = DoCreatePure2D(imgType, out newId, isHard);
                result = ImageComposerV2.ComposeImages(null, imgList);

                char answer = (char)('A' + newId);
                string filename = $"{exportType}imgType-{imgType}-sample_{index}-answ_{answer}{_exportSetting._ext}";
                string fullPath = Path.Combine(savePath, filename);

                result.Save(fullPath);
                labelExportInf.Text = "Write file: " + filename;
                string jsonLine = $"{{\"image\":\"./imgs/{filename}\",\"answer\":\"{answer}\"}}";
                writer.WriteLine(jsonLine);
                writer.Flush(); // 及时写入避免缓存溢出

                Thread.Sleep(5);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProcessExport error: {ex.Message}");
                return 0;
            }
            finally
            {
                // 释放所有图片资源
                if (result != null) result.Dispose();
                if (imgList != null)
                {
                    foreach (Image img in imgList)
                        img.Dispose();
                    imgList.Clear();
                }
            }
        }

        // 辅助方法：处理3D导出
        private int Process3DExport(_3DFaceImageType imgType, int index, string savePath, StreamWriter writer, bool isHard)
        {
            int newId = 0;
            List<Image> imgList = null;
            Bitmap result = null;

            try
            {
                imgList = DoCreatePure3D(imgType, out newId, isHard);
                result = ImageComposerV2.ComposeImages(null, imgList);

                char answer = (char)('A' + newId);
                string filename = $"_3DimgType-{imgType}-sample_{index}-answ_{answer}{_exportSetting._ext}";
                string fullPath = Path.Combine(savePath, filename);

                result.Save(fullPath);
                labelExportInf.Text = "Write file: " + filename;
                string jsonLine = $"{{\"image\":\"./imgs/{filename}\",\"answer\":\"{answer}\"}}";
                writer.WriteLine(jsonLine);
                writer.Flush();

                Thread.Sleep(5);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Process3DExport error: {ex.Message}");
                return 0;
            }
            finally
            {
                if (result != null) result.Dispose();
                if (imgList != null)
                {
                    foreach (Image img in imgList)
                        img.Dispose();
                    imgList.Clear();
                }
            }
        }

        // 辅助方法：处理2D转3D导出
        private int Process2DTo3DExport(_3DFaceImageType imgType, int index, string savePath, StreamWriter writer, bool isYType, bool isHard)
        {
            int newId = 0;
            List<Image> firstRow = new List<Image>();
            List<Image> imgList = null;
            Bitmap result = null;

            try
            {
                imgList = DoCreate2DTo3D(imgType, out newId, out firstRow, isYType, isHard);
                result = ImageComposerV2.ComposeImages(firstRow, imgList);

                char answer = (char)('A' + newId);
                string filename = $"_2DTo3DimgType-{imgType}-sample_{index}-answ_{answer}{_exportSetting._ext}";
                string fullPath = Path.Combine(savePath, filename);

                result.Save(fullPath);
                labelExportInf.Text = "Write file: "+ filename;
                string jsonLine = $"{{\"image\":\"./imgs/{filename}\",\"answer\":\"{answer}\"}}";
                writer.WriteLine(jsonLine);
                writer.Flush();

                Thread.Sleep(5);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Process2DTo3DExport error: {ex.Message}");
                return 0;
            }
            finally
            {
                if (result != null) result.Dispose();
                if (imgList != null)
                {
                    foreach (Image img in imgList)
                        img.Dispose();
                    imgList.Clear();
                }
                foreach (Image img in firstRow)
                    img.Dispose();
                firstRow.Clear();
            }
        }

        // 辅助方法：处理3D转2D导出
        private int Process3DTo2DExport(_3DFaceImageType imgType, int index, string savePath, StreamWriter writer, bool isYType, bool isHard)
        {
            int newId = 0;
            List<Image> firstRow = new List<Image>();
            List<Image> imgList = null;
            Bitmap result = null;

            try
            {
                imgList = DoCreate3DTo2D(imgType, out newId, out firstRow, isYType, isHard);
                result = ImageComposerV2.ComposeImages(firstRow, imgList);

                char answer = (char)('A' + newId);
                string filename = $"_3DTo2DimgType-{imgType}-sample_{index}-answ_{answer}{_exportSetting._ext}";
                string fullPath = Path.Combine(savePath, filename);

                result.Save(fullPath);
                labelExportInf.Text = "Write file: " + filename;
                string jsonLine = $"{{\"image\":\"./imgs/{filename}\",\"answer\":\"{answer}\"}}";
                writer.WriteLine(jsonLine);
                writer.Flush();

                Thread.Sleep(5);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Process3DTo2DExport error: {ex.Message}");
                return 0;
            }
            finally
            {
                if (result != null) result.Dispose();
                if (imgList != null)
                {
                    foreach (Image img in imgList)
                        img.Dispose();
                    imgList.Clear();
                }
                foreach (Image img in firstRow)
                    img.Dispose();
                firstRow.Clear();
            }
        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                textPath.Text = dlg.SelectedPath;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _stopExport = true;
            Thread.Sleep(300);
        }


        //混淆方面，可能只有布局的干扰，没有单面的旋转变换。或镜像？？ 90,180,270 或镜像等等。
        private List<Image> DoCreatePure2D(_3DFaceImageType imgType, out int answerId, bool isHard = true)
        {
            Bitmap[] bmps = GenerateImgs(imgType);
            List<Image> imgList = new List<Image>();
            answerId = _rand.Next() % 4;

            if (isHard)  //混合模式
            {
                //11布局选4
                List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(CubeNet.GroupNets.Count - 1, 4);

                for (int i = 0; i < idList.Count; i++)
                {
                    CubeNet cubenet = CubeNet.GroupNets[idList[i]][_rand.Next() % CubeNet.GroupNets[idList[i]].Length];
                    if (i != answerId)
                        imgList.Add(CubeNetDrawer.DrawCubeNet(cubenet, bmps,100, false));
                    else
                    {
                        //其他行为？？
                        imgList.Add(CubeNetDrawer.DrawCubeNet(CubeNet.CreateRandomUnSimlar(cubenet), bmps, 100, false));
                    }
                }
            }
            else 
            {
                //单布局模式 单一模式 24选4个
                CubeNet[] cubeGroup = CubeNet.GroupNets[_rand.Next(CubeNet.GroupNets.Count)];
                List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(cubeGroup.Length - 1, 4);

                for (int i = 0; i < idList.Count; i++)
                {
                    CubeNet cubenet = cubeGroup[idList[i]];
                    if (i != answerId)
                        imgList.Add(CubeNetDrawer.DrawCubeNet(cubenet, bmps, 100, false));
                    else
                    {
                        imgList.Add(CubeNetDrawer.DrawCubeNet(CubeNet.CreateRandomUnSimlar(cubenet), bmps, 100, false));
                    }
                }
            }
            return imgList;
        }

        private Bitmap _threadRenderBmp = null;

                //混淆方面，可能只有布局的干扰，没有单面的旋转变换。或镜像？？ 90,180,270 或镜像等等。
        private List<Image> DoCreatePure3D(_3DFaceImageType imgType, out int answerId, bool isHard )
        {
            Bitmap[] bmps = GenerateImgs(imgType);
            List<Image> imgList = new List<Image>();
            answerId = _rand.Next() % 4;
            Invoke(new Action(() =>
            {
                InitTexture(bmps);
            }));

            List<int> idList = new List<int>();
            //comboBox1 一共24次位置
            if (isHard)
            {
                List<int> groupList = Folding3DUtils.GetUniqueRandomNumbers(5, 4);
                foreach (int group in groupList)
                    idList.Add(group * 4 + _rand.Next(4));
            }
            else
            {
                int group = _rand.Next(6);
                List<int> tmpList = Folding3DUtils.GetUniqueRandomNumbers(3, 4);
                foreach (int tmp in tmpList)
                    idList.Add(group * 4 + tmp);
            }

            BackupOpacity();
            opacity = _exportSetting._opacity;
            for (int i = 0; i < 4; i++)
            {

                currentTransform = cubePositions[idList[i]];
                rotationX = 0;
                rotationY = 0;
                glControl1.Invalidate();
                ResetFaceTextIdx();
                if (i == answerId)
                {
                    ConfuseFaceIdx(idList[i]);
                }
                else
                {

                }


                //Task.Factory.StartNew(() =>
                //{
                //    return RenderInMainThread(800, 600);
                //}).ContinueWith(task =>
                //{
                //    if (task.IsFaulted)
                //    {
                //        // 处理异常
                //        MessageBox.Show("渲染失败:");
                //        return;
                //    }
    
                //    Bitmap subBmp = task.Result;
                //    Bitmap croppedImage = Folding3DUtils.GetSubBitmap(subBmp, new Rectangle(276, 177, 248, 248));
                //    imgList.Add(croppedImage);
                //});
                Invoke(new Action(() =>
                    {
                        RenderThreadFrameBuffer();
                    }));
                lock (_lockObj)
                {
                    if (_threadRenderBmp == null)
                    {
                        int err = 0;
                    }
                    Bitmap croppedImage = Folding3DUtils.GetSubBitmap(_threadRenderBmp, new Rectangle(276, 177, 248, 248));
                    imgList.Add(croppedImage);
                }

            }

            RestoreOpacity();
            ResetFaceTextIdx();
 
            foreach (Bitmap bmp in bmps)
                bmp.Dispose();

            return imgList;
        }


        private List<Image> DoCreate2DTo3D(_3DFaceImageType imgType, out int answerId, out List<Image> firstrow, bool isConsistent = true, bool isHard = false)
        {
            Bitmap[] bmps = GenerateImgs(imgType);
            List<Image> imgList = new List<Image>();
            firstrow = new List<Image>();
            answerId = _rand.Next() % 4;
            Invoke(new Action(() =>
            {
                InitTexture(bmps);
            }));

            int randGroup = _rand.Next(CubeNet.GroupNets.Count);
            int randId = _rand.Next(CubeNet.GroupNets[randGroup].Length);
            CubeNet randNet = CubeNet.GroupNets[randGroup][randId];

            firstrow.Add(CubeNetDrawer.DrawCubeNet(randNet, bmps, 100, false));
        //    List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(comboBox1.Items.Count - 1, 4);

            List<int> idList = new List<int>();
            //comboBox1 一共24次位置
            if (isHard)
            {
                List<int> groupList = Folding3DUtils.GetUniqueRandomNumbers(5, 4);
                foreach (int group in groupList)
                    idList.Add(group * 4 + _rand.Next(4));
            }
            else
            {
                int group = _rand.Next(6);
                List<int> tmpList = Folding3DUtils.GetUniqueRandomNumbers(3, 4);
                foreach (int tmp in tmpList)
                    idList.Add(group * 4 + tmp);
            }

            BackupOpacity();
            opacity = 0.8f;
            if (isConsistent) //选择不符合条件的一个。
            {
                for (int i = 0; i < 4; i++)
                {

                    currentTransform = cubePositions[idList[i]];
                    rotationX = 0;
                    rotationY = 0;
                    glControl1.Invalidate();
                    ResetFaceTextIdx();
                    if (i == answerId)
                    {
                        ConfuseFaceIdx(idList[i]);

                    }
                    else
                    {

                    }

                    Invoke(new Action(() =>
                    {
                        RenderThreadFrameBuffer();
                    }));
                    lock (_lockObj)
                    {
                        if (_threadRenderBmp == null)
                        {
                            int err = 0;
                        }
                        Bitmap croppedImage = Folding3DUtils.GetSubBitmap(_threadRenderBmp, new Rectangle(276, 177, 248, 248));
                        imgList.Add(croppedImage);
                    }


                }
            }
            else
            {

                List<int[]> confuseList = new List<int[]>();
                if(isHard)
                    confuseList = GetConfuseFaceIdx(4);
                else
                    confuseList = GetConfuseFaceIdx(idList[0],4);
                for (int i = 0; i < 4; i++)
                {

                    currentTransform = cubePositions[idList[i]];
                    rotationX = 0;
                    rotationY = 0;
                    glControl1.Invalidate();
                    ResetFaceTextIdx();
                    if (i != answerId)
                    {
                        _faceTexIdx = confuseList[i];
                        //   ConfuseFaceIdx();

                    }
                    else
                    {

                    }

                    Invoke(new Action(() =>
                    {
                        RenderThreadFrameBuffer();
                    }));
                    lock (_lockObj)
                    {
                        if (_threadRenderBmp == null)
                        {
                            int err = 0;
                        }
                        Bitmap croppedImage = Folding3DUtils.GetSubBitmap(_threadRenderBmp, new Rectangle(276, 177, 248, 248));
                        imgList.Add(croppedImage);
                    }


                }
            }

            RestoreOpacity();
            ResetFaceTextIdx();
 
 
            foreach (Bitmap bmp in bmps)
                bmp.Dispose();


            return imgList;
        }

        private List<Image> DoCreate3DTo2D(_3DFaceImageType imgType, out int answerId, out List<Image> firstrow, bool isConsistent = true, bool isHard= false)
        {
            Bitmap[] bmps = GenerateImgs(imgType);
            List<Image> imgList = new List<Image>();
            firstrow = new List<Image>();
            answerId = _rand.Next() % 4;
            Invoke(new Action(() =>
            {
                InitTexture(bmps);
            }));

            List<int> idList = Folding3DUtils.GetUniqueRandomNumbers(CubeNet.GroupNets.Count - 1, 4);

            List<int> tmpList = Folding3DUtils.GetUniqueRandomNumbers(CubeNet.GroupNets[0].Length-1, 4);

            for (int i = 0; i < 4; i++)
            {
                CubeNet randNet = CubeNet.GroupNets[idList[i]][_rand.Next(CubeNet.GroupNets[idList[i]].Length)];

                if (!isHard)
                {
                    randNet = CubeNet.GroupNets[idList[0]][tmpList[i]];
                }

                bool needConfuse = true;  //是否混淆

                if (isConsistent)
                {
                    needConfuse = i == answerId;
                }
                else
                {
                    needConfuse = i != answerId;
                }
                if (needConfuse)
                {
                    imgList.Add(CubeNetDrawer.DrawCubeNet(CubeNet.CreateRandomUnSimlar(randNet), bmps, 100, false));
                }
                else
                {
                    imgList.Add(CubeNetDrawer.DrawCubeNet(randNet, bmps, 100, false));

                }


            }




            BackupOpacity();
            opacity = 0.8f;
            //     if (checkBox1.Checked) //选择不符合条件的一个。
            {
                //       for (int i = 0; i < 4; i++)
                {

                    currentTransform = cubePositions[_rand.Next(cubePositions.Count)];
                    rotationX = 0;
                    rotationY = 0;
                    glControl1.Invalidate();
                    ResetFaceTextIdx();

                    Invoke(new Action(() =>
                    {
                        RenderThreadFrameBuffer();
                    }));
                    lock (_lockObj)
                    {
                        if (_threadRenderBmp == null)
                        {
                            int err = 0;
                        }
                        Bitmap croppedImage = Folding3DUtils.GetSubBitmap(_threadRenderBmp, new Rectangle(276, 177, 248, 248));
                        firstrow.Add(croppedImage);
                    }



                }

            }

            RestoreOpacity();
            ResetFaceTextIdx();


            foreach (Bitmap bmp in bmps)
                bmp.Dispose();

            return imgList;
        }

        private Bitmap RenderInMainThread(int width, int height)
        {
            if (this.InvokeRequired)
            {
                return (Bitmap)this.Invoke(new Func<int, int, Bitmap>(RenderInMainThread), width, height);
            }

            return RenderFrameBuffer(width, height); // 只能在主线程调用的方法
        }

        private object _lockObj = new object();

        private void RenderThreadFrameBuffer()
        {
            lock(_lockObj)
                _threadRenderBmp = RenderFrameBuffer(800, 600);
        }



        private Bitmap[] GenerateImgs(_3DFaceImageType imgType)
        {
            Bitmap[] bmps = null;

            switch (imgType)
            {
                case _3DFaceImageType.Basic:
                    {
                        List<Bitmap> bmpList = new List<Bitmap>();
                        for (int i = 0; i < 6; i++)
                            bmpList.Add(OrigamiConfuseScheme.GenRandomBitmap(256));
                        bmps = bmpList.ToArray();
                    }
                    break;
                case _3DFaceImageType.Char:
                    {
                        List<string> candidateList = new List<string>();
                        candidateList.Add("123456");
                        candidateList.Add("ABCDEF");
                        candidateList.Add("FLIGHT");
                        candidateList.Add("HZNUER");
                        candidateList.Add("FLOWER");
                        candidateList.Add("PLANET");
                        
                        candidateList.Add("子丑寅卯辰巳");
                        candidateList.Add("金木水火土阴");
                        candidateList.Add("甲乙丙丁戊己");
                        candidateList.Add("子丑寅卯辰巳");
                        candidateList.Add("鼠牛虎兔龙蛇");
                        candidateList.Add("礼乐射御书数");
                        candidateList.Add("东南西北上中");
                        candidateList.Add("乾坤震巽坎离");
                        candidateList.Add("孝悌忠信礼义");

                        int id = _rand.Next(candidateList.Count);

                        bmps = RandomImageGenerator.GenerateAllFromText(candidateList[id]);
                    }
                    break;
                case _3DFaceImageType.Line:
                    {
                        var lineOptions = new ImageGenerationOptions
                        {
                            Categories = ImageCategory.Lines,
                            LineWidth = 6,
                            Size = 150
                        };
                        bmps = RandomImageGenerator.Generate6Images(lineOptions);
                    }
                    break;
                case _3DFaceImageType.Mosaic:
                    {
                        ImageGenerationOptions options = new ImageGenerationOptions();
                        options.Categories = ImageCategory.Mosaic; // 仅生成填充纹理（含渐变色）
                        options.Size = 200; // 图片尺寸200x200像素（可选，默认256）
                        // 可选：如需提高渐变色比例，可在GenerateFillTextureImage中调整权重（见后文）
                        bmps = RandomImageGenerator.Generate6Images(options);
                    }
                    break;
                case _3DFaceImageType.Pattern:
                    {
                        ImageGenerationOptions options = new ImageGenerationOptions();
                        options.Categories = ImageCategory.FillTexture; // 仅生成填充纹理（含渐变色）
                        options.Size = 200; // 图片尺寸200x200像素（可选，默认256）
                        Bitmap[] gradientImages = RandomImageGenerator.Generate6Images(options);
                    }
                    break;
                case _3DFaceImageType.Shape:
                    {
                        ImageGenerationOptions options = new ImageGenerationOptions();
                        options.Categories = ImageCategory.Geometric; // 只生成几何图形
                        // 默认：随机颜色、尺寸256x256、不允许重复
                        // 2. 生成6张不同的几何图片
                        bmps = RandomImageGenerator.Generate6Images(options);
                    }
                    break;
                case _3DFaceImageType.Symmetric:
                    {
                        ImageGenerationOptions options = new ImageGenerationOptions();
                        options.Categories = ImageCategory.SymmetricGeometric; // 只生成几何图形
                        // 默认：随机颜色、尺寸256x256、不允许重复
                        // 2. 生成6张不同的几何图片
                        bmps = RandomImageGenerator.Generate6Images(options);
                    }
                    break;
            
            }

            return bmps;
        }
    }

    public static class TaskExtensions
    {
        // 为 .NET 4.0 提供兼容的 Task.Run 方法
        public static Task Run(this TaskFactory factory, Action action)
        {
            return factory.StartNew(action);
        }

        public static Task<TResult> Run<TResult>(this TaskFactory factory, Func<TResult> function)
        {
            return factory.StartNew(function);
        }
    }
}
