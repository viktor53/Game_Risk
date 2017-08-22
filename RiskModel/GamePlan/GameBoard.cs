namespace Risk.Model.GamePlan
{
  class GameBoard
  {
    public bool[][] Board { get; private set; }

    public Area[] Areas { get; private set; }

    public GameBoard(int countOfAreas)
    {
      Board = new bool[countOfAreas][];
      for (int i = 0; i < countOfAreas; i++)
      {
        Board[i] = new bool[countOfAreas];
      }

      Areas = new Area[countOfAreas];
    }
  }
}
