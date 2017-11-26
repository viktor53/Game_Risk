using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.ViewModel
{
  public interface IWindowManager
  {
    ViewModelBase WindowViewModel { get; set; }

    void CloseWindow();
  }
}
