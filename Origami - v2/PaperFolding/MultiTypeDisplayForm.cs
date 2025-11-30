﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Origami
{
    public partial class MultiTypeDisplayForm : Form
    {
        private MultiTypeDisplayControl multiTypeDisplayControl1;
        private PictureBox picPreview;
        private Label labelSelectedInfo;
        private Button btnLoadTypeA;
        private Button btnLoadTypeB;
        private Button btnLoadTypeC;

        public MultiTypeDisplayForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Type-A, Type-B, Type-C 并排显示";
            this.Size = new Size(900, 600);
            
            // 创建多类型显示控件
            multiTypeDisplayControl1 = new MultiTypeDisplayControl();
            multiTypeDisplayControl1.Dock = DockStyle.Fill;
            multiTypeDisplayControl1.SelectedImageChanged += multiTypeDisplayControl1_SelectedImageChanged;
            
            // 创建预览图片框
            picPreview = new PictureBox();
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.Width = 250;
            picPreview.Height = 250;
            
            // 创建选中信息标签
            labelSelectedInfo = new Label();
            labelSelectedInfo.Text = "未选中任何图像";
            labelSelectedInfo.AutoSize = true;
            
            // 创建加载按钮
            btnLoadTypeA = new Button();
            btnLoadTypeA.Text = "加载Type-A内容";
            btnLoadTypeA.Click += btnLoadTypeA_Click;
            
            btnLoadTypeB = new Button();
            btnLoadTypeB.Text = "加载Type-B内容";
            btnLoadTypeB.Click += btnLoadTypeB_Click;
            
            btnLoadTypeC = new Button();
            btnLoadTypeC.Text = "加载Type-C内容";
            btnLoadTypeC.Click += btnLoadTypeC_Click;
            
            // 创建右侧面板
            Panel rightPanel = new Panel();
            rightPanel.Width = 280;
            rightPanel.Dock = DockStyle.Right;
            rightPanel.Padding = new Padding(10);
            
            // 创建垂直布局面板
            FlowLayoutPanel flowPanel = new FlowLayoutPanel();
            flowPanel.FlowDirection = FlowDirection.TopDown;
            flowPanel.Dock = DockStyle.Fill;
            flowPanel.WrapContents = false;
            flowPanel.AutoScroll = true;
            flowPanel.Padding = new Padding(5);
            
            // 添加控件到流布局面板
            flowPanel.Controls.Add(labelSelectedInfo);
            flowPanel.Controls.Add(new Panel() { Height = 10 }); // 间隔
            flowPanel.Controls.Add(picPreview);
            flowPanel.Controls.Add(new Panel() { Height = 10 }); // 间隔
            flowPanel.Controls.Add(btnLoadTypeA);
            flowPanel.Controls.Add(new Panel() { Height = 5 }); // 间隔
            flowPanel.Controls.Add(btnLoadTypeB);
            flowPanel.Controls.Add(new Panel() { Height = 5 }); // 间隔
            flowPanel.Controls.Add(btnLoadTypeC);
            
            // 将流布局面板添加到右侧面板
            rightPanel.Controls.Add(flowPanel);
            
            // 创建分割容器
            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Panel1.Controls.Add(multiTypeDisplayControl1);
            splitContainer.Panel2.Controls.Add(rightPanel);
            splitContainer.SplitterDistance = this.Width - 300;
            
            // 将分割容器添加到窗体
            this.Controls.Add(splitContainer);
        }

        private void multiTypeDisplayControl1_SelectedImageChanged(object sender, EventArgs e)
        {
            // 更新预览图片和选中信息
            picPreview.Image = multiTypeDisplayControl1.SelectedImage;
            
            string[] typeNames = { "Type-A", "Type-B", "Type-C" };
            int selectedType = multiTypeDisplayControl1.SelectedType;
            int selectedIndex = multiTypeDisplayControl1.SelectedIndex;
            
            if (selectedIndex >= 0)
            {
                labelSelectedInfo.Text = string.Format("已选中: {0} - 图像索引 {1}", 
                    typeNames[selectedType], selectedIndex);
            }
            else
            {
                labelSelectedInfo.Text = string.Format("当前类型: {0} - 未选中图像", 
                    typeNames[selectedType]);
            }
        }

        private void btnLoadTypeA_Click(object sender, EventArgs e)
        {
            // 从Type-A目录加载图像
            string typeADir = Path.Combine(Application.StartupPath, "Product", "Datasets", "Type-A");
            LoadImagesFromDirectory(typeADir, 0); // 0 表示 Type-A
        }

        private void btnLoadTypeB_Click(object sender, EventArgs e)
        {
            // 从Type-B目录加载图像
            string typeBDir = Path.Combine(Application.StartupPath, "Product", "Datasets", "Type-B");
            LoadImagesFromDirectory(typeBDir, 1); // 1 表示 Type-B
        }

        private void btnLoadTypeC_Click(object sender, EventArgs e)
        {
            // 从Type-C目录加载图像
            string typeCDir = Path.Combine(Application.StartupPath, "Product", "Datasets", "Type-C");
            LoadImagesFromDirectory(typeCDir, 2); // 2 表示 Type-C
        }

        private void LoadImagesFromDirectory(string directoryPath, int typeIndex)
        {
            try
            {
                List<Image> imageList = new List<Image>();
                
                if (Directory.Exists(directoryPath))
                {
                    // 获取所有图片文件
                    string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                    var imageFiles = Directory.GetFiles(directoryPath, "*.*")
                        .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                        .Take(20); // 限制加载数量以避免内存问题
                    
                    foreach (string imagePath in imageFiles)
                    {
                        try
                        {   
                            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            {
                                Image img = Image.FromStream(fs);
                                // 创建图像副本，避免文件锁定
                                imageList.Add(new Bitmap(img));
                            }
                        }
                        catch { }
                    }
                    
                    // 将加载的图像绑定到对应的类型
                    switch (typeIndex)
                    {
                        case 0: // Type-A
                            multiTypeDisplayControl1.BindTypeAImages(imageList);
                            labelSelectedInfo.Text = string.Format("已加载 {0} 张 Type-A 图像", imageList.Count);
                            break;
                        case 1: // Type-B
                            multiTypeDisplayControl1.BindTypeBImages(imageList);
                            labelSelectedInfo.Text = string.Format("已加载 {0} 张 Type-B 图像", imageList.Count);
                            break;
                        case 2: // Type-C
                            multiTypeDisplayControl1.BindTypeCImages(imageList);
                            labelSelectedInfo.Text = string.Format("已加载 {0} 张 Type-C 图像", imageList.Count);
                            break;
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("目录不存在: {0}", directoryPath), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("加载图像时出错: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
