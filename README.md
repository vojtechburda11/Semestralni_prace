# Rezervace zasedacích místností

Seminární práce – Burda Vojtěch, Jodl David. Desktopová aplikace C# / WPF a SQL Server, navazující strukturou na [výukový Sklad](https://github.com/ZCU-KPV/Sklad-Hotovy-Horejsi).

## Spuštění ve Windows

1. Nainstalujte .NET 10 SDK a SQL Server Express LocalDB (součást instalace Visual Studia s databázovými nástroji), případně dostupný SQL Server.
2. Otevřete `SP_RezervaceZasedacichMistnosti.slnx` ve Visual Studiu s podporou .NET 10 a nastavte WPF projekt jako spouštěcí. Nebo použijte:

```powershell
dotnet run --project SP_RezervaceZasedacichMistnosti
```

Výchozí spojení: `(localdb)\MSSQLLocalDB`, databáze `RezervaceZasedacichMistnosti`, integrované přihlášení. Při startu se automaticky aplikují migrace; účet proto potřebuje vytvoření databáze/schématu. Pro jiný server nastavte v prostředí `REZERVACE_CONNECTION_STRING`, například:

```powershell
$env:REZERVACE_CONNECTION_STRING = 'Server=localhost;Database=RezervaceZasedacichMistnosti;Integrated Security=True;TrustServerCertificate=True'
```

Po změně proměnné restartujte Visual Studio, pokud aplikaci spouštíte z něj. Pro důvěryhodný produkční certifikát odstraňte `TrustServerCertificate=True`. Přihlašovací údaje nepatří do zdrojových souborů.

Databáze je zpočátku prázdná. V oknech **Zaměstnanci** a **Místnosti** přidejte řádky a uložte je, případně v SSMS spusťte volitelný `sql/03_UkazkovaData.sql`. Pak přidejte rezervaci, zadejte termín a minimální kapacitu, vyhledejte dostupné místnosti, vyberte zaměstnance a vyplňte účel. Hlavní okno filtruje záznamy po stisku **Obnovit / Vyhledat**. Budoucí záznamy lze změnit nebo smazat; probíhající a minulé rezervace jsou pouze ke čtení. Zavření rezervačního formuláře bez uložení změny zahodí.

## Databázové skripty

- `sql/01_Schema.sql`: EF generovaný idempotentní skript včetně historie migrací a triggeru. Alternativa k automatické migraci; spusťte v cílové databázi v SSMS / sqlcmd. Nedělá DROP databáze.
- `sql/02_Dotazy.sql`: oba dotazy ze zadání. Intervaly a auditní údaje jsou UTC. Druhý dotaz počítá datum **vytvoření**, nikoli datum konání.
- `sql/03_UkazkovaData.sql`: volitelná počáteční data číselníků.
- [Návrh, ER diagram a vazba na požadavky](docs/Navrh.md).

## Ověření

```powershell
dotnet build SP_RezervaceZasedacichMistnosti.slnx
dotnet test Tests/Tests.csproj
```

Testy nad SQLite ověřují aplikační pravidla, FK a dostupnost. Testy syntaxe přes Microsoft ScriptDom ověřují všechny SQL skripty včetně dynamického těla triggeru. Volitelné SQL Server testy ověřují skutečné migrace, trigger a souběžné přímé SQL zápisy. Pro jejich spuštění nastavte `REZERVACE_TEST_CONNECTION_STRING` na testovací SQL Server; účet musí smět vytvářet a odstraňovat testovací databáze. Každý test vytváří vlastní databázi s náhodným názvem. Bez proměnné jsou tyto testy přeskočené.

WPF se spouští pouze ve Windows. Na Linuxu je možné projekt sestavit díky `EnableWindowsTargeting`, ale nelze tam ověřit běh GUI ani LocalDB. Testování nad SQLite nenahrazuje ověření souběhu a triggerů na SQL Serveru.

