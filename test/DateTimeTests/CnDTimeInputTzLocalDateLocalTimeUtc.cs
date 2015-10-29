using System;
using CnDTimeControls;
using NUnit.Framework;

namespace DateTimeTests
{

    [TestFixture]
    public class CnDTimeInputTzLocalDateLocalTimeUtc
    {
        [Test, RequiresSTA]
        public void BeforeDST()
        {
            var ctrl = new CnDTimeInput();
            ctrl.CurrentDate = new DateTime(2015, 10, 24, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 26, 3, 0, 0, DateTimeKind.Utc);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 24, 8, 0, 0, DateTimeKind.Utc)));
        }

        [Test, RequiresSTA]
        public void AfterDST()
        {
            var ctrl = new CnDTimeInput();
            ctrl.CurrentDate = new DateTime(2015, 10, 26, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Utc);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 26, 9, 0, 0, DateTimeKind.Utc)));
        }

        [Test, RequiresSTA]
        public void AfterDSTDay()
        {
            var ctrl = new CnDTimeInput();
            ctrl.CurrentDate = new DateTime(2015, 10, 25, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 3, 0, 0, DateTimeKind.Utc);
            ctrl.OnTimeChanged(TimeSpan.FromHours(10));

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(new DateTime(2015, 10, 25, 9, 0, 0, DateTimeKind.Local)));
        }

        [Test, RequiresSTA]
        public void DSTSummer()
        {
            var ctrl = new CnDTimeInput();
            ctrl.CurrentDate = new DateTime(2015, 10, 25, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 24, 0, 0, 0, DateTimeKind.Utc);
            ctrl.IsSummerPeriod = true;
            ctrl.OnTimeChanged(TimeSpan.FromMinutes(150)); // Add 2h30

            var expected = new DateTime(2015, 10, 25, 0, 30, 0, DateTimeKind.Utc);

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(expected));
        }

        [Test, RequiresSTA]
        public void DSTWinter()
        {
            var ctrl = new CnDTimeInput();
            ctrl.CurrentDate = new DateTime(2015, 10, 25, 0, 0, 0, DateTimeKind.Local);
            ctrl.SelectedDateTime = new DateTime(2015, 10, 25, 4, 0, 0, DateTimeKind.Utc);
            ctrl.IsSummerPeriod = false;
            ctrl.OnTimeChanged(TimeSpan.FromMinutes(150)); // Add 2h30

            var expected = new DateTime(2015, 10, 25, 1, 30, 0, DateTimeKind.Utc);

            Assert.That(ctrl.SelectedDateTime, Is.EqualTo(expected));
        }
    }
}
