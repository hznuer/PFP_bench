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

namespace Origami
{
    public partial class ExportTypeBForm : Form
    {

        CheckBox[] _checkTemplates;
        exportSetting _exportSetting = new exportSetting();
        string[] _exts = new string[] { ".png", ".jpg", ".bmp" };
        public ExportTypeBForm()
        {
            InitializeComponent();
        }

        private void ExportTypeBForm_Load(object sender, EventArgs e)
        {
            InitTemplateUI();
            string path = Application.StartupPath + "\\Datasets";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "\\Type-B";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "\\" + DateTime.Now.ToString("yyyy-MM-dd");//_HH-mm-ss");
                DateTime now = DateTime.Now;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            textPath.Text = path;
            progressBar1.Visible = false;
            foreach(string ext in _exts)
                comboBox1.Items.Add(ext);
            comboBox1.SelectedIndex = 0;

        }

        private void InitTemplateUI()
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
            List<CheckBox> checkList = new List<CheckBox>();
            foreach (string filePath in xmlFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                CheckBox check = new CheckBox();
                check.Text = fileName;
                check.Parent = groupBox2;
                check.Location = new Point(30, 20 + (check.Height + 10) * checkList.Count);
                checkList.Add(check);
            }
            _checkTemplates = checkList.ToArray();
                 
        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                textPath.Text = dlg.SelectedPath;

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;

            _exportSetting._templateList.Clear();

            foreach (CheckBox check in _checkTemplates)
                if (check.Checked)
                    _exportSetting._templateList.Add(check.Text);

            _exportSetting._foldingExportParams[0]._enable = checkBox1.Checked;
            _exportSetting._foldingExportParams[1]._enable = checkBox2.Checked;
            _exportSetting._foldingExportParams[2]._enable = checkBox3.Checked;

            _exportSetting._foldingExportParams[0]._exportNum = GlobalUtils.IntParse(textBox1.Text);
            _exportSetting._foldingExportParams[1]._exportNum = GlobalUtils.IntParse(textBox2.Text);
            _exportSetting._foldingExportParams[2]._exportNum = GlobalUtils.IntParse(textBox3.Text);

            _exportSetting._foldingExportParams[0]._candidateNum = GlobalUtils.IntParse(textBox6.Text);
            _exportSetting._foldingExportParams[1]._candidateNum = GlobalUtils.IntParse(textBox5.Text);
            _exportSetting._foldingExportParams[2]._candidateNum = GlobalUtils.IntParse(textBox4.Text);

            _exportSetting._path = textPath.Text;
            _exportSetting._ext = comboBox1.SelectedItem.ToString();

            if (_exportSetting._templateList.Count == 0)
            {
                MessageBox.Show("No Template Selected!");
                return;
            }
            if (!_exportSetting._foldingExportParams[0]._enable && !_exportSetting._foldingExportParams[1]._enable && !_exportSetting._foldingExportParams[2]._enable)
            {
                MessageBox.Show("No Folding Selected!");
                return;
            }

            if (!checkCutObj1.Checked && !checkCutObj2.Checked && !checkCutObj3.Checked && !checkCutObj4.Checked)
            {
                return;
            }
            
            _stopExport = false;

            (new Thread(DoExport)).Start();
            btnExport.Enabled = false;
            labelExportInf.Visible = true;
            progressBar1.Value = 0;
        }

        private bool _isExporting = false;

        private bool _stopExport = false;

        /*
         *  导出逻辑：
         *  任选一个布局X。
         *  了解其是一折，二折，三折。
         *  
         *   随机生成若干的镂空形状。 OK的
         *   
         *   然后，选择X的同类布局，任选4-1=3组。
         *   表示成为候选答案。命名规则：
         *   
         *   生成图片的模式不一样。
         *   
         * 
         * 
         * newId  输出，表示对应的答案值。0 表示A,1表示B
         * List<int> 是随机打乱的
         */

