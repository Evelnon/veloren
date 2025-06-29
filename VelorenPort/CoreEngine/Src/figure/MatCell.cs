using System;

namespace VelorenPort.CoreEngine.figure
{
    /// <summary>
    /// Cell that stores either a reference to a material or explicit voxel data.
    /// </summary>
    [Serializable]
    public struct MatCell
    {
        public enum Kind { None, Mat, Normal }
        public Kind CellKind;
        public Material Material;
        public CellData Data;

        public static MatCell None => new MatCell { CellKind = Kind.None };
        public static MatCell FromMaterial(Material mat) => new MatCell { CellKind = Kind.Mat, Material = mat };
        public static MatCell FromData(CellData data) => new MatCell { CellKind = Kind.Normal, Data = data };

        public bool IsFilled => CellKind != Kind.None;
        public bool IsMaterial => CellKind == Kind.Mat;
        public bool IsNormal => CellKind == Kind.Normal;

        public void Clear()
        {
            CellKind = Kind.None;
            Material = default;
            Data = default;
        }
    }
}
