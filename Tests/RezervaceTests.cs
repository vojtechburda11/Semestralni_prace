using DataEntity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Xunit;
public sealed class RezervaceTests : IDisposable {
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly RezervaceContext db;
    public RezervaceTests() {
        connection.Open();
        db = new(new DbContextOptionsBuilder<RezervaceContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        db.Mistnosti.AddRange(new Mistnosti {Nazev="A",Kapacita=12},new Mistnosti {Nazev="B",Kapacita=6});
        db.Zamestnanci.AddRange(new Zamestnanci {Jmeno="Jan",Prijmeni="Novák",Email="jan@example.cz"},new Zamestnanci {Jmeno="Eva",Prijmeni="Malá",Email="eva@example.cz"});
        db.SaveChanges();
    }
    private Rezervace New(int m=1,int z=1,int start=9,int end=10) => new() {MistnostId=m,ZamestnanecId=z,Ucel="Porada",CasOd=DateTime.UtcNow.Date.AddDays(2).AddHours(start),CasDo=DateTime.UtcNow.Date.AddDays(2).AddHours(end)};
    [Fact] public void NavazujiciTerminyJsouPovolene() {
        db.Add(New());db.SaveChanges();db.Add(New(start:10,end:18));db.SaveChanges();Assert.Equal(2,db.Rezervace.Count());
    }
    [Theory] [InlineData(1,2)] [InlineData(2,1)]
    public void PrekryvMistnostiNeboZamestnanceJeZakazan(int m,int z) {
        db.Add(New());db.SaveChanges();db.Add(New(m,z));Assert.Throws<ValidationException>(()=>db.SaveChanges());
    }
    [Fact] public void KontrolaPlatiIProDveNoveRezervaceVJedneDavce() {
        db.Add(New());db.Add(New(z:2));Assert.Throws<ValidationException>(()=>db.SaveChanges());
    }
    [Theory] [InlineData(10,9)] [InlineData(9,9)] [InlineData(9,18)]
    public void NeplatnaDelkaJeZakazana(int start,int end) {
        db.Add(New(start:start,end:end));Assert.Throws<ValidationException>(()=>db.SaveChanges());
    }
    [Fact] public async Task MinulyZacatekJeZakazanIAsynchronne() {
        var x=New();x.CasOd=DateTime.UtcNow.AddHours(-1);x.CasDo=DateTime.UtcNow.AddHours(1);db.Add(x);
        await Assert.ThrowsAsync<ValidationException>(async()=>await db.SaveChangesAsync());
    }
    [Theory] [InlineData("")] [InlineData("   ")] [InlineData("\t")]
    public void UcelJePovinny(string text) {var x=New();x.Ucel=text;db.Add(x);Assert.Throws<ValidationException>(()=>db.SaveChanges());}
    [Fact] public void KapacitaMusiBytKladna() {db.Mistnosti.First().Kapacita=0;Assert.Throws<ValidationException>(()=>db.SaveChanges());}
    [Fact] public void OdkazovaneZaznamyNelzeSmazat() {
        db.Add(New());db.SaveChanges();db.ChangeTracker.Clear();
        db.Remove(db.Mistnosti.First());Assert.Throws<DbUpdateException>(()=>db.SaveChanges());
    }
    [Fact] public void EditaceSebeSamaNeniPrekryv() {
        var x=New();db.Add(x);db.SaveChanges();x.Ucel="Jiná porada";db.SaveChanges();Assert.Equal("Jiná porada",db.Rezervace.Single().Ucel);
    }
    [Fact] public void DostupnostRespektujeKapacituAPrekryv() {
        var x=New();db.Add(x);db.SaveChanges();
        Assert.Empty(db.DostupneMistnosti(x.CasOd,x.CasDo,8));
        Assert.Single(db.DostupneMistnosti(x.CasDo,x.CasDo.AddHours(1),8));
        Assert.Single(db.DostupneMistnosti(x.CasOd,x.CasDo,1));
    }
    [Fact] public void DatumVytvoreniNelzePrepsat() {
        var x=New();db.Add(x);db.SaveChanges();x.DatumVytvoreni=x.DatumVytvoreni.AddDays(-1);Assert.Throws<ValidationException>(()=>db.SaveChanges());
    }
    [Theory] [InlineData(true)] [InlineData(false)]
    public void HistoriiNelzeMenitAniSmazat(bool delete) {
        // Testovací import historie obchází aplikační zápis; produkční trigger import nedovolí.
        var past=DateTime.UtcNow.AddDays(-1);
        db.Database.ExecuteSqlInterpolated($"INSERT INTO Rezervace (MistnostId,ZamestnanecId,CasOd,CasDo,Ucel,DatumVytvoreni) VALUES (1,1,{past},{past.AddHours(1)},{"Historie"},{past.AddDays(-1)})");
        var x=db.Rezervace.Single();if(delete)db.Remove(x);else x.Ucel="Změna";
        Assert.Throws<ValidationException>(()=>db.SaveChanges());
    }
    public void Dispose(){db.Dispose();connection.Dispose();}
}
