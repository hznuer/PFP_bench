using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;


namespace Origami
{
    [Serializable]
    public class OrigamiStep
    {


        public OrigamiStep _nextStep = null;
        public OrigamiStep _prevStep = null;

        public void AppendStep(OrigamiStep step)
        {
            if (step != null)
            {
                _nextStep = step;
                step._prevStep = this;
            }
        }

        public void AppendLastStep(OrigamiStep step)
        {
            OrigamiStep cur = this;
            while (cur != null)
            {
                if (cur._nextStep != null)
                    cur = cur._nextStep;
                else
                    break;
            }
            if (step != null)
            {
                cur._nextStep = step;
                step._prevStep = cur;
            }
        }

        public OrigamiStep GetRootStep()
        {
            OrigamiStep step = this;
            while (step._prevStep != null)
                step = step._prevStep;
            return step;

        }

        public OrigamiStep GetLastStep()
        {
            OrigamiStep step = this;
            while (step!= null)
            {
                if (step._nextStep != null)
                {
                    step = step._nextStep;
                }
                else
                    break;
 
            }
            return step;
        }

        public virtual Bitmap Render()
        {
            return null;
        }

        public virtual RectangleF GetBounding()
        {
            return new RectangleF();
        }

        public virtual ClipAction GetLast()
        {
            return null;
        }

        public virtual Bitmap RenderAnswer()
        {
            return null;
        }


    }
}
