using DataEntity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SP_RezervaceZasedacichMistnosti.Helpers;
using SP_RezervaceZasedacichMistnosti.Views;
namespace SP_RezervaceZasedacichMistnosti.ViewModels;
public class MainWindowVM : BaseVM {
    public ObservableCollection<Rezervace> Polozky { get; } = [];
    public ObservableCollection<Mistnosti> Mistnosti { get; } = [];
    public ObservableCollection<Zamestnanci> Zamestnanci { get; } = [];
    public Mistnosti? MistnostFiltr { get; set; }
    public Zamestnanci? ZamestnanecFiltr { get; set; }
    public string[] Obdobi { get; } = ["Všechny", "Budoucí", "Probíhající", "Historie"];
    public string VybraneObdobi { get; set; } = "Všechny";
    private Rezervace? selected;
    public Rezervace? Selected { get => selected; set => Set(ref selected,value); }
    public ICommand ObnovitCommand { get; }
    public ICommand PridatCommand { get; }
    public ICommand ZmenitCommand { get; }
    public ICommand SmazatCommand { get; }
    public ICommand MistnostiCommand { get; }
    public ICommand ZamestnanciCommand { get; }
    public MainWindowVM() {
        ObnovitCommand = new RelayCommand(_ => Obnovit());
        PridatCommand = new RelayCommand(_ => Edit(null));
        ZmenitCommand = new RelayCommand(_ => Edit(Selected!.Id), _ => Selected?.CasOd > DateTime.UtcNow);
        SmazatCommand = new RelayCommand(_ => Smazat(), _ => Selected?.CasOd > DateTime.UtcNow);
        MistnostiCommand = new RelayCommand(_ => Catalog(true));
        ZamestnanciCommand = new RelayCommand(_ => Catalog(false));
        Obnovit();
    }
    public void Obnovit() { try {
        using var db = new RezervaceContext();
        int? m = MistnostFiltr?.Id, z = ZamestnanecFiltr?.Id;
        Mistnosti.Clear(); Mistnosti.Add(new() { Id = 0, Nazev = "Všechny místnosti" });
        foreach(var x in db.Mistnosti.AsNoTracking().OrderBy(x => x.Nazev)) Mistnosti.Add(x);
        Zamestnanci.Clear(); Zamestnanci.Add(new() { Id = 0, Jmeno = "Všichni zaměstnanci" });
        foreach(var x in db.Zamestnanci.AsNoTracking().OrderBy(x => x.Prijmeni)) Zamestnanci.Add(x);
        MistnostFiltr = Mistnosti.FirstOrDefault(x => x.Id == m) ?? Mistnosti[0]; Notify(nameof(MistnostFiltr));
        ZamestnanecFiltr = Zamestnanci.FirstOrDefault(x => x.Id == z) ?? Zamestnanci[0]; Notify(nameof(ZamestnanecFiltr));
        var q = db.Rezervace.AsNoTracking().Include(x => x.Mistnost).Include(x => x.Zamestnanec).AsQueryable();
        if (m > 0) q = q.Where(x => x.MistnostId == m);
        if (z > 0) q = q.Where(x => x.ZamestnanecId == z);
        var now = DateTime.UtcNow;
        if (VybraneObdobi == "Budoucí") q = q.Where(x => x.CasOd > now);
        if (VybraneObdobi == "Probíhající") q = q.Where(x => x.CasOd <= now && x.CasDo > now);
        if (VybraneObdobi == "Historie") q = q.Where(x => x.CasDo <= now);
        Polozky.Clear(); foreach(var x in q.OrderByDescending(x => x.CasOd)) Polozky.Add(x);
        Selected = null;
    } catch(Exception ex) { Globals.Chyba(ex); } }
    private void Edit(int? id) {
        try { new RezervaceEditView(id) { Owner = Application.Current.MainWindow }.ShowDialog(); Obnovit(); }
        catch(Exception ex) { Globals.Chyba(ex); }
    }
    private void Catalog(bool rooms) {
        try { new CiselnikView(rooms) { Owner = Application.Current.MainWindow }.ShowDialog(); Obnovit(); }
        catch(Exception ex) { Globals.Chyba(ex); }
    }
    private void Smazat() {
        if (Selected == null || MessageBox.Show("Smazat vybranou budoucí rezervaci?", "Smazání", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try {
            using var db = new RezervaceContext();
            db.Attach(Selected); db.Remove(Selected); db.SaveChanges(); Obnovit();
        } catch(Exception ex) { Globals.Chyba(ex); }
    }
}
