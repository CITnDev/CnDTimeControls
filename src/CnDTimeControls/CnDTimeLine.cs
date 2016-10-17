using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CnDTimeControls.Annotations;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace CnDTimeControls
{
    public class CnDTimeLine : Control
    {
        static CnDTimeLine()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CnDTimeLine), new FrameworkPropertyMetadata(typeof(CnDTimeLine)));
        }

        public CnDTimeLine()
        {
            _pointDuration = DefaultTimeBandItemDurationInSeconds/DefaultTimeBandItemWidth;
            _cnDTimeLineBehavior = new CnDTimeLineSpeedBehavior();
            Loaded += CnDTimeLine_Loaded;
            Unloaded += CnDTimeLine_Unloaded;
        }


        public CndTimeLineBehaviorType Mode
        {
            get
            {
                if (_cnDTimeLineBehavior is CnDTimeLineSpeedBehavior)
                    return CndTimeLineBehaviorType.Speed;

                return CndTimeLineBehaviorType.Draging;
            }
            set
            {
                if (value == CndTimeLineBehaviorType.Speed)
                    _cnDTimeLineBehavior = new CnDTimeLineSpeedBehavior();
                else
                    _cnDTimeLineBehavior = new CnDTimeLineDragBehavior();
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
            get { return (List<TimeBandItem>) GetValue(TimeBandProperty); }
            set { SetValue(TimeBandProperty, value); }
        }

        #endregion

        #region CurrentTime

        public static DependencyProperty CurrentTimeProperty = DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(CnDTimeLine), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCurrentTimeChange));

        private static void OnCurrentTimeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CnDTimeLine) d;
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
            get { return (DateTime) GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        #endregion

        #endregion

        #region Private fields

        private double _actualWidth;
        private Task _moveTask;
        private CancellationTokenSource _tokenSource;
        private const double DefaultTimeBandItemWidth = 100;
        private const long DefaultTimeBandItemDurationInSeconds = 1;
        private const int DelayRefreshInMs = 40;
        private readonly double _pointDuration;
        private Point _currentMousePosition;
        private Point _previousPosition;
        private List<TimeBandItem> _timeBandItems = new List<TimeBandItem>();
        private volatile bool _internalSet;
        private DateTime _currentTime;
        private ICnDTimeLineBehavior _cnDTimeLineBehavior;
        private volatile bool _timelineMoving;
        private DateTime _startTimeRange;
        private DateTime _endTimeRange;

        #endregion

        #region Override methods

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _currentMousePosition = Mouse.GetPosition(this);
            _previousPosition = _currentMousePosition;
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
            _actualWidth = ActualWidth;
            BuildTimeBandData(CurrentTime);
        }

        #endregion

        #region Private methods

        private TimeSpan Position2Time(double x)
        {
            var timeLineDuration = _endTimeRange.Subtract(_startTimeRange).TotalSeconds;
            return TimeSpan.FromSeconds(x * timeLineDuration/ ActualWidth);
        }

        private void MoveTimeline()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                ShiftTimeBand(_cnDTimeLineBehavior.GetTimeShifting(_previousPosition.X, _currentMousePosition.X, _actualWidth, _pointDuration, DelayRefreshInMs));
                _previousPosition = _currentMousePosition;

                Thread.Sleep(DelayRefreshInMs);
            }
        }

        private void BuildTimeBandData(DateTime newTime)
        {
            if ((newTime == DateTime.MinValue) || (newTime == DateTime.MaxValue))
                return;

            if (double.IsNaN(ActualWidth))
                return;

            GetTimeLineBounds(newTime, out _startTimeRange, out _endTimeRange);
            var items = new List<TimeBandItem>();

            // Compute the first visible item
            var itemTime =
                _startTimeRange.AddTicks(-_startTimeRange.Ticks%(10000000*DefaultTimeBandItemDurationInSeconds))
                    .AddSeconds(DefaultTimeBandItemDurationInSeconds); //TimeBandItem duration round            

            for (; itemTime < _endTimeRange; itemTime = itemTime.AddSeconds(DefaultTimeBandItemDurationInSeconds))
            {
                var leftOffset = itemTime.Subtract(_startTimeRange).TotalSeconds*DefaultTimeBandItemWidth/
                                 DefaultTimeBandItemDurationInSeconds;

                var item = new TimeBandItem
                {
                    DateTime = itemTime,
                    Left = leftOffset
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
            var mediumTimeRange = ActualWidth*DefaultTimeBandItemDurationInSeconds/(DefaultTimeBandItemWidth*2);
            startTimeRange = newTime.AddSeconds(-mediumTimeRange);
            endTimeRange = newTime.AddSeconds(mediumTimeRange);
        }

        private void ShiftTimeBand(double timeShiftInSeconds)
        {
            _internalSet = true;
            _currentTime = _currentTime.AddSeconds(timeShiftInSeconds);
            Dispatcher.Invoke(() => { CurrentTime = _currentTime; }
            );
            _internalSet = false;

            if (_timeBandItems.Count == 0)
                return;


            var visualOffset = -(timeShiftInSeconds*DefaultTimeBandItemWidth/DefaultTimeBandItemDurationInSeconds);
            if ((_timeBandItems[0].Left + visualOffset < 0) || (_timeBandItems[_timeBandItems.Count - 1].Left + visualOffset > ActualWidth))
                BuildTimeBandData(_currentTime);
            else
                foreach (var item in _timeBandItems)
                    item.Left += visualOffset;
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


        private void CnDTimeLine_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timelineMoving)
                StopMoveTask();
            RemoveHandler(MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            RemoveHandler(MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }

        private void CnDTimeLine_Loaded(object sender, RoutedEventArgs e)
        {
            _actualWidth = ActualWidth;

            EventManager.RegisterClassHandler(typeof(UIElement), MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            EventManager.RegisterClassHandler(typeof(UIElement), MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            if (_timelineMoving)
                StopMoveTask();
            _timelineMoving = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (_timelineMoving && (args.LeftButton == MouseButtonState.Pressed))
                _currentMousePosition = args.GetPosition(this);
        }

        #endregion
    }

    public enum CndTimeLineBehaviorType
    {
        Speed,
        Draging,
    }

    internal interface ICnDTimeLineBehavior
    {
        double GetTimeShifting(double previousPosition, double currentPosition, double controlWidth, double pointDuration, int delayRefreshInMs);
    }

    internal class CnDTimeLineSpeedBehavior : ICnDTimeLineBehavior
    {
        public double GetTimeShifting(double previousPosition, double currentPosition, double controlWidth, double pointDuration, int delayRefreshInMs)
        {
            return GetRatio(currentPosition, controlWidth) *delayRefreshInMs/1000;
        }

        private double GetRatio(double position, double width)
        {
            double ratio = 0;

            var middle = width/2;
            if (position < middle)
            {
                // Move backward
                ratio = position/middle;

                if (ratio >= 0.75)
                    ratio = -Math.Abs(ratio - 1);
                else
                    ratio = -(Math.Abs(ratio - 0.75)*19/0.75 + 1);
            }
            else if (position > middle)
            {
                // Move forward
                ratio = (position - middle)/middle;

                if (ratio <= 0.25)
                    ratio = ratio*4; // equals to ratio / 0.25
                else
                    ratio = (ratio - 0.25)*19 + 1;
            }

            return ratio;
        }
    }

    internal class CnDTimeLineDragBehavior : ICnDTimeLineBehavior
    {
        public double GetTimeShifting(double previousPosition, double currentPosition, double controlWidth, double pointDuration, int delayRefreshInMs)
        {
            return -(currentPosition - previousPosition) * pointDuration;
        }
    }


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