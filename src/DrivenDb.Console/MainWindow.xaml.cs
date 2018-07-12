using DrivenDbConsole;
using System.Windows;

namespace DrivenDb.Console2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
        }
    }
}
