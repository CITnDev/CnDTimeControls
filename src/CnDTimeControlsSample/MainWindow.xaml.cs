using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CnDTimeLineSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(new Action(() => cndTb.Focus()), System.Windows.Threading.DispatcherPriority.Input);

            var vm = new VM();

            DataContext = vm;

            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        var datetime = vm.MyDate.AddMinutes(1);
            //        vm.MyDate = datetime;
            //        Thread.Sleep(100);
            //    }

            //});
        }
    }

    public class VM : INotifyPropertyChanged
    {
        private DateTime _myDate;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        public VM()
        {
            //MyDate = DateTime.MinValue;
            //MyDate = new DateTime(2015,10,25,0,0,0,DateTimeKind.Utc);
            //MyDate = new DateTime(2015, 10, 25, 2, 30, 0, DateTimeKind.Local);
            MyDate = new DateTime(2015, 10, 1, 12, 0, 0, DateTimeKind.Local);
            //MyDate = DateTime.Now;
        }

        public DateTime MyDate
        {
            get { return _myDate; }
            set
            {
                if (_myDate != value || _myDate.ToUniversalTime()!= value.ToUniversalTime())
                {
                    _myDate = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("MyDate"));
                }
            }
        }

        public DateTime CurrentDate
        {
            get { return new DateTime(2015,10,24,0,0,0,DateTimeKind.Local); }
        }
    }
}
