using System;
using CnDTimeControls;
using NUnit.Framework;

namespace DateTimeTests
{
    [TestFixture]
    public class CnDTimeInputOffsetTests
    {
        public class CnDTimeInputOffset : CnDTimeInput
        {
            protected override DateTime ComputeNewDateTime(TimeSpan value)
            {
                if (value.TotalSeconds < 10800) //Test with 3h
                    return base.ComputeNewDateTime(value).AddDays(1);

                return base.ComputeNewDateTime(value);
            }
        }

        [Test, RequiresSTA]
        public void BeforeDST()
        {
            var ctrl = new CnDTimeInputOffset();
            ctrl.CurrentDate = new DateTime(2015, 10, 24, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 24, 10, 0, 0, DateTimeKind.Local)));
        }

        [Test, RequiresSTA]
        public void AfterDST()
        {
            var ctrl = new CnDTimeInputOffset();
            ctrl.CurrentDate = new DateTime(2015, 10, 26, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 26, 10, 0, 0, DateTimeKind.Local)));
        }

        [Test, RequiresSTA]
        public void AfterDSTDay()
        {
            var ctrl = new CnDTimeInputOffset();
            ctrl.CurrentDate = new DateTime(2015, 10, 25, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Local);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 25, 10, 0, 0, DateTimeKind.Local)));
        }

        [Test, RequiresSTA]
        public void DSTSummer()
        {
            var ctrl = new CnDTimeInputOffset();
            ctrl.CurrentDate = new DateTime(2015, 10, 24, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Local);
            ctrl.IsSummerPeriod = true;
            ctrl.OnTimeChanged(TimeSpan.FromMinutes(150)); // Add 2h30

            var expected = new DateTime(2015, 10, 25, 0, 30, 0, DateTimeKind.Utc);

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(expected.ToLocalTime()));
        }

        [Test, RequiresSTA]
        public void DSTWinter()
        {
            var ctrl = new CnDTimeInputOffset();
            ctrl.CurrentDate = new DateTime(2015, 10, 24, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 25, 4, 0, 0, DateTimeKind.Local);
            ctrl.IsSummerPeriod = false;
            ctrl.OnTimeChanged(TimeSpan.FromMinutes(150)); // Add 2h30

            var expected = new DateTime(2015, 10, 25, 1, 30, 0, DateTimeKind.Utc);

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(expected.ToLocalTime()));
        }
    }
}
