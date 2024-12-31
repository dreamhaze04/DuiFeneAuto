using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DuiFeneAuto.Commands {
    public class AsyncRelayCommand : ICommand {
        private readonly Func<object, Task> _execute; // 支持带参数的异步方法
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged;

        public AsyncRelayCommand(Func<object, Task> execute, Predicate<object> canExecute = null) {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) {
            return _canExecute == null || _canExecute(parameter);
        }

        public async void Execute(object parameter) {
            await _execute(parameter);
        }

        public void RaiseCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
