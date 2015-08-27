using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CnDTimeControls.Properties;

namespace CnDTimeControls
{
    [TemplatePart(Name = "PART_PreviousDay", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NextDay", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Date", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_Time", Type = typeof(CnDTimeInputTextBox))]
    public class CnDTimeInput : Control, INotifyPropertyChanged
    {
        private CnDTimeInputTextBox _partTime;
        private volatile bool _internalSet;
        private volatile DateTimeKind _lastSetDateTimeKind;

        static CnDTimeInput()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CnDTimeInput), new FrameworkPropertyMetadata(typeof(CnDTimeInput)));
        }

        #region Override methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _partTime = Template.FindName("PART_Time", this) as CnDTimeInputTextBox;
            if (_partTime == null)
                throw new Exception("PART_Time is missing");
            _partTime.AddExceptionKey(Key.Add);
            _partTime.AddExceptionKey(Key.Subtract);
            _partTime.TimeChanged += OnTimeChanged;

            var partPreviousDay = Template.FindName("PART_PreviousDay", this) as Button;
            if (partPreviousDay != null)
                partPreviousDay.Click += (s, e) => SelectedDateTime = SelectedDateTime.AddDays(-1);

            var partNextDay = Template.FindName("PART_NextDay", this) as Button;
            if (partNextDay != null)
                partNextDay.Click += (s, e) => SelectedDateTime = SelectedDateTime.AddDays(1);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _partTime.Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Add)
            {
                CurrentDate = CurrentDate.AddDays(1);
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract)
            {
                CurrentDate = CurrentDate.AddDays(-1);
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        #endregion

        private void OnTimeChanged(TimeSpan value)
        {
            DateTime dateTime;

            // If there is a timeZone and the SelectedDateTime was in Local kind, set SelectedDateTime with a local convertion
            if (TimeZone != null && Equals(TimeZone, TimeZoneInfo.Local))
            {
                var dayLightDelta = TimeZoneInfo.Local.GetAdjustmentRules()[0].DaylightDelta;
                dateTime = ComputeNewDateTime(value);
                if (_lastSetDateTimeKind == DateTimeKind.Local)
                {
                    if (TimeZone.SupportsDaylightSavingTime && TimeZone.IsAmbiguousTime(dateTime))
                    {
                        if (IsDaylight)
                            dateTime = CurrentDate.ToUniversalTime().Add(value).ToLocalTime();
                        else
                            dateTime = CurrentDate.ToUniversalTime().Add(value).Add(dayLightDelta).ToLocalTime();
                    }
                }
                else
                {
                    if (TimeZone.SupportsDaylightSavingTime && TimeZone.IsAmbiguousTime(dateTime))
                    {
                        if (IsDaylight)
                            dateTime = CurrentDate.ToUniversalTime().Add(value);
                        else
                            dateTime = CurrentDate.ToUniversalTime().Add(value).Add(dayLightDelta);
                    }
                }
            }
            else
            {
                dateTime = CurrentDate.Add(value);
            }

            _internalSet = true;
            SelectedDateTime = dateTime;
            _internalSet = false;
        }

        protected virtual DateTime ComputeNewDateTime(TimeSpan value)
        {
            return CurrentDate.Add(value);
        }

        #region Dependency properties

        #region CurrentDate dependency property

        // Using a DependencyProperty as the backing store for CurrentDate.  This enables animation, styling, binding, etc...
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty CurrentDateProperty =
    DependencyProperty.Register("CurrentDate", typeof(DateTime), typeof(CnDTimeInput), new PropertyMetadata(DateTime.UtcNow.Date));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public DateTime CurrentDate
        {
            get { return (DateTime)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }

        #endregion

        #region SelectedDateTime dependency property

        public static DependencyProperty SelectedDateTimeProperty = DependencyProperty.Register("SelectedDateTime", typeof(DateTime), typeof(CnDTimeInput),
            new FrameworkPropertyMetadata(DateTime.UtcNow, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateTimeChanged, OnCoerceSelectedDateTimeValue));

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CnDTimeInput)d;
            var dateTime = (DateTime)e.NewValue;

            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                ctrl._partTime.MaskProvider.Set(ctrl._partTime.Mask.DefaultValue);
                return;
            }

            if (ctrl._partTime != null && !ctrl._internalSet)
            {
                if (ctrl.TimeZone != null && Equals(ctrl.TimeZone, TimeZoneInfo.Local))
                {
                    DateTime localTime;
                    if (dateTime.Kind == DateTimeKind.Local)
                        localTime = dateTime;
                    else
                        localTime = dateTime.ToLocalTime();

                    ctrl.CurrentDate = localTime.Date;
                    ctrl._partTime.SelectedTime = localTime.TimeOfDay;
                }
                else
                {
                    ctrl.CurrentDate = dateTime.Date;
                    ctrl._partTime.SelectedTime = dateTime.TimeOfDay;
                }
            }
        }

        private static object OnCoerceSelectedDateTimeValue(DependencyObject d, object baseValue)
        {
            if (IsInDesignModeStatic)
                return baseValue;

            var newValue = (DateTime)baseValue;
            var ctrl = (CnDTimeInput)d;
            if (!ctrl._internalSet)
            {

                if (newValue.Kind == DateTimeKind.Unspecified && newValue != DateTime.MinValue && newValue != DateTime.MaxValue)
                    throw new ArgumentException("DateTimeKind must be Local or Utc");

                ctrl._lastSetDateTimeKind = newValue.Kind;
            }

            ctrl._internalSet = true;
            if (ctrl.TimeZone != null && Equals(ctrl.TimeZone, TimeZoneInfo.Local) && newValue.Kind == DateTimeKind.Utc)
                ctrl.IsDaylight = ctrl.TimeZone.IsDaylightSavingTime(newValue.ToLocalTime());
            if (ctrl.TimeZone != null && Equals(ctrl.TimeZone, TimeZoneInfo.Local) && newValue.Kind == DateTimeKind.Local)
                ctrl.IsDaylight = ctrl.TimeZone.IsDaylightSavingTime(newValue);
            ctrl._internalSet = false;

            return baseValue;
        }

        [Description("SelectedDateTime")]
        [Category("CnDTimeInput Category")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public DateTime SelectedDateTime
        {
            get
            {
                return ((DateTime)(GetValue(SelectedDateTimeProperty)));
            }
            set { SetValue(SelectedDateTimeProperty, value); }
        }

        #endregion

        #region TimeZone dependency property

        public static DependencyProperty TimeZoneProperty = DependencyProperty.Register("TimeZone", typeof(TimeZoneInfo), typeof(CnDTimeInput),
            new FrameworkPropertyMetadata(TimeZoneInfo.Local, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [Description("TimeZone")]
        [Category("CnDTimeInput Category")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public TimeZoneInfo TimeZone
        {
            get
            {
                return ((TimeZoneInfo)(GetValue(TimeZoneProperty)));
            }
            set
            {
                SetValue(TimeZoneProperty, value);
            }
        }

        #endregion

        #region DateFontSize dependency property

        public static DependencyProperty DateFontSizeProperty = DependencyProperty.Register("DateFontSizeProperty", typeof(double), typeof(CnDTimeInput), new PropertyMetadata((double)14));

        [Description("DateFontSizeProperty")]
        [Category("CnDTimeInput Category")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double DateFontSize
        {
            get
            {
                return ((double)(GetValue(DateFontSizeProperty)));
            }
            set
            {
                SetValue(DateFontSizeProperty, value);
            }
        }

        #endregion

        #region IsDaylight dependency property

        public static DependencyProperty IsDaylightProperty = DependencyProperty.Register("IsDaylight", typeof(bool), typeof(CnDTimeInput), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDaylightChanged));

        private static void OnIsDaylightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CnDTimeInput)d;
            if (ctrl._partTime != null && !ctrl._internalSet)
                ctrl.OnTimeChanged(ctrl._partTime.SelectedTime);
        }

        [Description("IsDaylight")]
        [Category("CnDTimeInput Category")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsDaylight
        {
            get
            {
                return ((bool)(GetValue(IsDaylightProperty)));
            }
            set
            {
                SetValue(IsDaylightProperty, value);
            }
        }

        #endregion


        #endregion


        #region DesignMode

        private static bool? _isInDesignMode;

        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running in Blend
        /// or Visual Studio).
        /// </summary>
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
#if SILVERLIGHT
            _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                        .FromProperty(prop, typeof(FrameworkElement))
                        .Metadata.DefaultValue;
#endif
                }

                return _isInDesignMode.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running under Blend
        /// or Visual Studio).
        /// </summary>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "Non static member needed for data binding")]
        public bool IsInDesignMode
        {
            get
            {
                return IsInDesignModeStatic;
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
