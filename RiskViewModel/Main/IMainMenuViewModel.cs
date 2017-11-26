using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.ViewModel.Main
{
  public interface IMainMenuViewModel
  {
    ViewModelBase ContentViewModel { get; set; }

    bool IsEnabled { get; set; }
  }
}
