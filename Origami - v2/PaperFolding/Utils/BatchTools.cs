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
    public class BatchTools
    {

        public static  List<OrigamiStep> CreateAllFoldingSamples()
        {
            List<OrigamiStep> stepList = new List<OrigamiStep>();





            return null;
        }

        public static List<OrigamiStep> CreateFoldingSamplesSquare()
        {
            List<OrigamiStep> stepList = new List<OrigamiStep>();
            OrigamiStart startStep;

            //sample 1: 一折：
            {
                startStep = new OrigamiStart(new Size(360, 360), OrigamiPaperType.UnRegular, OrigamiPaper.CreatePaper(PaperType.Square));
                stepList.Add(startStep);
                OrigamiFolding step1 = new OrigamiFolding();
                startStep.AppendLastStep(step1);
                step1.DoClip(new FoldingParam(FoldingType.Hori_Right, 0.5f, new LineSegment2DF()));
                OrigamiCut cutStep = new OrigamiCut();
                startStep.AppendLastStep(cutStep);
                
                //OrigamiFolding step2 = new OrigamiFolding();
                //startStep.AppendLastStep(step2);
                //step2.DoClip(new FoldingParam(FoldingType.Vert_Top, 0.5f, new LineSegment2DF()));


            }
            //sample 2:





            return stepList;
        }


    }
}
