using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MultiDofus.Commands;
using MultiDofus.Framework;

namespace MultiDofus.ViewModels
{
    internal class MainViewModel : ViewModelBase<MainWindow>
    {
        private const byte MaxWindowsCount = 8;

        private static readonly SolidColorBrush WhiteBrush;
        private static readonly SolidColorBrush TransparentBlackBrush;

        private readonly List<DofusProcess> _runningDofusProcesses;

        private int _selectedDofusProcessId = -1;

        #region Commands
        public ICommand MinimizeCommand =>
            new RelayCommand(_ => View.WindowState = WindowState.Minimized, CanMinimize);

        public ICommand FullScreenCommand =>
            new RelayCommand(_ => View.WindowState = View.WindowState is WindowState.Normal ? WindowState.Maximized : WindowState.Normal);

        public ICommand CloseCommand =>
            new RelayCommand(_ => View.Close());

        public ICommand SynchronizeCommand =>
            new RelayCommand(AttachDofusProcess);

        public ICommand ChangeForegroundWindowCommand =>
            new RelayCommand<Button>(ChangeForegroundWindow);

        public ICommand OpenOptionsCommand =>
            new RelayCommand(_ => new OptionViewModel(new()).View.ShowDialog());

        public ICommand CloseDofusCommand =>
            new RelayCommand<string>(Close);

        public ICommand RenameDofusCommand =>
            new RelayCommand<string>(Rename);
        #endregion

