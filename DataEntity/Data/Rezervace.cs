using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataEntity;
public class Rezervace : BaseModel, IValidatableObject
{
    public int Id { get; set; }
    [Range(1, int.MaxValue)] public int MistnostId { get; set; }
    public virtual Mistnosti Mistnost { get; set; } = null!;
    [Range(1, int.MaxValue)] public int ZamestnanecId { get; set; }
    public virtual Zamestnanci Zamestnanec { get; set; } = null!;
    public DateTime CasOd { get; set; }
    public DateTime CasDo { get; set; }
    [Required(ErrorMessage = "Vyplňte účel rezervace."), MaxLength(500)] public string Ucel { get; set; } = "";
    [NotMapped] public Enums.RezervaceStav Stav => CasDo <= DateTime.UtcNow ? Enums.RezervaceStav.Uskutecnena
        : CasOd <= DateTime.UtcNow ? Enums.RezervaceStav.Probihajici : Enums.RezervaceStav.Budouci;
    public IEnumerable<ValidationResult> Validate(ValidationContext context) {
        if (CasDo <= CasOd) yield return new("Ukončení musí být pozdější než zahájení.", [nameof(CasDo)]);
        if (CasDo - CasOd > TimeSpan.FromHours(8)) yield return new("Rezervace smí trvat nejvýše 8 hodin.", [nameof(CasDo)]);
    }
}