        private List<int> SelectRand(List<int> idList, int id, int num, out int newId)
        {
            newId = 0;
            List<int> tmpList = new List<int>(idList);
            List<int> retList = new List<int>();

            tmpList.Remove(id);
        //    retList.Add(id);
            if (num>1&&tmpList.Count >= num - 1)
            {

                while (retList.Count < num-1)
                {
                    int tmp = _random.Next()%tmpList.Count;
                    retList.Add(tmpList[tmp]);
                    tmpList.RemoveAt(tmp);
                }

                newId = _random.Next() % (num);
                retList.Insert(newId, id);
 
            }

            return retList;
 
        }

        private List<int> GetShuffleImgIdList(out int newid )
        {
            newid = _random.Next() % 4;
            List<int> retList = new List<int>();

            List<int> tmpList = new List<int> { 1, 2, 3 };

            while (retList.Count < 3)
            {
                int id = _random.Next() % tmpList.Count;
                retList.Add(tmpList[id]);
                tmpList.RemoveAt(id);
            }
            return retList;
        }


        private double[] GetNumRate()
        {
            double[] rates = new double[4]{0,0,0,0};
            if (checkCutObj1.Checked)
                rates[0] =Math.Max(1, GlobalUtils.IntParse(textRate1.Text));

            if (checkCutObj2.Checked)
                rates[1] = Math.Max(1,GlobalUtils.IntParse(textRate2.Text));

            if (checkCutObj3.Checked)
                rates[2] = Math.Max(1,GlobalUtils.IntParse(textRate3.Text));

            if (checkCutObj4.Checked)
                rates[3] = Math.Max(1,GlobalUtils.IntParse(textRate4.Text));

            double sum = rates.Sum();
            if (sum < 0)
                sum += 1;
            for (int i = 0; i < 4; i++)
                rates[i] = rates[i] / sum;

            return rates;

        }

