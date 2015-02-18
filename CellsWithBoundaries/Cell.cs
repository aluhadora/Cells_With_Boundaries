using System.Drawing;

namespace CellsWithBoundaries
{
  public class Cell
  {
    public const int Width = 10;
    public int X;
    public int Y;
    public bool Top = true;
    public bool Right = true;
    public bool Bottom = true;
    public bool Left = true;
    public bool Visited;

    public Color BackColor;

    public int RealX
    {
      get { return X*Width; }
    }

    public int RealY
    {
      get { return Y * Width; }
    }
  }
}