        static MainViewModel()
        {
            WhiteBrush = new SolidColorBrush(Colors.White);
            TransparentBlackBrush = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0));
        }

        public MainViewModel(MainWindow view) : base(view)
        {
            _runningDofusProcesses = new();

            Win32API.OnKeyUp += HandleKey;
            View.Closed += (_, __) =>
            {
                Win32API.OnKeyUp -= HandleKey;

                KillAllDofusProcess();
            };

            View.SizeChanged += (_, __) =>
            {
                foreach (var dofusProcess in _runningDofusProcesses)
                    Resize(dofusProcess);
            };

            DragIt.Dragged += OnButtonDragged;
        }

        #region CommandControler
        private bool CanMinimize(object? _) =>
        View.WindowState is not WindowState.Minimized;
        #endregion

        private void OnButtonDragged(UIElement element)
        {
            if (element is Button button && button.Parent == View.ButtonContainer && int.TryParse(button.Name[1..], out var processId))
            {
                var dofusProcess = _runningDofusProcesses.FirstOrDefault(x => x.Process.Id == processId);

                if (dofusProcess is not null && Math.Abs(button.RenderTransform.Value.OffsetX) > 10)
                {
                    var buttonPosition = button.ActualWidth + button.TranslatePoint(new(), View.ButtonContainer).X;

                    var buttonsDistances = new KeyValuePair<DofusProcess, double>[_runningDofusProcesses.Count - 1];

                    var i = 0;
                    foreach (var dp in _runningDofusProcesses)
                        if (dp.Process.Id != processId && i < buttonsDistances.Length)
                        {
                            buttonsDistances[i] = new(dp,
                                dp.Button.ActualWidth + dp.Button.TranslatePoint(new(), View.ButtonContainer).X);
                            i++;
                        }

                    var dofusProcessToSwitch = buttonsDistances.OrderBy(x => Math.Pow(x.Value - buttonPosition, 2)).FirstOrDefault().Key;

                    if (dofusProcessToSwitch is not null)
                    {
                        var matrix2 = button.RenderTransform.Value;
                        matrix2.SetIdentity();
                        button.RenderTransform = new MatrixTransform(matrix2);

                        var dofusProcessIndex = GetProcessIndex(processId);
                        var dofusProcessToSwitchIndex = GetProcessIndex(dofusProcessToSwitch.Process.Id);

                        if (dofusProcessIndex >= 0 && dofusProcessToSwitchIndex >= 0)
                        {
                            _runningDofusProcesses[dofusProcessIndex] = dofusProcessToSwitch;
                            _runningDofusProcesses[dofusProcessToSwitchIndex] = dofusProcess;

                            View.ButtonContainer.Children.Clear();

                            foreach (var dp in _runningDofusProcesses)
                                View.ButtonContainer.Children.Add(dp.Button);
                        }
                    }
                }
            }

            var matrix = element.RenderTransform.Value;
            matrix.SetIdentity();
            element.RenderTransform = new MatrixTransform(matrix);
        }

        private static void KillAllDofusProcess()
        {
            foreach (var dofusProcess in Process.GetProcessesByName("Dofus"))
                dofusProcess.Kill();
        }

        private void Close(string dofusProcessIdStr)
        {
            if (int.TryParse(dofusProcessIdStr[1..], out var dofusProcessId))
            {
                var dofusProcessIndex = GetProcessIndex(dofusProcessId);

                if (dofusProcessIndex >= 0 && View.DofusContainer.Child is System.Windows.Forms.Panel panel)
                {
                    _runningDofusProcesses[dofusProcessIndex].Process.Kill();
                    panel.Controls.Remove(_runningDofusProcesses[dofusProcessIndex].Panel);
                    View.ButtonContainer.Children.Remove(_runningDofusProcesses[dofusProcessIndex].Button);
                    _runningDofusProcesses.RemoveAt(dofusProcessIndex);
                }
            }
        }

        private void Rename(string dofusProcessIdStr)
        {
            if (int.TryParse(dofusProcessIdStr[1..], out var dofusProcessId))
            {
                var dofusProcess = _runningDofusProcesses.FirstOrDefault(x => x.Process.Id == dofusProcessId);

                if (dofusProcess is not null && dofusProcess.Button.Content is TextBlock textBlock)
                    new RenameViewModel(new(), this, textBlock.Text, dofusProcessId).View.ShowDialog();
            }
        }

        private void Resize(DofusProcess process) =>
            Win32API.SetWindowPos(process.Process.MainWindowHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (IntPtr)process.Panel.Width, (IntPtr)process.Panel.Height,
                (IntPtr)0x10);

        private int GetProcessIndex(int processId) =>
            _runningDofusProcesses.IndexOf(_runningDofusProcesses.First(x => x.Process.Id == processId));

        private void SetFocus(int processId)
        {
            foreach (var dofusProcess in _runningDofusProcesses.OrderBy(x => x.Process.Id == _selectedDofusProcessId))
                if (dofusProcess.Process.Id == processId)
                {
                    _selectedDofusProcessId = processId;
                    dofusProcess.Panel.Visible = true;

                    Win32API.SetForegroundWindow(dofusProcess.Process.MainWindowHandle);
                    Win32API.SetFocus(dofusProcess.Process.MainWindowHandle);

                }
                else
                    dofusProcess.Panel.Visible = false;
        }

        private void AttachDofusProcess(object? _)
        {
            if (_runningDofusProcesses.Count < MaxWindowsCount)
            {
                foreach (var process in Process.GetProcessesByName("Dofus").Where(x => _runningDofusProcesses.FirstOrDefault(y => y.Process.Id == x.Id) is null))
                {
                    var button = new Button()
                    {
                        Style = (Style)Application.Current.Resources["BorderedButtonStyle"],
                        MinWidth = 80,
                        Height = 35,
                        Content = new TextBlock()
                        {
                            Text = process.ProcessName,
                            Foreground = WhiteBrush,
                            FontSize = 14,
                        },
                        Background = TransparentBlackBrush,
                        BorderThickness = new(1),
                        BorderBrush = WhiteBrush,
                        Name = $"_{process.Id}",
                        Command = ChangeForegroundWindowCommand,
                        ContextMenu = new ContextMenu(),
                    };

                    button.CommandParameter = button;
                    button.ContextMenu.Items.Add(new MenuItem()
                    {
                        Header = "Renommer",
                        Command = RenameDofusCommand,
                        CommandParameter = button.Name,

                    });
                    button.ContextMenu.Items.Add(new MenuItem()
                    {
                        Header = "Fermer",
                        Command = CloseDofusCommand,
                        CommandParameter = button.Name,
                    });

                    View.ButtonContainer.Children.Add(button);

                    var panel = new System.Windows.Forms.Panel()
                    {
                        Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom,
                        Visible = false,
                        Width = (int)View.DofusContainer.ActualWidth,
                        Height = (int)View.DofusContainer.ActualHeight,
                    };

                    button.DragMove(disableYAxis: true, minXFunc: () => -button.TranslatePoint(new(), View.ButtonContainer).X, maxXFunc: () => panel.Width - button.ActualWidth - button.TranslatePoint(new(), View.ButtonContainer).X);

                    (View.DofusContainer.Child as System.Windows.Forms.Panel)!.Controls.Add(panel);

                    Win32API.SetParent(process.MainWindowHandle, panel.Handle);
                    Win32API.SetWindowLongPtr(process.MainWindowHandle, Win32API.GWL_STYLE, (IntPtr)(
                                          // Child window should be have a thin-line border
                                          Win32API.WindowStyles.WS_BORDER |
                                          Win32API.WindowStyles.WS_VISIBLE));

                    var dofusProcess = new DofusProcess(process, button, panel);
                    _runningDofusProcesses.Add(dofusProcess);

                    Resize(dofusProcess);
                    SetFocus(process.Id);
                }
            }
        }

        private void ChangeForegroundWindow(Button button)
        {
            if (int.TryParse(button.Name[1..], out var processId))
                SetFocus(processId);
        }

        private async Task MoveOnAllInstances(int x, int y)
        {
            var currentProcessId = _selectedDofusProcessId;

            foreach (var instance in _runningDofusProcesses)
            {
                SetFocus(instance.Process.Id);
                await Task.Delay(Configuration.DeathTimeSwap);
                Win32API.SetCursorPos(x, y);
                Win32API.mouse_event(Win32API.MouseLeftButtonDown | Win32API.MouseLeftButtonUp, x, y, 0, 0);
                await Task.Delay(Configuration.DeathTimeClick);
            }

            SetFocus(currentProcessId);
        }

        private void HandleKey(Win32API.VirtualKeys key)
        {
            if (_runningDofusProcesses.Count > 1)
            {
                if (key == Configuration.NextWindowKey)
                    View.Dispatcher.Invoke(() =>
                    {
                        var dofusProcessIndex = GetProcessIndex(_selectedDofusProcessId);

                        if (dofusProcessIndex >= 0)
                            SetFocus(_runningDofusProcesses[(dofusProcessIndex + 1) % _runningDofusProcesses.Count].Process.Id);
                    });
                else if (key == Configuration.PreviousWindowKey)
                {
                    var nextIndex = GetProcessIndex(_selectedDofusProcessId) - 1;

                    if (nextIndex >= -1)
                        View.Dispatcher.Invoke(() =>
                            SetFocus(_runningDofusProcesses[nextIndex < 0 ? _runningDofusProcesses.Count - 1 : nextIndex].Process.Id));
                }
                else if (key == Win32API.VirtualKeys.MiddleMouseButton && Win32API.GetCursorPos(out var mousePosition))
                    View.Dispatcher.Invoke(() =>
                        MoveOnAllInstances(mousePosition.X, mousePosition.Y));
            }
        }

        public void RenameDofus(int dofusProcessId, string newName)
        {
            var dofusProcess = _runningDofusProcesses.FirstOrDefault(x => x.Process.Id == dofusProcessId);

            if (dofusProcess is not null && dofusProcess.Button.Content is TextBlock textBlock)
                textBlock.Text = newName;
        }
    }
}
