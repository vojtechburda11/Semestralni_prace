# Stav ověření

- Sestavení obou projektů (.NET 10, Windows targeting z Linuxu) proběhlo bez chyb a varování.
- 21 testů prošlo: aplikační pravidla, dostupnost, FK, historie a syntaxe SQL Server 2022 včetně triggeru.
- 4 testy skutečného SQL Serveru jsou připravené, ale zde přeskočené. Testovací kontejner se nepodařilo spustit kvůli kapacitě disku.
- Běh WPF oken a LocalDB vyžaduje ověření ve Windows.

Před odevzdáním spusťte aplikaci ve Windows a ověřte přidání číselníků, vytvoření a změnu rezervace, odmítnutí překryvů a filtrování. Nastavte `REZERVACE_TEST_CONNECTION_STRING` podle README a spusťte všechny databázové testy. Testy vytvářejí vlastní dočasné databáze.
