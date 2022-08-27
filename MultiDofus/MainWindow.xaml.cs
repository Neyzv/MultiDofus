using System.Windows;
using System.Windows.Input;

namespace MultiDofus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            DragMove();

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
            Close();
    }
}
