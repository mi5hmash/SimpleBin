using System.Text.RegularExpressions;

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
            BinHelper? binHelper = null;
            IconHelper iconHelper = new(MainWindow.IsDarkThemeEnabled());

            // Dark mode is experimental feature for now, so we disable the warning
#pragma warning disable WFO5001
            Application.SetColorMode(SystemColorMode.System);
#pragma warning restore WFO5001
            try
            {
                binHelper = new BinHelper();

                ApplicationConfiguration.Initialize();
                Application.Run(new MainWindow(binHelper, iconHelper));
            }
            catch(Exception ex) 
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