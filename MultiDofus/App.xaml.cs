using System.Windows;
using MultiDofus.ViewModels;

namespace MultiDofus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new MainViewModel(new()).View;
            MainWindow.Show();
        }
    }
}
