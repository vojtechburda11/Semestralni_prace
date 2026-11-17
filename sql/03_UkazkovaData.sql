-- Volitelné ukázkové číselníky. Spusťte až po migracích / 01_Schema.sql.
-- Historické rezervace se nevkládají: nový záznam musí začínat v budoucnosti.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.Mistnosti)
    INSERT dbo.Mistnosti (Nazev, Kapacita, DatumVytvoreni)
    VALUES (N'Zasedačka A',12,SYSUTCDATETIME()),(N'Zasedačka B',6,SYSUTCDATETIME()),(N'Velká aula',30,SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM dbo.Zamestnanci)
    INSERT dbo.Zamestnanci (Jmeno,Prijmeni,Email,DatumVytvoreni)
    VALUES (N'Jan',N'Novák','novak@example.cz',SYSUTCDATETIME()),(N'Petr',N'Svoboda','svoboda@example.cz',SYSUTCDATETIME()),(N'Eva',N'Dvořáková','dvorakova@example.cz',SYSUTCDATETIME());
COMMIT;
