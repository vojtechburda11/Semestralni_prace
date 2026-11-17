using System.Windows;
using SP_RezervaceZasedacichMistnosti.ViewModels;
namespace SP_RezervaceZasedacichMistnosti.Views;
public partial class MainWindow : Window {
    public MainWindow() { InitializeComponent(); DataContext = new MainWindowVM(); }
}
