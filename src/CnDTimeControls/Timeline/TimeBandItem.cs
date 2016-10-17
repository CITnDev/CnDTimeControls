using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CnDTimeControls.Annotations;

namespace CnDTimeControls.Timeline
{
    public sealed class TimeBandItem : INotifyPropertyChanged
    {
        #region Left

        public double Left
        {
            get { return _left; }
            set
            {
                if (_left != value)
                {
                    _left = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region DateTime

        private DateTime? _dateTime;
        private double _left;

        public DateTime? DateTime
        {
            get { return _dateTime; }
            set
            {
                if (_dateTime != value)
                {
                    _dateTime = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion
    }
}