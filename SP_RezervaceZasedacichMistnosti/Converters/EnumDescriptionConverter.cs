using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
namespace SP_RezervaceZasedacichMistnosti.Converters;
public class EnumDescriptionConverter : IValueConverter {
    public object Convert(object value, Type t, object p, CultureInfo c) => value?.GetType().GetField(value.ToString() ?? "")?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value?.ToString() ?? "";
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
public class LocalTimeConverter : IValueConverter {
    public object Convert(object value, Type t, object p, CultureInfo c) => value is DateTime d ? DateTime.SpecifyKind(d, DateTimeKind.Utc).ToLocalTime().ToString("dd.MM.yyyy HH:mm") : "";
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
