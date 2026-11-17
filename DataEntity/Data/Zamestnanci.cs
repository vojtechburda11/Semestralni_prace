using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataEntity;
public class Zamestnanci : BaseModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vyplňte jméno."), MaxLength(100)] public string Jmeno { get; set; } = "";
    [Required(ErrorMessage = "Vyplňte příjmení."), MaxLength(100)] public string Prijmeni { get; set; } = "";
    [Required(ErrorMessage = "Vyplňte e-mail."), EmailAddress(ErrorMessage = "E-mail nemá platný formát."), MaxLength(254)] public string Email { get; set; } = "";
    public virtual ICollection<Rezervace> Rezervace { get; set; } = new List<Rezervace>();
    [NotMapped] public string CeleJmeno => $"{Jmeno} {Prijmeni}";
    public override string ToString() => $"{CeleJmeno} – {Email}";
}
