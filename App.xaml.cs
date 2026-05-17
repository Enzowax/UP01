using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ShutIKrol.Data;

namespace ShutIKrol
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Load saved connection settings
            var settings = DbSettings.Load();
            DB.ConnectionString = settings.ToConnectionString();

            // Test connection; show settings window if it fails
            if (!TestConnection())
            {
                var win = new ConnectionSettingsWindow();
                bool? ok = win.ShowDialog();
                if (ok != true)
                {
                    // User closed settings without saving — exit
                    Shutdown();
                    return;
                }
            }

            // All good — start the app
            new LoginWindow().Show();
        }

        private static bool TestConnection()
        {
            try
            {
                var opts = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(DB.ConnectionString).Options;
                using var ctx = new AppDbContext(opts);
                return ctx.Database.CanConnect();
            }
            catch
            {
                return false;
            }
        }
    }
}
