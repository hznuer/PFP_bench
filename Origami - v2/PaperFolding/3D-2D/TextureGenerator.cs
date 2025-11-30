using System;
using System.Drawing;
using System.Drawing.Text;
using Tao.OpenGl;
using System.Drawing.Imaging;

namespace Origami
{
    // 纹理生成工具类
    public class TextureGenerator
    {
        private uint[] textures = new uint[6];
        private bool initialized = false;

        public void Initialize()
        {
            if (initialized) return;

            Gl.glGenTextures(6, textures);

            for (int i = 0; i < 6; i++)
            {
                CreateNumberTexture(i + 1, textures[i]);
            }

            initialized = true;
        }

        private void CreateNumberTexture(int number, uint textureId)
        {
            // 创建一个256x256的位图，绘制数字
            Bitmap bmp = new Bitmap(256, 256);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // 填充背景为白色
                g.Clear(Color.White);

                // 设置字体和绘制数字
                Font font = new Font("Arial", 120, FontStyle.Bold);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(number.ToString(), font, Brushes.Black, 128, 128, format);
            }

            // 将位图数据转换为OpenGL纹理
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, 256, 256),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureId);
            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, 256, 256, 0,
                           Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, bmpData.Scan0);

            // 设置纹理参数
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

            bmp.UnlockBits(bmpData);
            bmp.Dispose();
        }

        public void BindTexture(int faceIndex)
        {
            if (!initialized) Initialize();
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[faceIndex]);
        }

        public void Dispose()
        {
            if (initialized)
            {
                Gl.glDeleteTextures(6, textures);
                initialized = false;
            }
        }

    }
}