﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Origami
{
    public partial class MultiTypeDisplayControl : UserControl
    {
        private List<Image> typeAImages = new List<Image>();
        private List<Image> typeBImages = new List<Image>();
        private List<Image> typeCImages = new List<Image>();
        
        private int selectedType = 0; // 0: Type-A, 1: Type-B, 2: Type-C
        private int selectedIndex = -1;
        
        private const int imagePadding = 5;
        private const int imageSize = 100;
        private HScrollBar hScrollBar;
        private int scrollValue = 0;
        
        // 定义选中图像改变事件
        public event EventHandler SelectedImageChanged;

        public MultiTypeDisplayControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            InitializeComponent();
            hScrollBar = new HScrollBar();
            hScrollBar.Dock = DockStyle.Bottom;
            hScrollBar.ValueChanged += HScrollBar_ValueChanged;
            this.Controls.Add(hScrollBar);
        }

        public void BindTypeAImages(List<Image> newImages)
        {
            typeAImages = newImages;
            UpdateScrollBar();
            Invalidate();
        }

        public void BindTypeBImages(List<Image> newImages)
        {
            typeBImages = newImages;
            UpdateScrollBar();
            Invalidate();
        }

        public void BindTypeCImages(List<Image> newImages)
        {
            typeCImages = newImages;
            UpdateScrollBar();
            Invalidate();
        }

        public int SelectedType
        {
            get { return selectedType; }
            set
            {
                if (value >= 0 && value <= 2 && value != selectedType)
                {
                    selectedType = value;
                    selectedIndex = -1; // 重置选中索引
                    UpdateScrollBar();
                    Invalidate();
                    OnSelectedImageChanged(new EventArgs());
                }
            }
        }

        public Image SelectedImage
        {
            get
            {
                try
                {
                    List<Image> currentImages = GetCurrentImages();
                    if (selectedIndex >= 0 && selectedIndex < currentImages.Count)
                        return currentImages[selectedIndex];
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
                List<Image> currentImages = GetCurrentImages();
                if (value >= -1 && value < currentImages.Count && value != selectedIndex)
                {
                    selectedIndex = value;
                    Invalidate();
                    OnSelectedImageChanged(new EventArgs());
                }
            }
        }

        private List<Image> GetCurrentImages()
        {
            switch (selectedType)
            {
                case 0: return typeAImages;
                case 1: return typeBImages;
                case 2: return typeCImages;
                default: return new List<Image>();
            }
        }

        private void UpdateScrollBar()
        {
            List<Image> currentImages = GetCurrentImages();
            int totalWidth = currentImages.Count * (imageSize + imagePadding);
            hScrollBar.Maximum = Math.Max(0, totalWidth - this.Width + imagePadding);
            hScrollBar.LargeChange = this.Width;
            hScrollBar.SmallChange = imageSize;
            hScrollBar.Value = Math.Min(scrollValue, hScrollBar.Maximum);
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
            
            // 绘制类型标签
            string[] typeLabels = { "Type-A", "Type-B", "Type-C" };
            Brush labelBrush;
            
            for (int i = 0; i < 3; i++)
            {
                labelBrush = (i == selectedType) ? Brushes.Red : Brushes.Black;
                g.DrawString(typeLabels[i], this.Font, labelBrush, new PointF(10 + i * 80, 10));
            }
            
            // 绘制分隔线
            g.DrawLine(Pens.LightGray, 0, 30, this.Width, 30);
            
            // 绘制当前类型的图像列表
            List<Image> currentImages = GetCurrentImages();
            for (int i = 0; i < currentImages.Count; i++)
            {                
                int x = i * (imageSize + imagePadding) - scrollValue;
                if (x + imageSize < 0 || x > this.Width)
                {                    
                    continue;
                }
                Rectangle rect = new Rectangle(x, 40, imageSize, imageSize);
                if (i == selectedIndex)
                {                    
                    g.DrawRectangle(Pens.Red, rect);
                }
                if (currentImages[i] != null)
                {                    
                    g.DrawImage(currentImages[i], rect);
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {            
            base.OnMouseClick(e);
            
            // 检查是否点击了类型标签区域
            if (e.Y < 30)
            {
                if (e.X >= 10 && e.X < 80)
                    SelectedType = 0;
                else if (e.X >= 90 && e.X < 170)
                    SelectedType = 1;
                else if (e.X >= 180 && e.X < 260)
                    SelectedType = 2;
                return;
            }
            
            // 点击图像区域
            if (e.Y >= 40)
            {
                int index = (e.X + scrollValue) / (imageSize + imagePadding);
                SelectedIndex = index;
            }
        }

        private void HScrollBar_ValueChanged(object sender, EventArgs e)
        {            
            scrollValue = hScrollBar.Value;
            Invalidate();
        }
        
        protected virtual void OnSelectedImageChanged(EventArgs e)
        {            
            if (SelectedImageChanged != null)
                SelectedImageChanged(this, e);
        }
    }
}
