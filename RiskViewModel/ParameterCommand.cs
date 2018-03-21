using System;
using System.Windows.Input;

namespace Risk.ViewModel
{
  /// <summary>
  /// Represents command with parametr. Implements ICommand.
  /// </summary>
  internal class ParameterCommand : ICommand
  {
    private readonly Action<object> _action;

    /// <summary>
    /// Creates parametr command with the action that will be executed.
    /// </summary>
    /// <param name="action">action that will be executed</param>
    public ParameterCommand(Action<object> action)
    {
      _action = action;
    }

    public event EventHandler CanExecuteChanged;

    /// <summary>
    /// If command can be executed.
    /// </summary>
    /// <param name="parameter">parametr is not used</param>
    /// <returns>true</returns>
    public bool CanExecute(object parameter)
    {
      return true;
    }

    /// <summary>
    /// Executed parametr command.
    /// </summary>
    /// <param name="parameter">parametr for action</param>
    public void Execute(object parameter)
    {
      _action(parameter);
    }
  }
}