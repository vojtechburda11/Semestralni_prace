using System.Windows;
using System.Globalization;
using System.Windows.Markup;
using DataEntity;
using Microsoft.EntityFrameworkCore;
using SP_RezervaceZasedacichMistnosti.Views;
namespace SP_RezervaceZasedacichMistnosti;
public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("cs-CZ");
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("cs-CZ")));
        try {
            using var db = new RezervaceContext();
            db.Database.Migrate();
            new MainWindow().Show();
        } catch(Exception ex) { Globals.Chyba(ex); Shutdown(1); }
    }
}
