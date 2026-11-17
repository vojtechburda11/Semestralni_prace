using DataEntity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace SP_RezervaceZasedacichMistnosti.Views;
public partial class CiselnikView : Window {
    private readonly RezervaceContext db = new();
    private readonly ObservableCollection<BaseModel> rows = [];
    private readonly bool rooms;
    public CiselnikView(bool rooms) {
        InitializeComponent(); this.rooms = rooms;
        Title = Heading.Text = rooms ? "Zasedací místnosti" : "Zaměstnanci";
        Grid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("Id"), IsReadOnly = true });
        foreach(var (name,label) in rooms ? new[] { ("Nazev","Název"),("Kapacita","Kapacita") } : new[] { ("Jmeno","Jméno"),("Prijmeni","Příjmení"),("Email","E-mail") })
            Grid.Columns.Add(new DataGridTextColumn { Header = label, Width = new DataGridLength(1, DataGridLengthUnitType.Star), Binding = new Binding(name) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, ValidatesOnDataErrors = true, ValidatesOnExceptions = true } });
        foreach(var x in rooms ? db.Mistnosti.OrderBy(x => x.Nazev).Cast<BaseModel>().ToList() : db.Zamestnanci.OrderBy(x => x.Prijmeni).Cast<BaseModel>().ToList()) rows.Add(x);
        Grid.ItemsSource = rows;
        Closing += PotvrditZavreni; Closed += (_,_) => db.Dispose();
    }
    private void Pridat(object sender, RoutedEventArgs e) {
        BaseModel x = rooms ? new Mistnosti { Kapacita = 1 } : new Zamestnanci();
        db.Add(x); rows.Add(x); Grid.SelectedItem = x; Grid.ScrollIntoView(x);
    }
    private void Smazat(object sender, RoutedEventArgs e) {
        if(Grid.SelectedItem is not BaseModel x) return;
        try {
            int id = rooms ? ((Mistnosti)x).Id : ((Zamestnanci)x).Id;
            if(id != 0 && db.Rezervace.Any(r => rooms ? r.MistnostId == id : r.ZamestnanecId == id)) {
                MessageBox.Show("Existuje rezervace tohoto záznamu (případně v historii). Smazání není dovoleno."); return;
            }
            if(MessageBox.Show("Smazat vybraný záznam? Změnu potvrďte tlačítkem Uložit.","Smazání",MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            db.Remove(x); rows.Remove(x);
        } catch(Exception ex) { Globals.Chyba(ex); }
    }
    private bool Save() {
        if(!Grid.CommitEdit(DataGridEditingUnit.Cell,true) || !Grid.CommitEdit(DataGridEditingUnit.Row,true)) return false;
        try { db.SaveChanges(); Grid.Items.Refresh(); return true; }
        catch(Exception ex) { Globals.Chyba(ex); return false; }
    }
    private void Ulozit(object sender, RoutedEventArgs e) { if(Save()) MessageBox.Show("Změny byly uloženy."); }
    private void Zavrit(object sender, RoutedEventArgs e) => Close();
    private void PotvrditZavreni(object? sender, CancelEventArgs e) {
        Grid.CommitEdit(DataGridEditingUnit.Cell,true); Grid.CommitEdit(DataGridEditingUnit.Row,true);
        if(!db.ChangeTracker.HasChanges()) return;
        var answer = MessageBox.Show("Uložit změny před zavřením?", "Neuložené změny", MessageBoxButton.YesNoCancel);
        e.Cancel = answer == MessageBoxResult.Cancel || (answer == MessageBoxResult.Yes && !Save());
    }
}
