using System;
using System.Threading.Tasks;
using System.Windows;
using DEBA.StockApp.Data;

namespace DEBA.StockApp
{
    public partial class App : Application
    {
        private IDatabaseService? _databaseService;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                _databaseService = new DatabaseService();
                await _databaseService.InitializeAsync();

                // Ouvrir la fenêtre principale
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'initialisation de la base : {ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }
    }
}
