using System.ComponentModel;
namespace DataEntity;
public static class Enums
{
    public enum RezervaceStav
    {
        [Description("Budoucí")] Budouci,
        [Description("Probíhající")] Probihajici,
        [Description("Uskutečněná")] Uskutecnena
    }
}
