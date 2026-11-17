using System.ComponentModel.DataAnnotations;
namespace DataEntity;
public class Mistnosti : BaseModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vyplňte název místnosti."), MaxLength(100)] public string Nazev { get; set; } = "";
    [Range(1, int.MaxValue, ErrorMessage = "Kapacita musí být větší než 0.")] public int Kapacita { get; set; }
    public virtual ICollection<Rezervace> Rezervace { get; set; } = new List<Rezervace>();
    public override string ToString() => $"{Nazev} (kapacita: {Kapacita})";
}
