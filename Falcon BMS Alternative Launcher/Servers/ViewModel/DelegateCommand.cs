using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FalconBMS.Launcher.Servers.ViewModel
{

    public class DelegateCommand : ICommand
    {

        Predicate<object> canExecute;
        Action<object> execute;

        public DelegateCommand(Predicate<object> _canexecute, Action<object> _execute)
            : this()
        {
            canExecute = _canexecute;
            execute = _execute;
        }

        public DelegateCommand(Action<object> _execute) : this()
        {
            execute = _execute;
        }

        public DelegateCommand() { }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}