using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CellsWithBoundaries
{
  public partial class MainForm : Form
  {
    private readonly Cell[,] _cells;
    private Graphics _g;
    private readonly Pen _p;
    private bool _stop;
    private Cell _cell;
    private readonly Random _random;
    private readonly Stack<Cell> _stack = new Stack<Cell>();
    private readonly int _hCells;
    private readonly int _vCells;

    public MainForm()
    {
      InitializeComponent();

      _hCells = pictureBox.Width / Cell.Width;
      _vCells = pictureBox.Height / Cell.Width;

      _cells = new Cell[_hCells,_vCells];
      for (int j = 0; j < _vCells; j++)
      {
        for (int i = 0; i < _hCells; i++)
        {
          _cells[i,j] = new Cell
            {
              X = i,
              Y = j,
            };
        }
      }

      var bmp = new Bitmap(Width, Height);
      pictureBox.Image = bmp;
      _g = Graphics.FromImage(bmp);
      _p = new Pen(Color.Black, 1);

      Redraw();
      _random = new Random();

      var thread = new Thread(Process);
      thread.Start();
    }

    private void Process()
    {
      Thread.Sleep(500);
      while (!_stop)
      {
        Tick();
      }
    }

    private int _counter;
    private bool _skipDraw;

    private void Tick()
    {
      if (_cell == null)
      {
        _cell = _cells[_random.Next(_hCells), _random.Next(_vCells)];
        _cell.Visited = true;
      }

      if (AllCells().Any(x => !x.Visited))
      {
        var neighbors = Neighbors(_cell);
        if (neighbors.Any(x => !x.Visited))
        {
          var neighbor = neighbors[_random.Next(neighbors.Count)];
          _stack.Push(_cell);
          RemoveWall(_cell, neighbor);
          _cell = neighbor;
          _cell.Visited = true;
        }
        else
        {
          if (_stack.Any())
          _cell = _stack.Pop();
        }
      }
      else
      {
        _cell = null;
        _stop = true;
        _skipDraw = false;
        Redraw();
        return;
      }

      _counter++;
      if (_counter == 1)
      {
        _counter = 0;
        Redraw();
      }
    }

    private static void RemoveWall(Cell cell, Cell neighbor)
    {
      if (cell.X == neighbor.X)
      {
        if (cell.Y < neighbor.Y)
        {
          cell.Bottom = false;
          neighbor.Top = false;
        }
        else
        {
          cell.Top = false;
          neighbor.Bottom = false;
        }
      }
      else
      {
        if (cell.X < neighbor.X)
        {
          cell.Right = false;
          neighbor.Left = false;
        }
        else
        {
          cell.Left = false;
          neighbor.Right = false;
        }
      }
    }

    private IEnumerable<Cell> AllCells()
    {
      var cells = new List<Cell>();
      foreach (var cell in _cells)
      {
        cells.Add(cell);
      }
      return cells;
    }

    private IList<Cell> Neighbors(Cell cell)
    {
      var neighbors = new List<Cell>();

      for (int j = cell.Y - 1; j < cell.Y + 2; j++)
      {
        for (int i = cell.X - 1; i < cell.X + 2; i++)
        {
          if (!i.InRange(0, _hCells - 1) || !j.InRange(0, _vCells - 1)) continue;
          if (i == cell.X && j == cell.Y) continue;
          if (i != cell.X && j != cell.Y) continue;
          if (_cells[i,j].Visited) continue;
          neighbors.Add(_cells[i,j]);
        }
      }

      return neighbors;
    }

    private void Redraw()
    {
      if (_skipDraw) return;

      if (pictureBox.InvokeRequired)
      {
        pictureBox.Invoke(new Action(Redraw));
        return;
      }

      var image = pictureBox.Image;
      _g = Graphics.FromImage(image);
      _g.Clear(Color.White);

      foreach (var cell in _cells)
      {
        if (cell.Bottom)
        {
          DrawBottom(cell);
        }
        if (cell.Left)
        {
          DrawLeft(cell);
        }
        if (cell.Right)
        {
          DrawRight(cell);
        }
        if (cell.Top)
        {
          DrawTop(cell);
        }

        cell.BackColor = cell.Visited ? Color.White : Color.Gray;
        if (cell == _cell) cell.BackColor = Color.Blue;
        DrawCellBody(cell);
      }

      pictureBox.Image = image;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      _stop = true;
      Thread.Sleep(500);
    }

    private void DrawCellBody(Cell cell)
    {
      _g.FillRectangle(new Pen(cell.BackColor, 1).Brush, cell.RealX + 1, cell.RealY + 1, Cell.Width - 2, Cell.Width - 2);
    }

    private void DrawBottom(Cell cell)
    {
      _g.DrawLine(_p, cell.RealX, cell.RealY + Cell.Width, cell.RealX + Cell.Width, cell.RealY + Cell.Width);
    }

    private void DrawLeft(Cell cell)
    {
      _g.DrawLine(_p, cell.RealX, cell.RealY, cell.RealX, cell.RealY + Cell.Width);
    }

    private void DrawRight(Cell cell)
    {
      _g.DrawLine(_p, cell.RealX + Cell.Width, cell.RealY, cell.RealX + Cell.Width, cell.RealY + Cell.Width);
    }

    private void DrawTop(Cell cell)
    {
      _g.DrawLine(_p, cell.RealX, cell.RealY, cell.RealX + Cell.Width, cell.RealY);
    }

    private void PictureBoxClick(object sender, EventArgs e)
    {
      if (!_stop) return;

      var image = pictureBox.Image;
      image.Save("Maze.png", ImageFormat.Png);
    }

    private void MainForm_ResizeBegin(object sender, EventArgs e)
    {
      _skipDraw = true;
    }

    private void MainForm_ResizeEnd(object sender, EventArgs e)
    {
      _skipDraw = false;
    }
  }
}
