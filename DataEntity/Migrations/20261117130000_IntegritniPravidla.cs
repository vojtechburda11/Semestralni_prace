using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
namespace DataEntity.Migrations;
public partial class IntegritniPravidla : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("EXEC(N'" + """
        CREATE OR ALTER TRIGGER TR_Rezervace_Pravidla ON dbo.Rezervace
        AFTER INSERT, UPDATE, DELETE AS
        BEGIN
            SET NOCOUNT ON;
            -- Transakční zámek serializuje konkurenční kontroly. Případný deadlock
            -- jednu transakci zruší; konfliktní rezervace se nikdy neuloží obě.
            DECLARE @lockResult int;
            EXEC @lockResult = sys.sp_getapplock @Resource=N'Rezervace_Integrita',
                @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
            IF @lockResult < 0 THROW 51000, N'Kontrola rezervace je zaneprázdněna. Opakujte operaci.', 1;
            DECLARE @now datetime2 = SYSUTCDATETIME();
            IF EXISTS (SELECT 1 FROM deleted WHERE CasOd <= @now)
                THROW 51001, N'Probíhající a uskutečněné rezervace nelze měnit ani mazat.', 1;
            IF EXISTS (SELECT 1 FROM inserted WHERE CasOd <= @now)
                THROW 51002, N'Začátek rezervace musí být v budoucnosti (UTC).', 1;
            IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON d.Id=i.Id WHERE i.DatumVytvoreni<>d.DatumVytvoreni)
                THROW 51003, N'Datum vytvoření rezervace nelze změnit.', 1;
            IF EXISTS (
                SELECT 1 FROM inserted i JOIN dbo.Rezervace r WITH (UPDLOCK, HOLDLOCK)
                ON r.Id<>i.Id AND r.CasOd<i.CasDo AND i.CasOd<r.CasDo
                AND (r.MistnostId=i.MistnostId OR r.ZamestnanecId=i.ZamestnanecId))
                THROW 51004, N'Překryv rezervací místnosti nebo zaměstnance.', 1;
        END
        """.Replace("'", "''") + "');");
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Rezervace_Pravidla;");
}
