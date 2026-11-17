using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Windows;
namespace SP_RezervaceZasedacichMistnosti;
public static class Globals {
    public static void Chyba(Exception ex) {
        var message = ex switch {
            DbUpdateConcurrencyException => "Záznam mezitím změnil jiný uživatel. Zavřete formulář a obnovte přehled.",
            ValidationException => ex.Message,
            DbUpdateException => ex.InnerException?.Message ?? ex.Message,
            _ => ex.Message
        };
        MessageBox.Show(message, "Operaci nelze dokončit", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
