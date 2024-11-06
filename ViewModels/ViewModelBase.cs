using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QwertyLauncher
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool RaisePropertyChangedIfSet<TResult>(ref TResult source,
           TResult value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TResult>.Default.Equals(source, value))
                return false;
            source = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
