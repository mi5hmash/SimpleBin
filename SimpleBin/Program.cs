using Velopack;

namespace SimpleBin
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            VelopackApp
                .Build()
                .OnBeforeUninstallFastCallback((v) =>
                {
                    if(StartupHelper.IsInStartup()) StartupHelper.RemoveFromStartup();
                })
                .Run();

            BinHelper? binHelper = null;
            IconHelper iconHelper = new(MainWindow.IsDarkThemeEnabled());

            try
            {
                binHelper = new BinHelper();

                ApplicationConfiguration.Initialize();
                Application.Run(new MainWindow(binHelper, iconHelper));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                binHelper?.Dispose();
            }
        }
    }
}