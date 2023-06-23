using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffPriceMonitor.ViewModels.BaseView
{
    internal class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnProperty([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        // Метод установки свойст
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnProperty(PropertyName);

            return true;

        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;

        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposing || _disposed) return;
            _disposed = true;
        }

    }
}
