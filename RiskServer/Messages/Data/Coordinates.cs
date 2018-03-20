namespace Risk.Networking.Messages.Data
{
  /// <summary>
  /// 2D coordinates
  /// </summary>
  public sealed class Coordinates
  {
    public int X { get; private set; }

    public int Y { get; private set; }

    public Coordinates(int x, int y)
    {
      X = x;
      Y = y;
    }
  }
}