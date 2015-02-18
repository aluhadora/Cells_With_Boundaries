
namespace CellsWithBoundaries
{
  public static class UtilityClass
  {
    public static bool InRange(this int x, int a, int b)
    {
      if (x < a) return false;
      if (x > b) return false;
      return true;
    }
  }
}
