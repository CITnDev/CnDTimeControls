using System;
using System.CodeDom;
using System.ComponentModel;
using CnDTimeControls;
using NUnit.Framework;

namespace DateTimeTests
{
    [TestFixture]
    public class CnDTimeInputOffsetTests
    {
        private volatile bool _isSummerPeriodByPropertyChanged;

        public class CnDTimeInputOffset3H : CnDTimeInput
        {
            public event Action<bool> SummerPeriodChanged;

            public CnDTimeInputOffset3H()
            {
                DependencyPropertyDescriptor.FromProperty(CnDTimeInput.IsSummerPeriodProperty, typeof(CnDTimeInput)).AddValueChanged(this, (sender, args) => OnSummerPeriodChanged());
            }

            private void OnSummerPeriodChanged()
            {
                if (SummerPeriodChanged != null)
                    SummerPeriodChanged(IsSummerPeriod);
            }

            protected override DateTime ComputeNewDateTime(TimeSpan value)
            {
                if (value.TotalSeconds < 10800) //Test with 3h
                    return base.ComputeNewDateTime(value).AddDays(1);

                return base.ComputeNewDateTime(value);
            }

            protected override DateTime ComputeNewDate(DateTime value)
            {
                if (value.Kind != DateTimeKind.Utc && value.Kind != DateTimeKind.Local)
                    throw new NotImplementedException();

                if (value.ToLocalTime().TimeOfDay.TotalSeconds < 10800)
                    return base.ComputeNewDate(value).AddDays(-1);
                
                return base.ComputeNewDate(value);
            }
        }

        [Test, RequiresSTA]
        public void BeforeDSTCheckIsSummerPeriod()
        {
            var ctrl = new CnDTimeInputOffset3H();
            ctrl.SummerPeriodChanged += b =>
            {
                _isSummerPeriodByPropertyChanged = b;
            };
            _isSummerPeriodByPropertyChanged = ctrl.IsSummerPeriod;
            Assert.That(ctrl.IsSummerPeriod, Is.EqualTo(false), "IsSummerPeriod is true.");
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 10, 0, 0, DateTimeKind.Local);

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 24, 10, 0, 0, DateTimeKind.Local)), "SelectedDateTime is bad.");
            Assert.That(ctrl.IsSummerPeriod, Is.EqualTo(true), "IsSummerPeriod is false.");
            Assert.That(_isSummerPeriodByPropertyChanged, Is.EqualTo(true), "_isSummerPeriodByPropertyChanged is false.");
        }

        [Test, RequiresSTA]
        public void BeforeDST()
        {
            var ctrl = new CnDTimeInputOffset3H();
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 24, 10, 0, 0, DateTimeKind.Local)));
        }

        [Test, RequiresSTA]
        public void AfterDST()
        {
            var ctrl = new CnDTimeInputOffset3H();
            ctrl.SelectedDateTime = new DateTime(2015, 10, 26, 1, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 25, 10, 0, 0, DateTimeKind.Local)));
        }

        [Test, RequiresSTA]
        public void AfterDSTDay()
        {
            var ctrl = new CnDTimeInputOffset3H();
            ctrl.SelectedDateTime = new DateTime(2015, 10, 25, 5, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 25, 10, 0, 0, DateTimeKind.Local)));
        }

        [Test, RequiresSTA]
        public void BeforeDSTDay()
        {
            var ctrl = new CnDTimeInputOffset3H();
            ctrl.SelectedDateTime = new DateTime(2015, 10, 25, 1, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 24, 10, 0, 0, DateTimeKind.Local)));
        }

        [Test, RequiresSTA]
        public void DSTSummer()
        {
            var ctrl = new CnDTimeInputOffset3H();
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromMinutes(150)); // Add 2h30
            ctrl.IsSummerPeriod = true;

            var expected = new DateTime(2015, 10, 25, 0, 30, 0, DateTimeKind.Utc);

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(expected.ToLocalTime()));
        }

        [Test, RequiresSTA]
        public void DSTWinter()
        {
            var ctrl = new CnDTimeInputOffset3H();
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromMinutes(140)); // Add 2h20
            ctrl.IsSummerPeriod = false;

            var expected = new DateTime(2015, 10, 25, 1, 20, 0, DateTimeKind.Utc);

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(expected.ToLocalTime()));
        }
    }
}