        private void DoExport()
        {
            _isExporting = true;
            int id = 0;
            int totalNum = 0;
            Image unknowImg = Bitmap.FromFile(Application.StartupPath+"\\unknown.bmp");
            
            string savePath = _exportSetting._path;
            string jsonlPath = savePath+ "\\answer.jsonl"; // 输出文件路径
            StreamWriter writer = new StreamWriter(jsonlPath);
            OrigamiConfuseScheme._objProbabilities = GetNumRate();
            string saveImgPath = savePath + "\\imgs";
            if (!Directory.Exists(saveImgPath))
                Directory.CreateDirectory(saveImgPath);
            foreach (string template in _exportSetting._templateList)
            {
                string path = Application.StartupPath + "\\Templates\\" + template + ".xml";
                List<OrigamiStep>  stepList = OrigamiEngine.ParseFile(path);

                for (int i = 0; i < stepList.Count; i++)
                {
                    progressBar1.Value = (int)(100*( (id + (float) (i)/stepList.Count)   / _exportSetting._templateList.Count));
                    Thread.Sleep(50);
                    int nfolderTime = (stepList[i] as OrigamiStart).GetFoldingCount();

                    OrigamiStart startStep = stepList[i] as OrigamiStart;

                    if (_stopExport)
                        break;

                    if (nfolderTime == 3 && startStep._confuseList.Count>0)
                    {
                        FoldingExportParam foldingParam = _exportSetting._foldingExportParams[nfolderTime - 1];
                        try
                        {
                            if (foldingParam._enable)
                            {
                                int num = foldingParam._exportNum;

                                List<Image> srcList = OrigamiEngine.RenderImageList(startStep, false);
                                srcList.RemoveAt(srcList.Count - 1);
                                List<Image> candidateList = startStep.GetConfuseImageList();

                                for (int k = candidateList.Count - 1; k >= 0; k--)
                                {
                                    foreach (Image src in srcList)
                                    {
                                        if (candidateList.Count > k)
                                        {
                                            if (OrigamiEngine.GetImgSSMDiff(new Bitmap(src), new Bitmap(candidateList[k])) < 50)
                                            {
                                                candidateList.RemoveAt(k);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            int err = 0;
                                        }
                                    }
                                }


                                OrigamiCut cutStep = startStep.GetLastStep() as OrigamiCut;
                                List<Image> foldBmpList = OrigamiEngine.RenderImageList(startStep, false);
                                foldBmpList.RemoveAt(foldBmpList.Count - 1);


                                List<OrigamiCutObjs> cutObjsList = OrigamiConfuseScheme.RandomCreateCutObjsList(cutStep, num);

                       //         for (int exportId = 0; exportId < num; exportId++)
                                {

                          //          int candidateNum = foldingParam._candidateNum;

                                
                                    if (cutStep != null)
                                    {
                                        for (int k = 0; k < cutObjsList.Count; k++)
                                        {
                                            cutStep._cutObjs = cutObjsList[k];

                                            List<Image> groupList = new List<Image>();
                                            groupList.Add(srcList[0]);
                                            groupList.Add(unknowImg);
                                            groupList.Add(cutStep.RenderCutObjs());
                                            groupList.Add(cutStep.RenderAnswer());

                                            int candidateId = _random.Next() % candidateList.Count;
                                            int newId = 0;
                                            List<int> tmpIdList = GetShuffleImgIdList(out newId);

                                            List<Image> groupList2 = new List<Image>();
                                            foreach (int tmp in tmpIdList)
                                                groupList2.Add(foldBmpList[tmp]);
                                            groupList2.Insert(newId, candidateList[candidateId]);



                                            Bitmap result = ImageComposer.Compose(groupList, groupList2);
                                            char answer = (char)('A' + newId);
                                            string filename = template + "-id_" + i + "-fold_" + nfolderTime + "-sample_" + k +  "-answ_" + answer + _exportSetting._ext;
                                            //  result.Save("test\\" + Environment.TickCount+".bmp" );
       
                                            result.Save(saveImgPath + "\\" + filename );
                                            labelExportInf.Text = "Write file: " + filename;
                                            string jsonLine = string.Format(
                                              "{{\"image\":\"./imgs/{0}\",\"answer\":\"{1}\"}}",
                                              filename,
                                              answer
                                            );
                                            writer.WriteLine(jsonLine);
                                            Thread.Sleep(5);

                                            totalNum++;
                                        }
                                    }

                                }
                            }
                        }
                        catch
                        { }
                      
                    }
                }
                id++;
                if (_stopExport)
                    break;
            }
            _isExporting = false;
            writer.Flush();
            writer.Close();
            labelExportInf.Visible = false;
            if (!_stopExport)
            {
                progressBar1.Value = 100;

                Invoke(new Action(() =>
                {
 
                    btnExport.Enabled = true;
   
                    MessageBox.Show(this, "Export project completed! Export count:" + totalNum, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);

  
                }));
            }
            else
            {

                Invoke(new Action(() =>
                {
          
                    btnExport.Enabled = true;
         
                    MessageBox.Show(this, "Export project terminated!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
           
                }));
            }
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _stopExport = true;
            Thread.Sleep(300);
        }


        private void ExportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isExporting)
            {
                e.Cancel = true;
                MessageBox.Show("Please stop the data export first!");
            }
        }

        Random _random = new Random(Environment.TickCount);
        public  List<Image> ShuffleImages(List<Image> candidates, out List<int> originalIndices)
        {
            int count = candidates.Count;

           
            originalIndices = new List<int>(new int[count]);
            for (int i = 0; i < count; i++)
            {
                originalIndices[i] = -1;
            }

      
            

            List<Image> shuffledList = new List<Image>(candidates);

        
            for (int i = count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);

              
                Image temp = shuffledList[i];
                shuffledList[i] = shuffledList[j];
                shuffledList[j] = temp;
            }

          
            for (int newIndex = 0; newIndex < count; newIndex++)
            {
                Image image = shuffledList[newIndex];
                int originalIndex = candidates.IndexOf(image);
                originalIndices[originalIndex] = newIndex;
            }

            return shuffledList;
        }

        static void WriteData(string filePath, string imagePath, string answer)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                    string jsonLine = string.Format(
                      "{{\"image\":\"{0}\",\"answer\":\"{1}\"}}",
                      imagePath,
                      answer
                  );
                    writer.WriteLine(jsonLine);


            }
        }

        private void ExportTypeCForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            OrigamiConfuseScheme.RestoreRate();
        }
    }
}
