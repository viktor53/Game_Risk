using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Risk.ViewModel.Game;
using Risk.Networking.Client;
using Risk.Model.GameCore;
using Risk.Model.Enums;
using System.IO;
using Risk.Networking.Server;
using Risk.AI;
using Risk.AI.MCTS;
using Risk.AI.NeuralNetwork;

namespace Risk.ViewModel.Main
{
  /// <summary>
  /// Represents creating single player.
  /// </summary>
  public class CreateSinglePlayerViewModel : ViewModelBase
  {
    private IWindowManager _windowManager;

    private IMainMenuViewModel _mainMenu;

    private int _players;

    private string _map;

    private string _agent;

    /// <summary>
    /// Click on Create button. Tries create new game room.
    /// </summary>
    public ICommand Create_Click { get; private set; }

    /// <summary>
    /// Click on Cancel button. Leaves creating new game room.
    /// </summary>
    public ICommand Cancel_Click { get; private set; }

    /// <summary>
    /// Maximum capacity of players
    /// </summary>
    public int Players
    {
      get
      {
        return _players;
      }
      set
      {
        _players = value;
        OnPropertyChanged("Players");
      }
    }

    /// <summary>
    /// Type of map. Default/Classic map or random generated map.
    /// </summary>
    public string Map
    {
      get
      {
        return _map;
      }
      set
      {
        _map = value;
        OnPropertyChanged("Map");
      }
    }

    public string Agent
    {
      get
      {
        return _agent;
      }
      set
      {
        _agent = value;
        OnPropertyChanged("Agent");
      }
    }

    /// <summary>
    /// List of map options.
    /// </summary>
    public ObservableCollection<string> Maps { get; private set; }

    /// <summary>
    /// Number of players options list.
    /// </summary>
    public ObservableCollection<int> NumberOfPlayers { get; private set; }

    /// <summary>
    /// List of agents.
    /// </summary>
    public ObservableCollection<string> Agents { get; private set; }

    public CreateSinglePlayerViewModel(IWindowManager windowManager, IMainMenuViewModel mainMenu)
    {
      _windowManager = windowManager;
      _mainMenu = mainMenu;

      Create_Click = new Command(CreateClick);
      Cancel_Click = new Command(CancelClick);

      Maps = CreateMaps();
      NumberOfPlayers = CreateNumberOfPlayers();
      Agents = CreateAgents();
    }

    /// <summary>
    /// Tries create new game room.
    /// </summary>
    private void CreateClick()
    {
      IGame game = RiskOfflineClient.GetGame(Map == "Default", Players, new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\DefaultLogger.xml"));

      ArmyColor color = ArmyColor.Blue;
      switch (Agent)
      {
        case "Random":
          for (int i = 0; i < Players - 1; ++i)
          {
            game.AddPlayer(new RandomPlayer(color + i));
          }
          break;

        case "MCTS":
          for (int i = 0; i < Players - 1; ++i)
          {
            game.AddPlayer(new MCTSAI(color + i));
          }
          break;

        case "MCTS with NN":
          for (int i = 0; i < Players - 1; ++i)
          {
            game.AddPlayer(new NeuroAI(color + i, true, 3));
          }
          break;

        case "NN":
          for (int i = 0; i < Players - 1; ++i)
          {
            game.AddPlayer(new NeuroAI(color + i, true, 3));
          }
          break;

        default:
          for (int i = 0; i < Players - 1; ++i)
          {
            game.AddPlayer(new RandomPlayer(color + i));
          }
          break;
      }

      var offClient = new RiskOfflineClient(game, ArmyColor.Green);

      _windowManager.WindowViewModel = new GameBoardViewModel(_windowManager, offClient, offClient.GetGameBoardInfo());
    }

    /// <summary>
    /// Leaves creating new game room.
    /// </summary>
    private void CancelClick()
    {
      _mainMenu.ContentViewModel = null;
    }

    /// <summary>
    /// Creates list of map options.
    /// </summary>
    /// <returns></returns>
    private ObservableCollection<string> CreateMaps()
    {
      var maps = new ObservableCollection<string>();
      maps.Add("Default");
      maps.Add("Generate");
      return maps;
    }

    /// <summary>
    /// Creates number of players options list.
    /// </summary>
    /// <returns></returns>
    private ObservableCollection<int> CreateNumberOfPlayers()
    {
      var numberOfPlayers = new ObservableCollection<int>();
      for (int i = 3; i <= 6; i++)
      {
        numberOfPlayers.Add(i);
      }
      return numberOfPlayers;
    }

    /// <summary>
    /// Creates agents options list.
    /// </summary>
    /// <returns></returns>
    private ObservableCollection<string> CreateAgents()
    {
      var agents = new ObservableCollection<string>();
      agents.Add("Random");
      agents.Add("MCTS");
      agents.Add("MCTS with NN");
      agents.Add("NN");
      return agents;
    }
  }
}