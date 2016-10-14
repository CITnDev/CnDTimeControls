using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CnDTimeControls.Annotations;
using Control = System.Windows.Controls.Control;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace CnDTimeControls
{


    public class CnDTimeLine : Control
    {
        private volatile bool _timelineMoving = false;
        static CnDTimeLine()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CnDTimeLine), new FrameworkPropertyMetadata(typeof(CnDTimeLine)));
        }


        public CnDTimeLine()
        {
            Loaded += CnDTimeLine_Loaded;
            Unloaded += CnDTimeLine_Unloaded;
        }

        private void CnDTimeLine_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timelineMoving)
                StopMoveTask();
            this.RemoveHandler(UIElement.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            this.RemoveHandler(UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }

        private void CnDTimeLine_Loaded(object sender, RoutedEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            if (_timelineMoving)
                StopMoveTask();
            _timelineMoving = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (_timelineMoving && args.LeftButton == MouseButtonState.Pressed)
            {
                _currentMousePosition = args.GetPosition(this);
            }
        }

        #region Dependency property

        #region TimeBand

        public static DependencyProperty TimeBandProperty = DependencyProperty.Register("TimeBand", typeof(List<TimeBandItem>), typeof(CnDTimeLine), new PropertyMetadata(new List<TimeBandItem>()));

        [Description("TimeBandProperty")]
        [Category("CnDTimeLine Category")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public List<TimeBandItem> TimeBand
        {
            get
            {
                return ((List<TimeBandItem>)(GetValue(TimeBandProperty)));
            }
            set
            {
                SetValue(TimeBandProperty, value);
            }
        }

        #endregion


        #region CurrentTime

        public static DependencyProperty CurrentTimeProperty = DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(CnDTimeLine), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCurrentTimeChange));

        private static void OnCurrentTimeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CnDTimeLine)d;
            if (!ctrl._internalSet)
            {
                var newTime = (DateTime) e.NewValue;
                ctrl._currentTime = newTime;
                ctrl.BuildTimeBandData(newTime);
            }
        }

        [Description("CurrentTime")]
        [Category("CnDTimeLine Category")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public DateTime CurrentTime
        {
            get
            {
                return ((DateTime)(GetValue(CurrentTimeProperty)));
            }
            set
            {
                SetValue(CurrentTimeProperty, value);
            }
        }

        #endregion

        #endregion

        #region Private fields

        private Task _moveTask;
        private CancellationTokenSource _tokenSource;
        private const int DefaultTimeBandItemWidth = 100;
        private const long DefaultTimeBandItemDurationInSeconds = 1;
        private const int DelayRefreshInMs = 40;
        private Point _currentMousePosition;
        private List<TimeBandItem> _timeBandItems = new List<TimeBandItem>();
        private volatile bool _internalSet = false;
        private DateTime _currentTime;

        #endregion

        #region Override methods

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _currentMousePosition = Mouse.GetPosition(this);
            _timelineMoving = true;
            lock (this)
            {
                if (_moveTask != null)
                {
                    _tokenSource.Cancel(false);
                    _moveTask.Wait(40);
                    _moveTask.Dispose();
                    _tokenSource.Dispose();
                }
                _tokenSource = new CancellationTokenSource();
                _moveTask = new Task(MoveTimeline, _tokenSource.Token);
                _moveTask.Start();
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            BuildTimeBandData(CurrentTime);
        }

        #endregion

        #region Private methods

        private void MoveTimeline()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                var ratio = GetRatio();
                ShiftTimeBand((ratio * DelayRefreshInMs)/1000);

                Thread.Sleep(DelayRefreshInMs);
            }
        }

        private double GetRatio()
        {
            double ratio = 0;

            var mousePosition = _currentMousePosition;
            var middle = ActualWidth / 2;
            if (mousePosition.X < middle)
            {
                // Move backward
                ratio = mousePosition.X/middle;

                if (ratio >= 0.75)
                {
                    // Less or equals to x1
                    ratio = -Math.Abs(ratio - 1);
                }
                else
                {
                    // Greater than x1
                    ratio = -((Math.Abs(ratio - 0.75)*19/0.75) + 1);
                }
            }
            else if (mousePosition.X > middle)
            {
                // Move forward
                ratio = (mousePosition.X - middle)/middle;

                if (ratio <= 0.25)
                {
                    // Less or equals to x1
                    ratio = ratio*4; // equals to ratio / 0.25
                }
                else
                {
                    // Greater than x1
                    ratio = (ratio - 0.25)*19 + 1;
                }
            }

            return ratio;
        }

        private void BuildTimeBandData(DateTime newTime)
        {
            if (newTime == DateTime.MinValue || newTime == DateTime.MaxValue)
                return;

            if (double.IsNaN(ActualWidth))
                return;

            DateTime startTimeRange;
            DateTime endTimeRange;
            GetTimeLineBounds(newTime, out startTimeRange, out endTimeRange);
            var items = new List<TimeBandItem>();

            // Compute the first visible item
            var itemTime =
                startTimeRange.AddTicks(-startTimeRange.Ticks % (10000000 * DefaultTimeBandItemDurationInSeconds))
                    .AddSeconds(DefaultTimeBandItemDurationInSeconds); //TimeBandItem duration round            

            for (; itemTime < endTimeRange; itemTime = itemTime.AddSeconds(DefaultTimeBandItemDurationInSeconds))
            {
                var leftOffset = itemTime.Subtract(startTimeRange).TotalSeconds * DefaultTimeBandItemWidth /
                             DefaultTimeBandItemDurationInSeconds;

                var item = new TimeBandItem
                {
                    DateTime = itemTime,
                    Left = leftOffset,
                };

                items.Add(item);
            }
            _timeBandItems = items;

            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(() => TimeBand = items);
            else
                TimeBand = items;
        }

        private void GetTimeLineBounds(DateTime newTime, out DateTime startTimeRange, out DateTime endTimeRange)
        {
            var mediumTimeRange = (ActualWidth*DefaultTimeBandItemDurationInSeconds)/(DefaultTimeBandItemWidth*2);
            startTimeRange = newTime.AddSeconds(-mediumTimeRange);
            endTimeRange = newTime.AddSeconds(mediumTimeRange);
        }

        private void ShiftTimeBand(double timeShiftInSeconds)
        {
            _internalSet = true;
            _currentTime = _currentTime.AddSeconds(timeShiftInSeconds);
            Dispatcher.Invoke(() =>
            {
                CurrentTime = _currentTime;
            }
                );
            _internalSet = false;
            
            if (_timeBandItems.Count == 0)
                return;


            var visualOffset = - (timeShiftInSeconds*DefaultTimeBandItemWidth/DefaultTimeBandItemDurationInSeconds);
            if ((_timeBandItems[0].Left + visualOffset) < 0 || _timeBandItems[_timeBandItems.Count - 1].Left + visualOffset > ActualWidth)
            {
                //Out of range drawing
                BuildTimeBandData(_currentTime);
            }
            else
            {
                foreach (var item in _timeBandItems)
                {                    
                    item.Left += visualOffset;
                }
            }
        }

        private void StopMoveTask()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _moveTask.Wait(40);
                _moveTask.Dispose();
                _tokenSource.Dispose();
                _moveTask = null;
                _tokenSource = null;
            }
        }

        #endregion

    }

    public class TimeBandItem : INotifyPropertyChanged
    {

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
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
