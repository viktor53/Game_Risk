using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.ViewModel.Multiplayer
{
  public interface IMultiplayerMenuViewModel
  {
    bool IsEnabled { get; set; }

    ViewModelBase DialogViewModel { get; set; }

    ObservableCollection<FakeData> TestData { get; }
  }
}
