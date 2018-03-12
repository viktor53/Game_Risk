using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Risk.ViewModel
{
  public interface IWindowManager
  {
    ViewModelBase WindowViewModel { get; set; }

    SynchronizationContext UI { get; }

    void CloseWindow();
  }
}