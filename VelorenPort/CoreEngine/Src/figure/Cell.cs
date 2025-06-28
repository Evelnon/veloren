using System;

namespace VelorenPort.CoreEngine.figure
{
    /// <summary>
    /// Metadata for a single voxel. Only stores colour and simple flags.
    /// </summary>
    [Serializable]
    public struct CellData
    {
        public Rgb8 Color;
        public bool Glowy;
        public bool Shiny;
        public bool Hollow;

        public CellData(Rgb8 color, bool glowy, bool shiny, bool hollow)
        {
            Color = color;
            Glowy = glowy;
            Shiny = shiny;
            Hollow = hollow;
        }
    }

    /// <summary>
    /// Represents a voxel in a figure. When <see cref="Data"/> is null the cell
    /// is considered empty.
    /// </summary>
    [Serializable]
    public struct Cell
    {
        public CellData? Data;

        public bool IsEmpty => Data == null;
        public bool IsGlowy => Data?.Glowy == true;
        public bool IsShiny => Data?.Shiny == true;
        public bool IsHollow => Data?.Hollow == true;

        public Cell(Rgb8 color, bool glowy = false, bool shiny = false, bool hollow = false)
        {
            Data = new CellData(color, glowy, shiny, hollow);
        }

        public static Cell Empty => new Cell { Data = null };
    }
}
