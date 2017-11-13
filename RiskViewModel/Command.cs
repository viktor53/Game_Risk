using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel
{
  class Command: ICommand
  {
    private readonly Action _aciton;

    public Command(Action action)
    {
      _aciton = action;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      _aciton();
    }
  }
}
