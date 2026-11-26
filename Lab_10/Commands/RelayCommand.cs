using System;
using System.Windows.Input;
namespace Lab_10.Commands{
    public class RelayCommand : ICommand {
        private readonly Action action;
        private readonly Func<bool>? condition;
        public RelayCommand(Action action, Func<bool>? condition = null){
            this.action = action;
            this.condition = condition;
        }
        public bool CanExecute(object? parameter) {
            return condition?.Invoke() ?? true;
        }
        public void Execute(object? parameter) {
            action.Invoke();
        }
        public event EventHandler? CanExecuteChanged {
            add {
                CommandManager.RequerySuggested += value;
            }
            remove {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}