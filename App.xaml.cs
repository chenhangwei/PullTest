using ControlzEx.Theming;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PullTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set the application theme to Dark.Green
            //ThemeManager.Current.ChangeTheme(this, "Dark.Lime");
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
        }
    }

}
