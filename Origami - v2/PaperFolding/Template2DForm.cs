using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Origami
{
    public partial class Template2DForm : Form
    {
        private OrigamiStart startStep = null;
        private List<OrigamiStep> _stepList = new List<OrigamiStep>();

        public Template2DForm()
        {
            InitializeComponent();
        }

        private void Template2DForm_Load(object sender, EventArgs e)
        {
            splitContainer2.SplitterDistance = splitContainer2.Height * 4 / 5;
            foreach (string name in Enum.GetNames(typeof(PaperType)))
            {
                comboBox1.Items.Add(name);
            }
            comboBox1.SelectedIndex = 0;

            LoadTemplate();
        }

        private void btnExport_A_Click(object sender, EventArgs e)
        {
            ExportTypeAForm frm = new ExportTypeAForm();
            frm.ShowDialog();
        }

        private void btnExport_B_Click(object sender, EventArgs e)
        {
            ExportTypeBForm frm = new ExportTypeBForm();
            frm.ShowDialog();
        }

        private void btnExport_C_Click(object sender, EventArgs e)
        {
            ExportTypeCForm frm = new ExportTypeCForm();
            frm.ShowDialog();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
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

        private void btnTest1_Click(object sender, EventArgs e)
        {
            GenSample1();
        }

        private void btnTest2_Click(object sender, EventArgs e)
        {
            GenSample2();
        }

        private void btnTest3_Click(object sender, EventArgs e)
        {
            GenSample3();
        }

        private void btnTest4_Click(object sender, EventArgs e)
        {
            GenSample4();
        }

        private void btnTest5_Click(object sender, EventArgs e)
        {
            GenSample5();
        }

        private void btnTest6_Click(object sender, EventArgs e)
        {
            GenSample6();
        }

        private void btnTest7_Click(object sender, EventArgs e)
        {
            GenSample7();
        }

        private void btnTest8_Click(object sender, EventArgs e)
        {
            GenSample8();
        }

        private void btnTest9_Click(object sender, EventArgs e)
        {
            GenSample9();
        }

        private void btnExtraTest1_Click(object sender, EventArgs e)
        {
            GenSampleExtra1();
        }


        private void LoadTemplate()
        {
            try
            {

                List<string> result = new List<string>();
                string path = Application.StartupPath + "\\Templates";

                if (!Directory.Exists(path))
                {

                    return;
                }

                // 获取所有XML文件（搜索模式支持大小写不敏感）
                string[] xmlFiles = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly);

                // 提取文件名（不含扩展名）
                foreach (string filePath in xmlFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    comboBox2.Items.Add(fileName);
                }
            }
            catch { }
        }

        private OrigamiStart GenStartStep()
        {
            int typeId = comboBox1.SelectedIndex;
            OrigamiStart startStep = new OrigamiStart(new Size(360, 360), OrigamiPaperType.UnRegular, OrigamiPaper.CreatePaper((PaperType)typeId));


            return startStep;
        }

        private void GenSample1()
        {
            startStep = GenStartStep();

            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendLastStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Hori_Right, 0.5f, new LineSegment2DF()));
            OrigamiFolding step2 = new OrigamiFolding();
            startStep.AppendLastStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Vert_Top, 0.5f, new LineSegment2DF()));

            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);

        }

        private void GenSample2()
        {
            startStep = GenStartStep();

            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendLastStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Hori_Right, 0.5f, new LineSegment2DF()));
            OrigamiFolding step2 = new OrigamiFolding();
            startStep.AppendLastStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Hori_Right, 0.5f, new LineSegment2DF()));
            OrigamiFolding step3 = new OrigamiFolding();
            startStep.AppendLastStep(step3);
            step3.DoClip(new FoldingParam(FoldingType.Vert_Top, 0.5f, new LineSegment2DF()));

            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);
        }

        private void GenSample3()
        {
            startStep = GenStartStep();

            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Hori_Right, 0.35f, new LineSegment2DF()));

            OrigamiFolding step2 = new OrigamiFolding();
            step1.AppendStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Vert_Bottom, 0.45f, new LineSegment2DF()));

            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);
        }

        private void GenSample4()
        {
            startStep = GenStartStep();
            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendLastStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(360, 0), new PointF(0, 360))));


            OrigamiFolding step2 = new OrigamiFolding();
            startStep.AppendLastStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(180, 180), new PointF(360, 360))));

            OrigamiFolding step3 = new OrigamiFolding();
            startStep.AppendLastStep(step3);
            step3.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(180, 180), new PointF(360, 180))));


            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);
            //picStep.Image = cutStep.RenderAnswer();

        }

        private void GenSample5()
        {
            startStep = GenStartStep();

            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendLastStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Vert_Bottom, 0.3333333333333f, new LineSegment2DF()));
            OrigamiFolding step2 = new OrigamiFolding();
            startStep.AppendLastStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Vert_Top, 0.5f, new LineSegment2DF()));
            OrigamiFolding step3 = new OrigamiFolding();
            startStep.AppendLastStep(step3);
            step3.DoClip(new FoldingParam(FoldingType.Hori_Right, 0.5f, new LineSegment2DF()));

            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);
        }


        private void GenSample6()
        {
            startStep = GenStartStep();

            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendLastStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(160, 0), new PointF(0, 160))));
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(360, 160), new PointF(200, 0))));
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(200, 360), new PointF(360, 200))));
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(0, 200), new PointF(160, 360))));
            OrigamiFolding step2 = new OrigamiFolding();
            startStep.AppendLastStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Hori_Right, 0.25f, new LineSegment2DF()));
            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);
        }


        private void GenSample7()
        {
            startStep = GenStartStep();

            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendLastStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(160, 0), new PointF(0, 160))));
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(360, 160), new PointF(200, 0))));
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(200, 360), new PointF(360, 200))));
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(0, 200), new PointF(160, 360))));
            OrigamiFolding step2 = new OrigamiFolding();
            startStep.AppendLastStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Hori_Left, 0.8f, new LineSegment2DF()));
            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);
        }

        private void GenSample8()
        {
            startStep = GenStartStep();


            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendLastStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Hori_Right, 0.5f, new LineSegment2DF()));
            OrigamiFolding step2 = new OrigamiFolding();
            startStep.AppendLastStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Vert_Top, 0.5f, new LineSegment2DF()));

            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);

        }

        private void GenSample9()
        {
            try
            {

                List<OrigamiStep> stepList = OrigamiEngine.ParseFile(Application.StartupPath + "\\Templates\\square.xml");
                if (stepList.Count > 0)
                {
                    List<Image> imgList = OrigamiEngine.RenderImageList(stepList[stepList.Count - 1] as OrigamiStart, true);
                    imageListControl1.BindImages(imgList);
                }
            }
            catch
            { }
        }

        private void GenSampleExtra1()
        {
            startStep = GenStartStep();
            OrigamiFolding step1 = new OrigamiFolding();
            startStep.AppendLastStep(step1);
            step1.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(360, 0), new PointF(0, 360))));


            OrigamiFolding step2 = new OrigamiFolding();
            startStep.AppendLastStep(step2);
            step2.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(180, 360), new PointF(360, 180))), 1);

            OrigamiFolding step3 = new OrigamiFolding();
            startStep.AppendLastStep(step3);
            step3.DoClip(new FoldingParam(FoldingType.Angle, 1f, new LineSegment2DF(new PointF(360, 90), new PointF(90, 360))), 1);



            OrigamiCut cutStep = new OrigamiCut();
            startStep.AppendLastStep(cutStep);
            cutStep.RandAddObj();

            List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
            imageListControl1.BindImages(imgList);
            //picStep.Image = cutStep.RenderAnswer();

        }

        private void SwitchTemplate()
        {
            try
            {
                if (comboBox1.SelectedIndex >= 0)
                {
                    string path = Application.StartupPath + "\\Templates\\" + comboBox1.SelectedItem.ToString() + ".xml";
                    _stepList = OrigamiEngine.ParseFile(path);
                    listBox1.Items.Clear();
                    for (int i = 0; i < _stepList.Count; i++)
                        listBox1.Items.Add("" + i + ": Folding Times: " + (_stepList[i] as OrigamiStart).GetFoldingCount());
                    listBox1.SelectedIndex = -1;
                    if (_stepList.Count > 0)
                        listBox1.SelectedIndex = 0;

                }
            }
            catch
            { }
        }

        private void SwitchFolding()
        {
            try
            {
                if (listBox1.SelectedIndex >= 0)
                {
                    OrigamiStart startStep = _stepList[listBox1.SelectedIndex] as OrigamiStart;
                    List<Image> imgList = OrigamiEngine.RenderImageList(startStep, true);
                    if (checkConfuse.Checked)
                        imgList.AddRange(startStep.GetConfuseImageList());
                    imageListControl1.BindImages(imgList);


                    if (imgList.Count > 0)
                        imageListControl1.SelectedIndex = 0;

                }
            }
            catch
            { }
        }


        private void imageListControl1_Load(object sender, EventArgs e)
        {

        }

        private void imageListControl1_SelectedImageChanged(object sender, EventArgs e)
        {
            try
            {
                picStep.Image = imageListControl1.SelectedImage;
            }
            catch
            { }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            int id = listBox1.SelectedIndex;
            SwitchTemplate();
            if (id >= 0 && listBox1.Items.Count > id)
                listBox1.SelectedIndex = id;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchFolding();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        private void label6_Click(object sender, EventArgs e)
        {

        }

    }
}
