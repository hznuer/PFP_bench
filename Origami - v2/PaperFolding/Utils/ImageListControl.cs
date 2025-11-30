using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Origami
{
    public partial class ImageListControl : UserControl
    {
        private List<Image> images = new List<Image>();
        private int selectedIndex = -1;
        private const int imagePadding = 5;
        private const int imageSize = 100;
        private HScrollBar hScrollBar;
        private int scrollValue = 0;

        // 定义选中图像改变事件
        public event EventHandler SelectedImageChanged;

        public ImageListControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            InitializeComponent();
            hScrollBar = new HScrollBar();
            hScrollBar.Dock = DockStyle.Bottom;
            hScrollBar.ValueChanged += HScrollBar_ValueChanged;
            this.Controls.Add(hScrollBar);
        }

        public void BindImages(List<Image> newImages)
        {
            images = newImages;
            UpdateScrollBar();
            Invalidate();
        }

        public Image SelectedImage
        {
            get {
                try
                {
                    if(selectedIndex>=0 && selectedIndex<images.Count())
                        return images[selectedIndex];

                }
                catch { }
                return null;
            }

            
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (value >= -1 && value < images.Count && value != selectedIndex)
                {
                    int oldIndex = selectedIndex;
                    selectedIndex = value;
                    Invalidate();
                    // 触发选中图像改变事件
                    OnSelectedImageChanged(new EventArgs());
                }
            }
        }

        private void UpdateScrollBar()
        {
            int totalWidth = images.Count * (imageSize + imagePadding);
            if (totalWidth > this.Width)
            {
                hScrollBar.Maximum = totalWidth - this.Width;
                hScrollBar.Visible = true;
            }
            else
            {
                hScrollBar.Visible = false;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            for (int i = 0; i < images.Count; i++)
            {
                int x = i * (imageSize + imagePadding) - scrollValue;
                if (x + imageSize < 0 || x > this.Width)
                {
                    continue;
                }
                Rectangle rect = new Rectangle(x, 0, imageSize, imageSize);
                if (i == selectedIndex)
                {
                    g.DrawRectangle(Pens.Red, rect);
                }
                if (images[i] != null)
                {
                    g.DrawImage(images[i], rect);
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int index = (e.X + scrollValue) / (imageSize + imagePadding);
            if (index < images.Count)
            {
                SelectedIndex = index;
            }
        }

        private void HScrollBar_ValueChanged(object sender, EventArgs e)
        {
            scrollValue = hScrollBar.Value;
            Invalidate();
        }
        // 触发选中图像改变事件的方法
        protected virtual void OnSelectedImageChanged(EventArgs e)
        {
            if(SelectedImageChanged!=null)
                SelectedImageChanged(this, e); 
        }
    }
}
