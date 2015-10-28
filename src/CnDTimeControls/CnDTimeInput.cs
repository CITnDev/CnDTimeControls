using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
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
        private volatile DateTimeKind _lastSetDateTimeKind = DateTimeKind.Utc;

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

        internal void OnTimeChanged(TimeSpan value)
        {
            DateTime dateTime;


            if (TimeZone != null && Equals(TimeZone, TimeZoneInfo.Local) && _lastSetDateTimeKind == DateTimeKind.Local)
                dateTime = ComputeDateTimeTimeZoneLocalDateLocal(value);
            else if (TimeZone != null && Equals(TimeZone, TimeZoneInfo.Local) && _lastSetDateTimeKind == DateTimeKind.Utc)
                dateTime = ComputeDateTimeTimeZoneLocalDateUtc(value);
            else
            {
                throw new NotImplementedException("Only a TimeZone set to Local works for the moment.");
            }
            _internalSet = true;
            SelectedDateTime = dateTime;
            if (TimeZone != null && Equals(TimeZone, TimeZoneInfo.Local))
            {
                if (dateTime.Kind == DateTimeKind.Utc)
                    IsSummerPeriod = TimeZone.IsDaylightSavingTime(dateTime.ToLocalTime());
                else
                    IsSummerPeriod = TimeZone.IsDaylightSavingTime(dateTime);
            }
            _internalSet = false;
        }

        private DateTime ComputeDateTimeTimeZoneLocalDateLocal(TimeSpan value)
        {
            if (CurrentDate.Kind != DateTimeKind.Local)
                throw new Exception("Invalid CurrentDate DateTimeKind : must be Local");

            var baseUtcOffset = TimeZone.BaseUtcOffset;

            var dateTime = ComputeNewDateTime(value);
            if (TimeZone.SupportsDaylightSavingTime && TimeZone.IsAmbiguousTime(dateTime))
            {
                if (IsSummerPeriod)
                    return new DateTime(dateTime.AddTicks(-baseUtcOffset.Ticks).AddTicks(-TimeZone.GetAdjustmentRules()[0].DaylightDelta.Ticks).Ticks, DateTimeKind.Utc).ToLocalTime();

                return new DateTime(dateTime.AddTicks(-baseUtcOffset.Ticks).Ticks, DateTimeKind.Utc).ToLocalTime();
            }

            return dateTime;
        }

        private DateTime ComputeDateTimeTimeZoneLocalDateUtc(TimeSpan value)
        {
            if (CurrentDate.Kind != DateTimeKind.Local)
                throw new Exception("Invalid CurrentDate DateTimeKind : must be Local");

            var baseUtcOffset = TimeZone.BaseUtcOffset;

            var dateTime = ComputeNewDateTime(value);
            if (TimeZone.SupportsDaylightSavingTime)
            {

                if (TimeZone.IsAmbiguousTime(dateTime))
                {
                    if (IsSummerPeriod)
                        return new DateTime(dateTime.AddTicks(-baseUtcOffset.Ticks).AddTicks(-TimeZone.GetAdjustmentRules()[0].DaylightDelta.Ticks).Ticks, DateTimeKind.Utc);

                    return new DateTime(dateTime.AddTicks(-baseUtcOffset.Ticks).Ticks, DateTimeKind.Utc);
                }

                IsSummerPeriod = TimeZone.IsDaylightSavingTime(dateTime);
            }

            return dateTime.ToUniversalTime();
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

                if (ctrl.TimeZone != null && Equals(ctrl.TimeZone, TimeZoneInfo.Local) && newValue.Kind == DateTimeKind.Utc)
                    ctrl.IsSummerPeriod = ctrl.TimeZone.IsDaylightSavingTime(newValue.ToLocalTime());
                if (ctrl.TimeZone != null && Equals(ctrl.TimeZone, TimeZoneInfo.Local) && newValue.Kind == DateTimeKind.Local)
                    ctrl.IsSummerPeriod = ctrl.TimeZone.IsDaylightSavingTime(newValue);
            }

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

        #region IsSummerPeriod dependency property

        public static DependencyProperty IsSummerPeriodProperty = DependencyProperty.Register("IsSummerPeriod", typeof(bool), typeof(CnDTimeInput), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSummerPeriodChanged, OnCoerceIsSummerPeriod));

        private static object OnCoerceIsSummerPeriod(DependencyObject d, object basevalue)
        {
            var ctrl = (CnDTimeInput)d;
            if ((ctrl.SelectedDateTime == DateTime.MinValue || ctrl.SelectedDateTime == DateTime.MaxValue) && !ctrl._internalSet)
                return ctrl.IsSummerPeriod;

            return basevalue;
        }

        private static void OnIsSummerPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CnDTimeInput)d;
            if (ctrl._partTime != null && !ctrl._internalSet)
                ctrl.OnTimeChanged(ctrl._partTime.SelectedTime);
        }

        [Description("IsSummerPeriod")]
        [Category("CnDTimeInput Category")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsSummerPeriod
        {
            get
            {
                return ((bool)(GetValue(IsSummerPeriodProperty)));
            }
            set
            {
                SetValue(IsSummerPeriodProperty, value);
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

    internal static class DateTimeExtension
    {
        public static DateTime ToSpecificUniversalTime(this DateTime value, bool isSummerPeriod)
        {
            if (value.Kind == DateTimeKind.Utc)
                throw new Exception("Value must be of local or unspecified kind");

            if (!TimeZoneInfo.Local.SupportsDaylightSavingTime)
                return value.ToUniversalTime();

            var baseUtcOffset = TimeZoneInfo.Local.BaseUtcOffset;

            var dateTime = new DateTime(value.AddTicks(-baseUtcOffset.Ticks).Ticks, DateTimeKind.Utc);

            if (isSummerPeriod)
            {
                var dayLightDelta = TimeZoneInfo.Local.GetAdjustmentRules()[0].DaylightDelta;

                dateTime = new DateTime(dateTime.AddTicks(-dayLightDelta.Ticks).Ticks, DateTimeKind.Utc);
            }

            return dateTime;
        }
    }
}
