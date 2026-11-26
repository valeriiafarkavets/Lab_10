using System.Windows;
using Lab_10.ViewModels;
namespace Lab_10{
    public partial class MainWindow : Window {
        public MainWindow(){
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}