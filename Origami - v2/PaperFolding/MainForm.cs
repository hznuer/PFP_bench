using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;

namespace Origami
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadReasoningIntro();
        }




        private void template2DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Template2DForm frm = new Template2DForm();
            frm.ShowDialog();
        }


        private void template3DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Template3DForm frm = new Template3DForm();
            frm.ShowDialog();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox box = new AboutBox();
            box.ShowDialog();
        }

        private void LoadReasoningIntro()
        {
            // 清空原有内容
            richTextBoxIntro.Clear();

            // 1. 设置标题样式（特殊颜色+放大字体）
            string title = "Paper Folding Puzzles: Can Multimodal Large Language Models Perform Spatial Reasoning?";
            richTextBoxIntro.AppendText(title + Environment.NewLine + Environment.NewLine);
            // 选中标题文本并设置样式
            richTextBoxIntro.Select(0, title.Length);
            richTextBoxIntro.SelectionFont = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point); // 放大字体（11号加粗）
            richTextBoxIntro.SelectionColor = Color.FromArgb(0, 80, 160); // 深蓝色（实验室风格，可替换为其他色值）
            richTextBoxIntro.Select(richTextBoxIntro.TextLength, 0); // 重置光标位置，避免影响后续文本

            // 2. 设置2D推理内容（默认样式）
            string reasoning2D = @"2D Reasoning:
- Core: Symmetric folds (horizontal/vertical/diagonal) + cutout pattern prediction
- Features: 1-3 step folding, 4 cutout shapes, symmetry-based generation
- Application: Evaluate sequential spatial reasoning" + Environment.NewLine + Environment.NewLine;
            richTextBoxIntro.AppendText(reasoning2D);

            // 3. 设置3D推理内容（默认样式）
            string reasoning3D = @"3D Reasoning:
- Core: Bidirectional 2D-3D transformation (fold/unfold cubes)
- Features: 11 nets, 30 surface patterns, 24 viewpoints, easy/hard modes
- Application: Test 3D spatial comprehension & perspective reasoning" + Environment.NewLine + Environment.NewLine;
            richTextBoxIntro.AppendText(reasoning3D);

            // 4. 设置邮箱（可单独加浅灰色突出）
            string email = "Email:dibinz@zju.edu.cn";
            richTextBoxIntro.AppendText(email);
            // 选中邮箱文本设置浅灰色
            richTextBoxIntro.Select(richTextBoxIntro.TextLength - email.Length, email.Length);
            richTextBoxIntro.SelectionColor = Color.Gray;
            richTextBoxIntro.Select(richTextBoxIntro.TextLength, 0);

            // 禁用编辑（保持只读）
            richTextBoxIntro.ReadOnly = true;
        }

    }
}
