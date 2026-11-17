using DataEntity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Input;
using SP_RezervaceZasedacichMistnosti.Helpers;
namespace SP_RezervaceZasedacichMistnosti.ViewModels;
public sealed class RezervaceEditVM : BaseVM, IDisposable {
    private readonly RezervaceContext db = new();
    private readonly Rezervace entity;
    private readonly Action close;
    private string od = "", doCas = "", kapacita = "8";
    public string Od { get => od; set { if(Set(ref od,value)) Invalidovat(); } }
    public string Do { get => doCas; set { if(Set(ref doCas,value)) Invalidovat(); } }
    public string Kapacita { get => kapacita; set { if(Set(ref kapacita,value)) Invalidovat(); } }
    public string Ucel { get; set; }
    public ObservableCollection<Mistnosti> Dostupne { get; } = [];
    public List<Zamestnanci> Zamestnanci { get; }
    private Mistnosti? mistnost;
    public Mistnosti? Mistnost { get => mistnost; set => Set(ref mistnost,value); }
    public Zamestnanci? Zamestnanec { get; set; }
    public string Napoveda => "Datum a čas: dd.MM.yyyy HH:mm (místní čas). Po změně termínu znovu vyhledejte místnosti.";
    public ICommand HledatCommand { get; }
    public ICommand UlozitCommand { get; }
    public RezervaceEditVM(int? id, Action close) {
        this.close = close;
        entity = id.HasValue ? db.Rezervace.Single(x => x.Id == id) : new();
        if (id.HasValue && entity.CasOd <= DateTime.UtcNow) throw new ValidationException("Historii a probíhající rezervace nelze měnit.");
        var start = id.HasValue ? DateTime.SpecifyKind(entity.CasOd, DateTimeKind.Utc).ToLocalTime() : DateTime.Today.AddDays(1).AddHours(9);
        Od = start.ToString("dd.MM.yyyy HH:mm");
        Do = (id.HasValue ? DateTime.SpecifyKind(entity.CasDo, DateTimeKind.Utc).ToLocalTime() : start.AddHours(1)).ToString("dd.MM.yyyy HH:mm");
        if (id.HasValue) Kapacita = "1";
        Ucel = entity.Ucel;
        Zamestnanci = db.Zamestnanci.AsNoTracking().OrderBy(x => x.Prijmeni).ToList();
        Zamestnanec = Zamestnanci.FirstOrDefault(x => x.Id == entity.ZamestnanecId);
        HledatCommand = new RelayCommand(_ => Hledat());
        UlozitCommand = new RelayCommand(_ => Ulozit());
        Hledat(); Mistnost = Dostupne.FirstOrDefault(x => x.Id == entity.MistnostId);
    }
    private void Invalidovat() { Dostupne.Clear(); Mistnost = null; }
    private (DateTime od, DateTime konec, int kapacita) Interval() {
        DateTime Parse(string text) {
            if (!DateTime.TryParseExact(text, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                || TimeZoneInfo.Local.IsInvalidTime(d) || TimeZoneInfo.Local.IsAmbiguousTime(d))
                throw new ValidationException("Zadejte jednoznačný místní čas ve formátu dd.MM.yyyy HH:mm.");
            return DateTime.SpecifyKind(d, DateTimeKind.Local).ToUniversalTime();
        }
        var start = Parse(Od); var end = Parse(Do);
        if(start <= DateTime.UtcNow || end <= start || end-start > TimeSpan.FromHours(8))
            throw new ValidationException("Zahájení musí být v budoucnosti, konec později; délka nejvýše 8 hodin.");
        if(!int.TryParse(Kapacita, out var k) || k < 1) throw new ValidationException("Minimální kapacita musí být kladné celé číslo.");
        return (start,end,k);
    }
    private void Hledat() { try {
        var (start,end,k) = Interval();
        Invalidovat(); foreach(var x in db.DostupneMistnosti(start,end,k,entity.Id).ToList()) Dostupne.Add(x);
    } catch(Exception ex) { Globals.Chyba(ex); } }
    private void Ulozit() { try {
        var (start,end,k) = Interval();
        if(Mistnost == null || Zamestnanec == null) throw new ValidationException("Vyberte dostupnou místnost a zaměstnance.");
        if(!db.DostupneMistnosti(start,end,k,entity.Id).Any(x => x.Id == Mistnost.Id)) throw new ValidationException("Místnost již není dostupná. Vyhledejte znovu.");
        entity.CasOd = start; entity.CasDo = end; entity.MistnostId = Mistnost.Id; entity.ZamestnanecId = Zamestnanec.Id; entity.Ucel = Ucel.Trim();
        if(entity.Id == 0 && db.Entry(entity).State == EntityState.Detached) db.Add(entity);
        db.SaveChanges(); close();
    } catch(Exception ex) { Globals.Chyba(ex); } }
    public void Dispose() => db.Dispose();
}
