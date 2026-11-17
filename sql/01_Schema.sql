IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117123120_PocatecniSchema'
)
BEGIN
    CREATE TABLE [Mistnosti] (
        [Id] int NOT NULL IDENTITY,
        [Nazev] nvarchar(100) NOT NULL,
        [Kapacita] int NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [DatumVytvoreni] datetime2 NOT NULL,
        CONSTRAINT [PK_Mistnosti] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Mistnosti_Kapacita] CHECK ([Kapacita] > 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117123120_PocatecniSchema'
)
BEGIN
    CREATE TABLE [Zamestnanci] (
        [Id] int NOT NULL IDENTITY,
        [Jmeno] nvarchar(100) NOT NULL,
        [Prijmeni] nvarchar(100) NOT NULL,
        [Email] nvarchar(254) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [DatumVytvoreni] datetime2 NOT NULL,
        CONSTRAINT [PK_Zamestnanci] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117123120_PocatecniSchema'
)
BEGIN
    CREATE TABLE [Rezervace] (
        [Id] int NOT NULL IDENTITY,
        [MistnostId] int NOT NULL,
        [ZamestnanecId] int NOT NULL,
        [CasOd] datetime2 NOT NULL,
        [CasDo] datetime2 NOT NULL,
        [Ucel] nvarchar(500) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [DatumVytvoreni] datetime2 NOT NULL,
        CONSTRAINT [PK_Rezervace] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Rezervace_Interval] CHECK ([CasDo] > [CasOd] AND [CasDo] <= DATEADD(hour, 8, [CasOd])),
        CONSTRAINT [CK_Rezervace_Ucel] CHECK (LEN(LTRIM(RTRIM([Ucel]))) > 0),
        CONSTRAINT [FK_Rezervace_Mistnosti_MistnostId] FOREIGN KEY ([MistnostId]) REFERENCES [Mistnosti] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Rezervace_Zamestnanci_ZamestnanecId] FOREIGN KEY ([ZamestnanecId]) REFERENCES [Zamestnanci] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117123120_PocatecniSchema'
)
BEGIN
    CREATE INDEX [IX_Rezervace_DatumVytvoreni] ON [Rezervace] ([DatumVytvoreni]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117123120_PocatecniSchema'
)
BEGIN
    CREATE INDEX [IX_Rezervace_MistnostId_CasOd_CasDo] ON [Rezervace] ([MistnostId], [CasOd], [CasDo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117123120_PocatecniSchema'
)
BEGIN
    CREATE INDEX [IX_Rezervace_ZamestnanecId_CasOd_CasDo] ON [Rezervace] ([ZamestnanecId], [CasOd], [CasDo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117123120_PocatecniSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117123120_PocatecniSchema', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117130000_IntegritniPravidla'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER TR_Rezervace_Pravidla ON dbo.Rezervace
    AFTER INSERT, UPDATE, DELETE AS
    BEGIN
        SET NOCOUNT ON;
        -- Transakční zámek serializuje konkurenční kontroly. Případný deadlock
        -- jednu transakci zruší; konfliktní rezervace se nikdy neuloží obě.
        DECLARE @lockResult int;
        EXEC @lockResult = sys.sp_getapplock @Resource=N''Rezervace_Integrita'',
            @LockMode=''Exclusive'', @LockOwner=''Transaction'', @LockTimeout=10000;
        IF @lockResult < 0 THROW 51000, N''Kontrola rezervace je zaneprázdněna. Opakujte operaci.'', 1;
        DECLARE @now datetime2 = SYSUTCDATETIME();
        IF EXISTS (SELECT 1 FROM deleted WHERE CasOd <= @now)
            THROW 51001, N''Probíhající a uskutečněné rezervace nelze měnit ani mazat.'', 1;
        IF EXISTS (SELECT 1 FROM inserted WHERE CasOd <= @now)
            THROW 51002, N''Začátek rezervace musí být v budoucnosti (UTC).'', 1;
        IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON d.Id=i.Id WHERE i.DatumVytvoreni<>d.DatumVytvoreni)
            THROW 51003, N''Datum vytvoření rezervace nelze změnit.'', 1;
        IF EXISTS (
            SELECT 1 FROM inserted i JOIN dbo.Rezervace r WITH (UPDLOCK, HOLDLOCK)
            ON r.Id<>i.Id AND r.CasOd<i.CasDo AND i.CasOd<r.CasDo
            AND (r.MistnostId=i.MistnostId OR r.ZamestnanecId=i.ZamestnanecId))
            THROW 51004, N''Překryv rezervací místnosti nebo zaměstnance.'', 1;
    END');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117130000_IntegritniPravidla'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117130000_IntegritniPravidla', N'10.0.10');
END;

COMMIT;
GO

