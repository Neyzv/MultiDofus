using System.Media;
using System.Windows.Input;
using MultiDofus.Commands;

namespace MultiDofus.ViewModels
{
    internal class RenameViewModel : ViewModelBase<RenameWindow>
    {
        private readonly MainViewModel _main;
        private readonly int _dofusProcessId;

        public ICommand CloseCommand =>
            new RelayCommand(_ => SaveAndClose());

        public RenameViewModel(RenameWindow view, MainViewModel main, string windowName, int dofusProcessId) : base(view)
        {
            _main = main;
            _dofusProcessId = dofusProcessId;
            View.WindowName.Text = windowName;
        }

        private void SaveAndClose()
        {
            if (!string.IsNullOrEmpty(View.WindowName.Text) && View.WindowName.Text.Length <= 30)
            {
                _main.RenameDofus(_dofusProcessId, View.WindowName.Text);
                View.Close();
            }
            else
                SystemSounds.Hand.Play();
        }
    }
}
