using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CnDTimeControls
{
    public enum CnDTimeInputTextBoxType
    {
        TimeWithMilliseconds,
        TimeWithoutMilliseconds,
        Date,
        DateAndTimeWithoutMilliseconds,
    }

    public class CnDTimeInputTextBox : System.Windows.Controls.TextBox
    {
        private static readonly string[] InputCharacters = {"0"};
        private volatile bool _isSelectionChanging;
        internal readonly CnDTimeInputMask Mask;
        private readonly List<Key> _exceptionKeys = new List<Key>();

        #region WPF Control metadata

        static CnDTimeInputTextBox()
        {
            //override the meta data for the Text Property of the textbox
            var metaData = new FrameworkPropertyMetadata();
            metaData.CoerceValueCallback = CoerceValueCallback;
            TextProperty.OverrideMetadata(typeof(CnDTimeInputTextBox), metaData);
        }//force the text of the control to use the mask
        private static object CoerceValueCallback(DependencyObject sender, object value)
        {
            var textBox = (CnDTimeInputTextBox) sender;
            if (textBox.MaskProvider != null)
                return textBox.MaskProvider.ToDisplayString();
            
            return "No date and time";
        }

        #endregion

        public CnDTimeInputTextBox()
        {
            Mask = new CnDTimeInputMaskTimeWithMilliseconds();
            MaskProvider = new MaskedTextProvider(Mask.Mask);
            MaskProvider.Set(Mask.DefaultValue);
            RefreshText();
        }

        #region Events

        public event Action<TimeSpan> TimeChanged;

        protected virtual void OnTimeChanged(TimeSpan obj)
        {
            Action<TimeSpan> handler = TimeChanged;
            if (handler != null) handler(obj);
        }

        #endregion

        #region Properties

        public MaskedTextProvider MaskProvider { get; private set; }

        #endregion

        #region Public methods

        public void AddExceptionKey(Key key)
        {
            _exceptionKeys.Add(key);
        }

        public void AddRangeExceptionKeys(IEnumerable<Key> keys)
        {
            _exceptionKeys.AddRange(keys);
        }

        #endregion

        #region Overrides methods

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = true;
            var validInput = false;
            if (!string.IsNullOrWhiteSpace(e.Text) && e.Text.Length == 1)
            {
                MaskedTextResultHint hint;
                validInput = MaskProvider.VerifyChar(e.Text[0], SelectionStart, out hint);
                if (validInput)
                {

                    var charArray = MaskProvider.ToDisplayString().ToCharArray();
                    charArray[SelectionStart] = e.Text[0];
                    var verifyString = new string(charArray);

                    TimeSpan ts;
                    if (TimeSpan.TryParse(verifyString, CultureInfo.CurrentCulture, out ts))
                        MaskProvider.Replace(e.Text, SelectionStart);
                    else
                        validInput = false;
                }
            }
            base.OnPreviewTextInput(e);
            if (validInput)
            {
                RefreshText();
                SelectionStart = GetNextCharacterPosition(SelectionStart);
            }
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            if (_isSelectionChanging)
                return;
            _isSelectionChanging = true;

            SelectionLength = 1;

            _isSelectionChanging = false;
            e.Handled = true;
            base.OnSelectionChanged(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                SelectionStart = GetPreviousCharacterPosition(SelectionStart);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                SelectionStart = GetNextCharacterPosition(SelectionStart);
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                MaskProvider.Replace("0", SelectionStart);
                SelectionStart = GetPreviousCharacterPosition(SelectionStart);
                RefreshText();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                MaskProvider.Replace("0", SelectionStart);
                RefreshText();
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                //TODO : Paste
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                Dispatcher.BeginInvoke(new Action<string>(OnTextInputValidating), MaskProvider.ToDisplayString());
                e.Handled = true;
            }
            else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                //TODO : Copy
                e.Handled = true;
            }
            else if (!IsValidKey(e.Key))
            {
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        #endregion

        #region Private methods

        private void RefreshText()
        {
            _isSelectionChanging = true;
            var selectionStart = SelectionStart;
            Text = MaskProvider.ToDisplayString();
            SelectionStart = selectionStart;
            SelectionLength = 1;
            _isSelectionChanging = false;
        }

        private bool IsValidKey(Key key)
        {
            if (Mask.Mask.Substring(SelectionStart, 1) == "0")
            {
                if (key == Key.NumPad0 || key == Key.NumPad1 || key == Key.NumPad2 || key == Key.NumPad3 || key == Key.NumPad4 || key == Key.NumPad5 || key == Key.NumPad6 || key == Key.NumPad7 || key == Key.NumPad8 || key == Key.NumPad9)
                    return true;

                if (key == Key.D0 || key == Key.D1 || key == Key.D2 || key == Key.D3 || key == Key.D4 || key == Key.D5 || key == Key.D6 || key == Key.D7 || key == Key.D8 || key == Key.D9)
                    return true;
            }


            if (_exceptionKeys.Contains(key))
                return true;

            return false;
        }

        private int GetNextCharacterPosition(int position)
        {
            var newPosition = position+1;
            while (newPosition < Mask.Mask.Length && !InputCharacters.Any(_ => _ == Mask.Mask.Substring(newPosition, 1)))
                newPosition++;

            if (newPosition >= Mask.Mask.Length)
                newPosition = SelectionStart;

            return newPosition;
        }

        private int GetPreviousCharacterPosition(int position)
        {
            var newPosition = position-1;
            while (newPosition >= 0 && !InputCharacters.Any(_ => _ == Mask.Mask.Substring(newPosition, 1)))
                newPosition--;

            if (newPosition < 0)
                newPosition = 0;

            return newPosition;
        }

        private void OnTextInputValidating(string value) {
            TimeSpan ts;
            if (TimeSpan.TryParse(value, CultureInfo.CurrentCulture, out ts))
            {
                SelectedTime = ts;
                OnTimeChanged(SelectedTime);
            }
        }

        #endregion

        #region SelectedDateTime dependency property

        public static DependencyProperty SelectedTimeProperty = DependencyProperty.Register("SelectedTime", typeof(TimeSpan), typeof(CnDTimeInputTextBox),
            new FrameworkPropertyMetadata(DateTime.Now.TimeOfDay, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateTimeChanged, OnCoerceSelectedDateTimeProperty));

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var tb = d  as CnDTimeInputTextBox;
            //if (tb == null)
            //    return;

            //if (e.NewValue is DateTime)
            //{
            //    var newValue = (DateTime)e.NewValue;
            //    //tb.MaskProvider.Set();
            //}
        }

        private static object OnCoerceSelectedDateTimeProperty(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        [Description("SelectedDateTime")]
        [Category("SelectedDateTime Category")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public TimeSpan SelectedTime
        {
            get
            {
                return ((TimeSpan)(GetValue(SelectedTimeProperty)));
            }
            set
            {
                SetValue(SelectedTimeProperty, value);
                if (MaskProvider != null && Mask != null)
                {
                    MaskProvider.Set(value.ToString(Mask.ProviderValueFormat));
                    RefreshText();
                }
            }
        }
        #endregion
    }

    public abstract class CnDTimeInputMask
    {
        public abstract string Mask { get; }
        public abstract string DefaultValue { get; }
        public abstract string ProviderValueFormat { get; }
    }

    public class CnDTimeInputMaskTimeWithMilliseconds : CnDTimeInputMask
    {
        public override string Mask { get { return "00:00:00.000"; } }
        public override string DefaultValue { get { return "000000000"; } }
        public override string ProviderValueFormat { get { return "hhmmssfff"; } }
    }
}
