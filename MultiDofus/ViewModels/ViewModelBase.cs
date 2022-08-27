using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MultiDofus.ViewModels
{
    internal class ViewModelBase<TWindow> : INotifyPropertyChanged
        where TWindow : Window
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public TWindow View { get; }

        public ViewModelBase(TWindow view)
        {
            View = view;
            view.DataContext = this;
        }

        protected virtual void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? property = default)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return;

            storage = value;
            RaisePropertyChanged(property);
        }

        protected virtual void SetProperty<T>(ref T storage, T value, Delegate onChanged, [CallerMemberName] string? property = default)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return;

            storage = value;
            onChanged?.DynamicInvoke();
            RaisePropertyChanged(property);
        }

        protected void RaisePropertyChanged([CallerMemberName] string? property = default) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
