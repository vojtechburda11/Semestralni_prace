-- SQL Server; veškeré uložené časy i parametry jsou UTC.
-- 1. Dostupné místnosti pro nejméně 8 osob; interval [od, do).
DECLARE @Od datetime2 = DATEADD(day, 1, SYSUTCDATETIME());
DECLARE @Do datetime2 = DATEADD(hour, 2, @Od);
SELECT m.Id, m.Nazev, m.Kapacita
FROM dbo.Mistnosti AS m
WHERE m.Kapacita >= 8
  AND NOT EXISTS (
    SELECT 1 FROM dbo.Rezervace AS r
    WHERE r.MistnostId = m.Id AND r.CasOd < @Do AND @Od < r.CasDo
  )
ORDER BY m.Kapacita DESC, m.Nazev;
GO
-- 2. Více než 3 rezervace VYTVOŘENÉ za posledních 30 dní.
DECLARE @Nyni datetime2 = SYSUTCDATETIME();
SELECT z.Id, z.Jmeno, z.Prijmeni, z.Email, COUNT(*) AS PocetRezervaci
FROM dbo.Zamestnanci AS z
JOIN dbo.Rezervace AS r ON r.ZamestnanecId = z.Id
WHERE r.DatumVytvoreni >= DATEADD(day, -30, @Nyni) AND r.DatumVytvoreni <= @Nyni
GROUP BY z.Id, z.Jmeno, z.Prijmeni, z.Email
HAVING COUNT(*) > 3
ORDER BY PocetRezervaci DESC, z.Prijmeni, z.Jmeno;
