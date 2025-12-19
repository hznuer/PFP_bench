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
    public partial class ExportTypeAForm : Form
    {

        CheckBox[] _checkTemplates;
        exportSetting _exportSetting = new exportSetting();
        string[] _exts = new string[] { ".png", ".jpg", ".bmp" };
        public ExportTypeAForm()
        {
            InitializeComponent();
        }

        private void ExportTypeAForm_Load(object sender, EventArgs e)
        {
            InitTemplateUI();
            string path = Application.StartupPath + "\\Datasets";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "\\Type-A";
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
            labelExportInf.Visible = true;
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
            
            _stopExport = false;

            (new Thread(DoExport)).Start();
            btnExport.Enabled = false;
            progressBar1.Value = 0;
        }

        private bool _isExporting = false;

        private bool _stopExport = false;

        private void DoExport()
        {
            _isExporting = true;
            int id = 0;
            int totalNum = 0;
            Image unknowImg = null;
            StreamWriter writer = null;

            try
            {
                // 初始化图片资源
                unknowImg = Bitmap.FromFile(Application.StartupPath + "\\unknown.bmp");
                string savePath = _exportSetting._path;
                string jsonlPath = savePath + "\\answer.jsonl";
                writer = new StreamWriter(jsonlPath);
                string saveImgPath = savePath + "\\imgs";
                if (!Directory.Exists(saveImgPath))
                    Directory.CreateDirectory(saveImgPath);

                foreach (string template in _exportSetting._templateList)
                {
                    string path = Application.StartupPath + "\\Templates\\" + template + ".xml";
                    List<OrigamiStep> stepList = OrigamiEngine.ParseFile(path);

                    for (int i = 0; i < stepList.Count; i++)
                    {
                        progressBar1.Value = (int)(100 * ((id + (float)(i) / stepList.Count) / _exportSetting._templateList.Count));
                        Thread.Sleep(50);
                        int nfolderTime = (stepList[i] as OrigamiStart).GetFoldingCount();

                        OrigamiStart startStep = stepList[i] as OrigamiStart;

                        if (_stopExport)
                            break;

                        if (nfolderTime > 0 && nfolderTime < 4)
                        {
                            FoldingExportParam foldingParam = _exportSetting._foldingExportParams[nfolderTime - 1];
                            try
                            {
                                if (foldingParam._enable)
                                {
                                    int num = foldingParam._exportNum;
                                    int candidateNum = foldingParam._candidateNum;

                                    OrigamiCut cutStep = startStep.GetLastStep() as OrigamiCut;
                                    // 获取图片列表，后续手动释放其中的Image对象
                                    List<Image> foldBmpList = OrigamiEngine.RenderImageList(startStep, false);

                                    try
                                    {
                                        foldBmpList.RemoveAt(foldBmpList.Count - 1);

                                        List<OrigamiCutObjs> cutObjsList = OrigamiConfuseScheme.RandomCreateCutObjsList(cutStep, num);

                                        if (cutStep != null)
                                        {
                                            for (int k = 0; k < cutObjsList.Count; k++)
                                            {
                                                if (_stopExport) break;

                                                cutStep._cutObjs = cutObjsList[k];
                                                OrigamiConfuseScheme.RenderOneFold(cutStep, candidateNum);
                                                if (cutStep._cutObjs._candidateList.Count < 4)
                                                    continue;

                                                // 临时图片对象，用完立即释放
                                                Image cutRender = null;
                                                Bitmap result = null;
                                                List<Image> tempImages = new List<Image>();

                                                try
                                                {
                                                    cutRender = cutStep.Render();
                                                    tempImages.Add(cutRender);

                                                    // 构建图片列表（仅引用，不重复创建）
                                                    List<Image> imgList = new List<Image>();
                                                    imgList.AddRange(foldBmpList);
                                                    imgList.Add(cutRender);
                                                    imgList.AddRange(cutStep._cutObjs._candidateList);

                                                    List<Image> firstRow = new List<Image>();
                                                    firstRow.AddRange(foldBmpList);
                                                    firstRow.Add(cutRender);
                                                    firstRow.Add(unknowImg);

                                                    List<Image> candidates = new List<Image>(cutStep._cutObjs._candidateList);
                                                    List<int> tmpList = new List<int>();
                                                    candidates = ShuffleImages(candidates, out tmpList);

                                                    result = ImageComposer.Compose(firstRow, candidates);
                                                    tempImages.Add(result);

                                                    char answer = (char)('A' + tmpList[0]);
                                                    string filename = template + "-id_" + i + "-fold_" + nfolderTime + "-sample_" + k + "-candidate_" + candidates.Count + "-answ_" + answer + _exportSetting._ext;

                                                    result.Save(saveImgPath + "\\" + filename);
                                                    labelExportInf.Text = "Write file: "+filename;
                                                    string jsonLine = string.Format(
                                                        "{{\"image\":\"./imgs/{0}\",\"answer\":\"{1}\"}}",
                                                        filename,
                                                        answer
                                                    );
                                                    writer.WriteLine(jsonLine);
                                                    writer.Flush(); // 及时写入，避免缓存溢出
                                                    Thread.Sleep(5);
                                                    totalNum++;
                                                }
                                                finally
                                                {
                                                    // 释放当前循环创建的图片
                                                    if (result != null) result.Dispose();
                                                    if (cutRender != null) cutRender.Dispose();

                                                    // 释放候选列表中动态生成的图片（如果是新创建的）
                                                    foreach (Image img in cutStep._cutObjs._candidateList)
                                                    {
                                                        if (!foldBmpList.Contains(img) && img != unknowImg)
                                                            img.Dispose();
                                                    }
                                                    cutStep._cutObjs._candidateList.Clear();
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        // 释放foldBmpList中的所有Image对象
                                        if (foldBmpList != null)
                                        {
                                            foreach (Image img in foldBmpList)
                                                img.Dispose();
                                            foldBmpList.Clear();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // 记录异常但不中断整体流程
                            //    Console.WriteLine($"Error processing template {template}, step {i}: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                    }
                    id++;
                    if (_stopExport)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error during export: {ex.Message}");
            }
            finally
            {
                // 最终确保所有资源释放
                if (unknowImg != null)
                    unknowImg.Dispose();
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                    writer.Dispose();
                }
                _isExporting = false;
                labelExportInf.Visible = false;
            }

            // 更新UI状态
            progressBar1.Value = _stopExport ? progressBar1.Value : 100;

            Invoke(new Action(() =>
            {
                labelExportInf.Visible = false;
                btnExport.Enabled = true;
                string message = _stopExport ? "Export project terminated!" : $"Export project completed! Export count: {totalNum}";
                MessageBox.Show(this, message, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _stopExport = true;
            Thread.Sleep(300);
        }


        private void ExportTypeAForm_FormClosing(object sender, FormClosingEventArgs e)
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

            // Create original index array (initialized to -1)
            originalIndices = new List<int>(new int[count]);
            for (int i = 0; i < count; i++)
            {
                originalIndices[i] = -1;
            }

            // Create random number generator


            // Create new list and copy elements from original list
            List<Image> shuffledList = new List<Image>(candidates);

            // Fisher-Yates shuffle algorithm
            for (int i = count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);

                // Swap images
                Image temp = shuffledList[i];
                shuffledList[i] = shuffledList[j];
                shuffledList[j] = temp;
            }

            // Record the position of each original index in the new list
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
    }
}
