using System;
using System.Collections.Generic;
using System.ComponentModel;
using Invert.uFrame.Editor.Annotations;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        private object _dataObject;

        public object DataObject
        {
            get { return _dataObject; }
            set
            {
                _dataObject = value;
                DataObjectChanged();
            }
        }

        protected virtual void DataObjectChanged()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T backingField, T value, string name)
        {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, value);

            if (changed)
            {
                backingField = value;
                this.OnPropertyChanged(name);
            }

            return changed;
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class ViewModelExtensions
    {
        public static Action SubscribeToProperty<TViewModel>(this TViewModel vm, string propertyName, Action<TViewModel> action) where TViewModel : ViewModel
        {
            PropertyChangedEventHandler handler = (sender, args) =>
            {
                action(sender as TViewModel);
            };;
            vm.PropertyChanged += handler;

            return ()=> { vm.PropertyChanged -= handler; };
        }
    }
}