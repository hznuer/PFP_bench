using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Origami
{
    public class exportSetting
    {
        public List<string> _templateList = new List<string>();
        public FoldingExportParam[] _foldingExportParams = new FoldingExportParam[3];
        public string _path;
        public string _ext = ".png";
    }

    public struct FoldingExportParam
    {
        public int _exportNum; // = 20;  // Export quantity
        public int _candidateNum; // = 4; // Number of candidate answers
        public bool _enable; // = true; // Enabled
    }

    public class Export3DSetting
    {
        public List<_3DExportParam> _templateList = new List<_3DExportParam>();

        public List<_3DFaceImageType> _faceImgTypeList = new List<_3DFaceImageType>();

        public float _opacity = 0.8f;

        public bool _supportMulti3D = false;

        public string _path = "";

        public string _ext = ".png";

    }

    public enum _3DExportType
    {
        _2D,
        _3D,
        _2DTo3D_Y,
        _3DTo2D_Y,
        _2DTo3D_N,
        _3DTo2D_N
    }


    public struct _3DExportParam
    {
        public int _exportNum;
        public _3DExportType _exportType;
    }

    public enum _3DFaceImageType
    {
        Basic,
        Char,
        Line,
        Shape,
        Pattern,
        Mosaic,
        Symmetric,
    }
}