using DataEntity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;
public sealed class SqlServerFactAttribute : FactAttribute {
    public SqlServerFactAttribute() {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REZERVACE_TEST_CONNECTION_STRING")))
            Skip = "Vyžaduje testovací SQL Server a REZERVACE_TEST_CONNECTION_STRING.";
    }
}
public sealed class SqlServerTests {
    private static async Task WithDatabase(Func<string,Task> test, bool useScript = false) {
        var cs = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("REZERVACE_TEST_CONNECTION_STRING")) { InitialCatalog = "RezervaceTest_" + Guid.NewGuid().ToString("N") };
        await using var db = new RezervaceContext(new DbContextOptionsBuilder<RezervaceContext>().UseSqlServer(cs.ConnectionString).Options);
        try {
            if (!useScript) await db.Database.MigrateAsync();
            else {
                var masterCs = new SqlConnectionStringBuilder(cs.ConnectionString) { InitialCatalog = "master" };
                await using var master = new SqlConnection(masterCs.ConnectionString); await master.OpenAsync();
                await using var create = master.CreateCommand(); create.CommandText = $"CREATE DATABASE [{cs.InitialCatalog}]";
                await create.ExecuteNonQueryAsync();
                for (int i=0;i<2;i++) await ExecuteScript(cs.ConnectionString,"01_Schema.sql");
            }
            db.Mistnosti.AddRange(new Mistnosti { Nazev="A",Kapacita=12 },new Mistnosti { Nazev="B",Kapacita=6 });
            db.Zamestnanci.AddRange(new Zamestnanci { Jmeno="Jan",Prijmeni="Novák",Email="jan@example.cz" },new Zamestnanci { Jmeno="Eva",Prijmeni="Malá",Email="eva@example.cz" });
            await db.SaveChangesAsync();
            await test(cs.ConnectionString);
        } finally { await db.Database.EnsureDeletedAsync(); }
    }

    private static async Task ExecuteScript(string cs, string name) {
        var text = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"sql",name));
        await using var c=new SqlConnection(cs);await c.OpenAsync();
        foreach(var batch in System.Text.RegularExpressions.Regex.Split(text,@"(?im)^GO\s*$")) {
            if(string.IsNullOrWhiteSpace(batch))continue;
            await using var cmd=c.CreateCommand();cmd.CommandText=batch;await cmd.ExecuteNonQueryAsync();
        }
    }
    [SqlServerFact] public Task InstalaceSkriptemJeIdempotentniADotazyJsouSpustitelne() => WithDatabase(async cs => {
        await ExecuteScript(cs,"02_Dotazy.sql");await ExecuteScript(cs,"03_UkazkovaData.sql");
        await using var db=new RezervaceContext(new DbContextOptionsBuilder<RezervaceContext>().UseSqlServer(cs).Options);
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(2,await db.Mistnosti.CountAsync());
        var start=DateTime.UtcNow.AddDays(1);
        await Insert(cs,1,1,start,start.AddHours(1));
        await Assert.ThrowsAsync<SqlException>(()=>Insert(cs,1,2,start,start.AddHours(1)));
    },useScript:true);
    private static async Task Insert(string cs, int room, int employee, DateTime start, DateTime end, string purpose="Porada") {
        await using var c = new SqlConnection(cs); await c.OpenAsync();
        await using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT dbo.Rezervace(MistnostId,ZamestnanecId,CasOd,CasDo,Ucel,DatumVytvoreni) VALUES(@m,@z,@od,@do,@u,SYSUTCDATETIME());";
        cmd.Parameters.AddWithValue("@m",room);cmd.Parameters.AddWithValue("@z",employee);
        cmd.Parameters.AddWithValue("@od",start);cmd.Parameters.AddWithValue("@do",end);cmd.Parameters.AddWithValue("@u",purpose);
        await cmd.ExecuteNonQueryAsync();
    }
    [SqlServerFact] public Task PrimeSqlZapisyRespektujiOmezeni() => WithDatabase(async cs => {
        var start=DateTime.UtcNow.AddDays(2);
        await Insert(cs,1,1,start,start.AddHours(1));
        await Assert.ThrowsAsync<SqlException>(()=>Insert(cs,1,2,start,start.AddHours(1)));
        await Assert.ThrowsAsync<SqlException>(()=>Insert(cs,2,1,start,start.AddHours(1)));
        await Assert.ThrowsAsync<SqlException>(()=>Insert(cs,2,2,start,start.AddHours(9)));
        await Assert.ThrowsAsync<SqlException>(()=>Insert(cs,2,2,start,start.AddHours(1)," "));
        await Assert.ThrowsAsync<SqlException>(()=>Insert(cs,2,2,start.AddDays(-3),start.AddDays(-3).AddHours(1)));
        await Insert(cs,1,1,start.AddHours(1),start.AddHours(2));
        await using var db=new RezervaceContext(new DbContextOptionsBuilder<RezervaceContext>().UseSqlServer(cs).Options);
        Assert.Equal(2,await db.Rezervace.CountAsync());
        await Assert.ThrowsAsync<SqlException>(()=>db.Database.ExecuteSqlRawAsync("DELETE dbo.Mistnosti WHERE Id=1"));
    });
    [SqlServerFact] public Task SoubezneKonfliktniZapisyNemohouObaUspet() => WithDatabase(async cs => {
        var start=DateTime.UtcNow.AddDays(2);
        var ready=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<bool> Attempt(int employee) {
            await ready.Task;
            try { await Insert(cs,1,employee,start,start.AddHours(1)); return true; }
            catch(SqlException) { return false; }
        }
        var a=Attempt(1);var b=Attempt(2);ready.SetResult();
        var results=await Task.WhenAll(a,b);Assert.Single(results,x=>x);
        await using var db=new RezervaceContext(new DbContextOptionsBuilder<RezervaceContext>().UseSqlServer(cs).Options);
        Assert.Equal(1,await db.Rezervace.CountAsync());
    });
    [SqlServerFact] public Task HistoriiChraniDatabazeProtiUpdateIDelete() => WithDatabase(async cs => {
        // Urychlení času pouze v izolované testovací DB: načteme historický záznam
        // s dočasně vypnutým triggerem a před kontrolou ho opět povolíme.
        await using var db=new RezervaceContext(new DbContextOptionsBuilder<RezervaceContext>().UseSqlServer(cs).Options);
        await db.Database.ExecuteSqlRawAsync("DISABLE TRIGGER dbo.TR_Rezervace_Pravidla ON dbo.Rezervace;");
        var start=DateTime.UtcNow.AddDays(-1);
        try { await Insert(cs,1,1,start,start.AddHours(1)); }
        finally { await db.Database.ExecuteSqlRawAsync("ENABLE TRIGGER dbo.TR_Rezervace_Pravidla ON dbo.Rezervace;"); }
        await Assert.ThrowsAsync<SqlException>(()=>db.Database.ExecuteSqlRawAsync("UPDATE dbo.Rezervace SET Ucel=N'Změna'"));
        await Assert.ThrowsAsync<SqlException>(()=>db.Database.ExecuteSqlRawAsync("DELETE dbo.Rezervace"));
        Assert.Equal(1,await db.Rezervace.CountAsync());
    });
}
