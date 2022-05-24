using System;
using System.Windows.Input;

namespace Jewelry.Commands
{
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _executeMethod;
        private readonly Func<T, bool> _canExecuteMethod;

        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, (T o) => true)
        {
        }
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod");
            }

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }
        public bool CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }
        protected void Execute(T parameter)
        {
            _executeMethod(parameter);
        }
        protected bool CanExecute(T parameter)
        {
            return _canExecuteMethod(parameter);
        }
    }
    public class DelegateCommand : ICommand
    {
        private readonly Action _executeMethod;
        private readonly Func<bool> _canExecuteMethod;

        public DelegateCommand(Action executeMethod)
            : this(executeMethod, () => true)
        {
        }
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod");
            }

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            Execute();
        }
        public bool CanExecute(object parameter)
        {
            return CanExecute();
        }
        protected void Execute()
        {
            _executeMethod();
        }
        protected bool CanExecute()
        {
            return _canExecuteMethod();
        }
    }
}
