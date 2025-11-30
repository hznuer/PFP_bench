namespace Origami
{
    partial class Template2DForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.picStep = new System.Windows.Forms.PictureBox();
            this.imageListControl1 = new Origami.ImageListControl();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnExtraTest1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btnTest7 = new System.Windows.Forms.Button();
            this.btnTest8 = new System.Windows.Forms.Button();
            this.btnTestHole = new System.Windows.Forms.Button();
            this.btnTest6 = new System.Windows.Forms.Button();
            this.btnTest5 = new System.Windows.Forms.Button();
            this.btnTest4 = new System.Windows.Forms.Button();
            this.btnTest3 = new System.Windows.Forms.Button();
            this.btnTest2 = new System.Windows.Forms.Button();
            this.btnTest1 = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.checkConfuse = new System.Windows.Forms.CheckBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.btnExport_A = new System.Windows.Forms.Button();
            this.btnExport_B = new System.Windows.Forms.Button();
            this.btnExport_C = new System.Windows.Forms.Button();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStep)).BeginInit();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl2);
            this.splitContainer1.Size = new System.Drawing.Size(1153, 771);
            this.splitContainer1.SplitterDistance = 784;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.imageListControl1);
            this.splitContainer2.Size = new System.Drawing.Size(784, 771);
            this.splitContainer2.SplitterDistance = 604;
            this.splitContainer2.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(784, 604);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.picStep);
            this.tabPage1.Location = new System.Drawing.Point(4, 33);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(776, 567);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "2D";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // picStep
            // 
            this.picStep.BackColor = System.Drawing.Color.Gray;
            this.picStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picStep.Location = new System.Drawing.Point(3, 3);
            this.picStep.Name = "picStep";
            this.picStep.Size = new System.Drawing.Size(770, 561);
            this.picStep.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picStep.TabIndex = 0;
            this.picStep.TabStop = false;
            // 
            // imageListControl1
            // 
            this.imageListControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.imageListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageListControl1.Location = new System.Drawing.Point(0, 0);
            this.imageListControl1.Name = "imageListControl1";
            this.imageListControl1.SelectedIndex = -1;
            this.imageListControl1.Size = new System.Drawing.Size(784, 163);
            this.imageListControl1.TabIndex = 2;
            this.imageListControl1.SelectedImageChanged += new System.EventHandler(this.imageListControl1_SelectedImageChanged);
            this.imageListControl1.Load += new System.EventHandler(this.imageListControl1_Load);
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(365, 771);
            this.tabControl2.TabIndex = 11;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnExtraTest1);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.comboBox1);
            this.tabPage3.Controls.Add(this.btnTest7);
            this.tabPage3.Controls.Add(this.btnTest8);
            this.tabPage3.Controls.Add(this.btnTestHole);
            this.tabPage3.Controls.Add(this.btnTest6);
            this.tabPage3.Controls.Add(this.btnTest5);
            this.tabPage3.Controls.Add(this.btnTest4);
            this.tabPage3.Controls.Add(this.btnTest3);
            this.tabPage3.Controls.Add(this.btnTest2);
            this.tabPage3.Controls.Add(this.btnTest1);
            this.tabPage3.Location = new System.Drawing.Point(4, 33);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(357, 734);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Simple";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnExtraTest1
            // 
            this.btnExtraTest1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExtraTest1.Location = new System.Drawing.Point(92, 556);
            this.btnExtraTest1.Name = "btnExtraTest1";
            this.btnExtraTest1.Size = new System.Drawing.Size(91, 36);
            this.btnExtraTest1.TabIndex = 23;
            this.btnExtraTest1.Text = "Special Fold 1";
            this.btnExtraTest1.UseVisualStyleBackColor = true;
            this.btnExtraTest1.Click += new System.EventHandler(this.btnExtraTest1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(14, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 24);
            this.label1.TabIndex = 20;
            this.label1.Text = "Paper Type";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(128, 25);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 32);
            this.comboBox1.TabIndex = 22;
            // 
            // btnTest7
            // 
            this.btnTest7.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTest7.Location = new System.Drawing.Point(92, 394);
            this.btnTest7.Name = "btnTest7";
            this.btnTest7.Size = new System.Drawing.Size(91, 36);
            this.btnTest7.TabIndex = 21;
            this.btnTest7.Text = "Fold 7";
            this.btnTest7.UseVisualStyleBackColor = true;
            this.btnTest7.Click += new System.EventHandler(this.btnTest7_Click);
            // 
            // btnTest8
            // 
            this.btnTest8.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTest8.Location = new System.Drawing.Point(92, 448);
            this.btnTest8.Name = "btnTest8";
            this.btnTest8.Size = new System.Drawing.Size(91, 36);
            this.btnTest8.TabIndex = 19;
            this.btnTest8.Text = "Fold 8";
            this.btnTest8.UseVisualStyleBackColor = true;
            this.btnTest8.Click += new System.EventHandler(this.btnTest8_Click);
            // 
            // btnTestHole
            // 
            this.btnTestHole.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTestHole.Location = new System.Drawing.Point(92, 502);
            this.btnTestHole.Name = "btnTestHole";
            this.btnTestHole.Size = new System.Drawing.Size(91, 36);
            this.btnTestHole.TabIndex = 18;
            this.btnTestHole.Text = "Fold 9";
            this.btnTestHole.UseVisualStyleBackColor = true;
            this.btnTestHole.Click += new System.EventHandler(this.btnTest9_Click);
            // 
            // btnTest6
            // 
            this.btnTest6.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTest6.Location = new System.Drawing.Point(92, 340);
            this.btnTest6.Name = "btnTest6";
            this.btnTest6.Size = new System.Drawing.Size(91, 36);
            this.btnTest6.TabIndex = 17;
            this.btnTest6.Text = "Fold 6";
            this.btnTest6.UseVisualStyleBackColor = true;
            this.btnTest6.Click += new System.EventHandler(this.btnTest6_Click);
            // 
            // btnTest5
            // 
            this.btnTest5.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTest5.Location = new System.Drawing.Point(92, 286);
            this.btnTest5.Name = "btnTest5";
            this.btnTest5.Size = new System.Drawing.Size(91, 36);
            this.btnTest5.TabIndex = 16;
            this.btnTest5.Text = "Fold 5";
            this.btnTest5.UseVisualStyleBackColor = true;
            this.btnTest5.Click += new System.EventHandler(this.btnTest5_Click);
            // 
            // btnTest4
            // 
            this.btnTest4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTest4.Location = new System.Drawing.Point(92, 232);
            this.btnTest4.Name = "btnTest4";
            this.btnTest4.Size = new System.Drawing.Size(91, 36);
            this.btnTest4.TabIndex = 15;
            this.btnTest4.Text = "Fold 4";
            this.btnTest4.UseVisualStyleBackColor = true;
            this.btnTest4.Click += new System.EventHandler(this.btnTest4_Click);
            // 
            // btnTest3
            // 
            this.btnTest3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTest3.Location = new System.Drawing.Point(92, 178);
            this.btnTest3.Name = "btnTest3";
            this.btnTest3.Size = new System.Drawing.Size(91, 36);
            this.btnTest3.TabIndex = 14;
            this.btnTest3.Text = "Fold 3";
            this.btnTest3.UseVisualStyleBackColor = true;
            this.btnTest3.Click += new System.EventHandler(this.btnTest3_Click);
            // 
            // btnTest2
            // 
            this.btnTest2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTest2.Location = new System.Drawing.Point(92, 124);
            this.btnTest2.Name = "btnTest2";
            this.btnTest2.Size = new System.Drawing.Size(91, 36);
            this.btnTest2.TabIndex = 13;
            this.btnTest2.Text = "Fold 2";
            this.btnTest2.UseVisualStyleBackColor = true;
            this.btnTest2.Click += new System.EventHandler(this.btnTest2_Click);
            // 
            // btnTest1
            // 
            this.btnTest1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTest1.Location = new System.Drawing.Point(92, 70);
            this.btnTest1.Name = "btnTest1";
            this.btnTest1.Size = new System.Drawing.Size(91, 36);
            this.btnTest1.TabIndex = 12;
            this.btnTest1.Text = "Fold 1";
            this.btnTest1.UseVisualStyleBackColor = true;
            this.btnTest1.Click += new System.EventHandler(this.btnTest1_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.checkConfuse);
            this.tabPage4.Controls.Add(this.btnUpdate);
            this.tabPage4.Controls.Add(this.label2);
            this.tabPage4.Controls.Add(this.listBox1);
            this.tabPage4.Controls.Add(this.label3);
            this.tabPage4.Controls.Add(this.comboBox2);
            this.tabPage4.Location = new System.Drawing.Point(4, 33);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(357, 734);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Batch";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // checkConfuse
            // 
            this.checkConfuse.AutoSize = true;
            this.checkConfuse.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkConfuse.Location = new System.Drawing.Point(123, 63);
            this.checkConfuse.Name = "checkConfuse";
            this.checkConfuse.Size = new System.Drawing.Size(103, 28);
            this.checkConfuse.TabIndex = 23;
            this.checkConfuse.Text = "Confuse";
            this.checkConfuse.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnUpdate.Location = new System.Drawing.Point(251, 63);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 30);
            this.btnUpdate.TabIndex = 21;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(21, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 24);
            this.label2.TabIndex = 20;
            this.label2.Text = "Folding";
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 24;
            this.listBox1.Location = new System.Drawing.Point(24, 101);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(315, 604);
            this.listBox1.TabIndex = 19;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(29, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 24);
            this.label3.TabIndex = 18;
            this.label3.Text = "Template";
            // 
            // comboBox2
            // 
            this.comboBox2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(115, 20);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(181, 32);
            this.comboBox2.TabIndex = 17;
            // 
            // btnExport_A
            // 
            this.btnExport_A.BackColor = System.Drawing.Color.Linen;
            this.btnExport_A.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExport_A.Location = new System.Drawing.Point(191, 8);
            this.btnExport_A.Name = "btnExport_A";
            this.btnExport_A.Size = new System.Drawing.Size(147, 44);
            this.btnExport_A.TabIndex = 0;
            this.btnExport_A.Text = "Type A";
            this.btnExport_A.UseVisualStyleBackColor = false;
            this.btnExport_A.Click += new System.EventHandler(this.btnExport_A_Click);
            // 
            // btnExport_B
            // 
            this.btnExport_B.BackColor = System.Drawing.Color.MistyRose;
            this.btnExport_B.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExport_B.Location = new System.Drawing.Point(191, 185);
            this.btnExport_B.Name = "btnExport_B";
            this.btnExport_B.Size = new System.Drawing.Size(147, 44);
            this.btnExport_B.TabIndex = 1;
            this.btnExport_B.Text = "Type B";
            this.btnExport_B.UseVisualStyleBackColor = false;
            this.btnExport_B.Click += new System.EventHandler(this.btnExport_B_Click);
            // 
            // btnExport_C
            // 
            this.btnExport_C.BackColor = System.Drawing.Color.PaleTurquoise;
            this.btnExport_C.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExport_C.Location = new System.Drawing.Point(191, 378);
            this.btnExport_C.Name = "btnExport_C";
            this.btnExport_C.Size = new System.Drawing.Size(147, 44);
            this.btnExport_C.TabIndex = 2;
            this.btnExport_C.Text = "Type C";
            this.btnExport_C.UseVisualStyleBackColor = false;
            this.btnExport_C.Click += new System.EventHandler(this.btnExport_C_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::Origami.Properties.Resources._3;
            this.pictureBox3.Location = new System.Drawing.Point(594, 419);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(377, 173);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 11;
            this.pictureBox3.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(560, 72);
            this.label4.TabIndex = 12;
            this.label4.Text = "Display the complete sequence of origami steps, \r\nfrom the initial sheet of paper" +
    " to the final folded and cut result,\r\n including clear step-by-step guidance and" +
    " standard answers.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 255);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(588, 96);
            this.label5.TabIndex = 13;
            this.label5.Text = "Focus on the three-fold case,\r\n provide folding step diagrams but possibly in a s" +
    "crambled order to \r\nincrease task difficulty,\r\n and generate a JSONL file with a" +
    "nswer markers.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 454);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(440, 96);
            this.label6.TabIndex = 14;
            this.label6.Text = "Supports all folding times, \r\nprovides multiple candidate answers for selection,\r" +
    "\n includes randomly generated blank shapes, \r\nfor more complex reasoning trainin" +
    "g.";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Origami.Properties.Resources._1;
            this.pictureBox1.Location = new System.Drawing.Point(594, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(377, 177);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // tabPage5
            // 
            this.tabPage5.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage5.Controls.Add(this.pictureBox2);
            this.tabPage5.Controls.Add(this.pictureBox1);
            this.tabPage5.Controls.Add(this.label6);
            this.tabPage5.Controls.Add(this.label5);
            this.tabPage5.Controls.Add(this.label4);
            this.tabPage5.Controls.Add(this.pictureBox3);
            this.tabPage5.Controls.Add(this.btnExport_C);
            this.tabPage5.Controls.Add(this.btnExport_B);
            this.tabPage5.Controls.Add(this.btnExport_A);
            this.tabPage5.Location = new System.Drawing.Point(4, 33);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(776, 567);
            this.tabPage5.TabIndex = 2;
            this.tabPage5.Text = "Export";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Origami.Properties.Resources._2;
            this.pictureBox2.Location = new System.Drawing.Point(594, 220);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(377, 168);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 16;
            this.pictureBox2.TabStop = false;
            // 
            // Template2DForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1153, 771);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Template2DForm";
            this.Text = "2D Template Files";
            this.Load += new System.EventHandler(this.Template2DForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picStep)).EndInit();
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.PictureBox picStep;
        private ImageListControl imageListControl1;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button btnExtraTest1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btnTest7;
        private System.Windows.Forms.Button btnTest8;
        private System.Windows.Forms.Button btnTestHole;
        private System.Windows.Forms.Button btnTest6;
        private System.Windows.Forms.Button btnTest5;
        private System.Windows.Forms.Button btnTest4;
        private System.Windows.Forms.Button btnTest3;
        private System.Windows.Forms.Button btnTest2;
        private System.Windows.Forms.Button btnTest1;
        private System.Windows.Forms.CheckBox checkConfuse;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button btnExport_C;
        private System.Windows.Forms.Button btnExport_B;
        private System.Windows.Forms.Button btnExport_A;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}