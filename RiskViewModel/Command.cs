using System;
using System.Windows.Input;

namespace Risk.ViewModel
{
  /// <summary>
  /// Represents command. Implements ICommand.
  /// </summary>
  internal class Command : ICommand
  {
    private readonly Action _aciton;

    /// <summary>
    /// Creates command with the action that will be executed.
    /// </summary>
    /// <param name="action">action that will be executed</param>
    public Command(Action action)
    {
      _aciton = action;
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
    /// Executed command.
    /// </summary>
    /// <param name="parameter">parametr is not used</param>
    public void Execute(object parameter)
    {
      _aciton();
    }
  }
}