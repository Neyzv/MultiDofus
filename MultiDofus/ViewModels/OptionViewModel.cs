using System;
using System.Collections.Generic;
using System.Media;
using System.Windows.Input;
using MultiDofus.Commands;
using MultiDofus.Framework;

namespace MultiDofus.ViewModels
{
    internal class OptionViewModel : ViewModelBase<OptionWindow>
    {
        private static readonly HashSet<Win32API.VirtualKeys> _availableKeys = new()
        {
            Win32API.VirtualKeys.Tab,
            Win32API.VirtualKeys.Enter,
            Win32API.VirtualKeys.Multiply,
            Win32API.VirtualKeys.Add,
            Win32API.VirtualKeys.Substract,
            Win32API.VirtualKeys.Divide,
            Win32API.VirtualKeys.F1,
            Win32API.VirtualKeys.F2,
            Win32API.VirtualKeys.F3,
            Win32API.VirtualKeys.F4,
            Win32API.VirtualKeys.F5,
            Win32API.VirtualKeys.F6,
            Win32API.VirtualKeys.F7,
            Win32API.VirtualKeys.F8,
            Win32API.VirtualKeys.F9,
            Win32API.VirtualKeys.F10,
            Win32API.VirtualKeys.F11,
            Win32API.VirtualKeys.F12,
            Win32API.VirtualKeys.UpArrow,
            Win32API.VirtualKeys.DownArrow,
            Win32API.VirtualKeys.LeftArrow,
            Win32API.VirtualKeys.RightArrow,
            Win32API.VirtualKeys.NumericKeyPadZero,
            Win32API.VirtualKeys.NumericKeyPadOne,
            Win32API.VirtualKeys.NumericKeyPadTwo,
            Win32API.VirtualKeys.NumericKeyPadThree,
            Win32API.VirtualKeys.NumericKeyPadFour,
            Win32API.VirtualKeys.NumericKeyPadFive,
            Win32API.VirtualKeys.NumericKeyPadSix,
            Win32API.VirtualKeys.NumericKeyPadSeven,
            Win32API.VirtualKeys.NumericKeyPadEight,
            Win32API.VirtualKeys.NumericKeyPadNine,
        };

        public ICommand CloseCommand =>
            new RelayCommand(_ => SaveAndClose());

        public OptionViewModel(OptionWindow view) : base(view)
        {
            var keys = Enum.GetNames<Key>();

            View.NextWindowKeys.ItemsSource = _availableKeys;
            View.NextWindowKeys.SelectedIndex = View.NextWindowKeys.Items.IndexOf(Configuration.NextWindowKey);

            View.PreviousWindowKeys.ItemsSource = _availableKeys;
            View.PreviousWindowKeys.SelectedIndex = View.NextWindowKeys.Items.IndexOf(Configuration.PreviousWindowKey);

            View.DeathTimeClick.Text = Configuration.DeathTimeClick.ToString();
            View.DeathTimeSwap.Text = Configuration.DeathTimeSwap.ToString();
        }

        private void SaveAndClose()
        {
            if (int.TryParse(View.DeathTimeClick.Text, out var deathTimeClick) && deathTimeClick is >= 30 and <= 3000 &&
                int.TryParse(View.DeathTimeSwap.Text, out var deathTimeSwap) && deathTimeSwap is >= 30 and <= 3000 &&
                Enum.TryParse<Win32API.VirtualKeys>(View.NextWindowKeys.Items[View.NextWindowKeys.SelectedIndex].ToString(), out var nextWindowKey) && _availableKeys.Contains(nextWindowKey) &&
                Enum.TryParse<Win32API.VirtualKeys>(View.PreviousWindowKeys.Items[View.PreviousWindowKeys.SelectedIndex].ToString(), out var previousWindowKey) && _availableKeys.Contains(previousWindowKey))
            {
                Configuration.DeathTimeClick = deathTimeClick;
                Configuration.DeathTimeSwap = deathTimeSwap;
                Configuration.NextWindowKey = nextWindowKey;
                Configuration.PreviousWindowKey = previousWindowKey;

                View.Close();
            }
            else
                SystemSounds.Hand.Play();
        }
    }
}
