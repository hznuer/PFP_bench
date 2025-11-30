using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Origami
{
    public partial class CandidateForm : Form
    {
        public OrigamiStart _startStep = null;

        public List<OrigamiCutObjs> _cutObjsList = new List<OrigamiCutObjs>();
        public List<Image> _foldBmpList = new List<Image>();

        public CandidateForm(OrigamiStart step)
        {
           InitializeComponent();
            _startStep = step;
            splitContainer2.SplitterDistance = splitContainer2.Height * 4 / 5;
 
        }

        private void CandidateForm_Load(object sender, EventArgs e)
        {
            _foldBmpList = OrigamiEngine.RenderImageList(_startStep, false);
            _foldBmpList.RemoveAt(_foldBmpList.Count - 1);
        //    GenerateCandidateList();
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

        private void GenerateCandidateList()
        {
            try
            {
                int num = GetCandidateNum();
                listBox1.Items.Clear();

                OrigamiCut cutStep = _startStep.GetLastStep() as OrigamiCut;
                _cutObjsList = OrigamiConfuseScheme.RandomCreateCutObjsList(cutStep, num);
                for (int i = 0; i < _cutObjsList.Count; i++)
                    listBox1.Items.Add(i);
                if (cutStep != null)
                {
                    for (int i = 0; i < _cutObjsList.Count; i++)
                    {
                        cutStep._cutObjs = _cutObjsList[i];
                        OrigamiConfuseScheme.RenderOneFold(cutStep, 6);
                    }
                }
                listBox1.SelectedIndex = -1;
                if (listBox1.Items.Count > 0)
                    listBox1.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
            }
 
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            GC.Collect();
            GenerateCandidateList();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchFolding();
        }

        private void SwitchFolding()
        {
            try
            {
                if (listBox1.SelectedIndex >= 0)
                {
                    OrigamiCut cutStep = _startStep.GetLastStep() as OrigamiCut;
                    if (cutStep != null)
                    {
                        cutStep._cutObjs = _cutObjsList[listBox1.SelectedIndex];

                        List<Image> imgList = new List<Image>();
                        imgList.AddRange(_foldBmpList);
                        imgList.Add(cutStep.Render());
                        if (cutStep._cutObjs._candidateList.Count > 0)
                            imgList.AddRange(cutStep._cutObjs._candidateList);
                        imageListControl1.BindImages(imgList);
                        //if (imgList.Count > 0)
                        //    imageListControl1.SelectedIndex = 0;
                    }

                }
            }
            catch
            { }
        }

        private int GetCandidateNum()
        {
            try
            {
                int val = 30;
                val = GlobalUtils.IntParse(textBox1.Text, 30);
                if (val < 0)
                    val = 30;
                if (val > 200)
                    val = 200;
                return val;
            }
            catch
            { }
            return 30;
        }
    }
}
