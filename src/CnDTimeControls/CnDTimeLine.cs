using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CnDTimeControls.Timeline;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace CnDTimeControls
{
    public class CnDTimeLine : Control
    {
        #region Constructors & Load/Unload
        static CnDTimeLine()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CnDTimeLine), new FrameworkPropertyMetadata(typeof(CnDTimeLine)));
        }

        public CnDTimeLine()
        {
            PointDuration = DefaultTimeBandItemDurationInSeconds/DefaultTimeBandItemWidth;
            Loaded += CnDTimeLine_Loaded;
            Unloaded += CnDTimeLine_Unloaded;
        }

        private void CnDTimeLine_Loaded(object sender, RoutedEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(UIElement), MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            if (_cnDTimeLineBehavior == null)
            {
                if (Mode == CndTimeLineBehaviorType.Speed)
                    _cnDTimeLineBehavior = new CnDTimeLineSpeedBehavior(this);
                else
                    _cnDTimeLineBehavior = new CnDTimeLineDragBehavior(this);
            }
        }

        private void CnDTimeLine_Unloaded(object sender, RoutedEventArgs e)
        {
            _cnDTimeLineBehavior?.Dispose();

            if (_timelineMoving)
                StopMoveTask();
        }
        #endregion

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
                if (_cnDTimeLineBehavior != null)
                    _cnDTimeLineBehavior.Dispose();
                if (value == CndTimeLineBehaviorType.Speed)
                    _cnDTimeLineBehavior = new CnDTimeLineSpeedBehavior(this);
                else
                    _cnDTimeLineBehavior = new CnDTimeLineDragBehavior(this);
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

        #region Fields

        internal const int DelayRefreshInMs = 40;
        internal readonly double PointDuration;

        private Task _moveTask;
        private CancellationTokenSource _tokenSource;
        private const double DefaultTimeBandItemWidth = 100;
        private const long DefaultTimeBandItemDurationInSeconds = 1;
        private List<TimeBandItem> _timeBandItems = new List<TimeBandItem>();
        private volatile bool _internalSet;
        private DateTime _currentTime;
        private CnDTimeLineBehaviorBase _cnDTimeLineBehavior;
        private volatile bool _timelineMoving;
        private DateTime _startTimeRange;
        private DateTime _endTimeRange;

        #endregion

        #region Override methods

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _cnDTimeLineBehavior.OnMouseDown();

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

        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            if (_timelineMoving)
                StopMoveTask();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _cnDTimeLineBehavior.ControlWidth = ActualWidth;
            BuildTimeBandData(CurrentTime);
        }

        #endregion

        #region Private methods

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

        private void MoveTimeline()
        {
            _timelineMoving = true;
            while (!_tokenSource.IsCancellationRequested)
            {
                ShiftTimeBand(_cnDTimeLineBehavior.GetShifting() * PointDuration);

                Thread.Sleep(DelayRefreshInMs);
            }
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
            _timelineMoving = false;
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
}