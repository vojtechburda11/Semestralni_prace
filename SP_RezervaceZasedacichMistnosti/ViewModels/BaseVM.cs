using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace SP_RezervaceZasedacichMistnosti.ViewModels;
public abstract class BaseVM : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Notify([CallerMemberName] string? p = null) => PropertyChanged?.Invoke(this, new(p));
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? p = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value; Notify(p); return true;
    }
}
