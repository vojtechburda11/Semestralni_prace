using System.Windows;
using SP_RezervaceZasedacichMistnosti.ViewModels;
namespace SP_RezervaceZasedacichMistnosti.Views;
public partial class RezervaceEditView : Window {
    public RezervaceEditView(int? id) {
        InitializeComponent();
        var vm = new RezervaceEditVM(id, () => { DialogResult = true; });
        DataContext = vm; Closed += (_,_) => vm.Dispose();
    }
}
