using System;
using CnDTimeControls;
using NUnit.Framework;

namespace DateTimeTests
{
    [TestFixture]
    public class DaylightSavingTests
    {
        private TimeZoneInfo _tzi;

        [SetUp]
        public void Setup()
        {
            _tzi = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        }

        [Test]
        public void IsDaylightSavingTime2015InSummerIsTrue()
        {
            var utcSummerTime = new DateTime(2015, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var utcJustBeforeAmbiguousTime = new DateTime(2015, 10, 24, 23, 59, 59, 999, DateTimeKind.Utc);


            var localSummerTime = TimeZoneInfo.ConvertTimeFromUtc(utcSummerTime, _tzi);
            var localTimeAfterAmbiguousTime = TimeZoneInfo.ConvertTimeFromUtc(utcJustBeforeAmbiguousTime, _tzi);

            Assert.That(_tzi.IsDaylightSavingTime(localSummerTime), Is.EqualTo(true));
            Assert.That(_tzi.IsDaylightSavingTime(localTimeAfterAmbiguousTime), Is.EqualTo(true));
        }

        [Test]
        public void IsDaylightSavingTime2015InWinterIsFalse()
        {
            var utcWinterTime = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var utcJustAfterAmbiguousTime = new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc);


            var localWinterTime = TimeZoneInfo.ConvertTimeFromUtc(utcWinterTime, _tzi);
            var localTimeAfterAmbiguousTime = TimeZoneInfo.ConvertTimeFromUtc(utcJustAfterAmbiguousTime, _tzi);

            Assert.That(_tzi.IsDaylightSavingTime(localWinterTime), Is.EqualTo(false));
            Assert.That(_tzi.IsDaylightSavingTime(localTimeAfterAmbiguousTime), Is.EqualTo(false));
        }

        [Test]
        public void IsDaylightSavingTime2015AmbiguousTime()
        {
            var utcJustBeforeAmbiguousTime = new DateTime(2015, 10, 24, 23, 59, 59, 999, DateTimeKind.Utc);
            var utcJustAfterAmbiguousTime = new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc);
            var utcAmbiguousTimeStart = new DateTime(2015, 10, 25, 1, 0, 0, DateTimeKind.Utc);
            var utcAmbiguousTimeEnd = new DateTime(2015, 10, 25, 1, 0, 0, DateTimeKind.Utc);


            var localJustBeforeAmbiguousTime = TimeZoneInfo.ConvertTimeFromUtc(utcJustBeforeAmbiguousTime, _tzi);
            var localJustAfterAmbiguousTime = TimeZoneInfo.ConvertTimeFromUtc(utcJustAfterAmbiguousTime, _tzi);
            var localAmbiguousTimeStart = TimeZoneInfo.ConvertTimeFromUtc(utcAmbiguousTimeStart, _tzi);
            var localAmbiguousTimeEnd = TimeZoneInfo.ConvertTimeFromUtc(utcAmbiguousTimeEnd, _tzi);

            Assert.That(_tzi.IsDaylightSavingTime(localJustBeforeAmbiguousTime), Is.EqualTo(true));
            Assert.That(_tzi.IsAmbiguousTime(localJustBeforeAmbiguousTime), Is.EqualTo(false));


            Assert.That(_tzi.IsDaylightSavingTime(localJustAfterAmbiguousTime), Is.EqualTo(false));
            Assert.That(_tzi.IsAmbiguousTime(localJustAfterAmbiguousTime), Is.EqualTo(false));

            Assert.That(_tzi.IsDaylightSavingTime(localAmbiguousTimeStart), Is.EqualTo(false));
            Assert.That(_tzi.IsAmbiguousTime(localAmbiguousTimeStart), Is.EqualTo(true));

            Assert.That(_tzi.IsDaylightSavingTime(localAmbiguousTimeEnd), Is.EqualTo(false));
            Assert.That(_tzi.IsAmbiguousTime(localAmbiguousTimeEnd), Is.EqualTo(true));
        }


        [Test]
        public void IsDaylightSavingTime2014AmbiguousTime()
        {
            var utcJustBeforeAmbiguousTime = new DateTime(2014, 10, 25, 23, 59, 59, 999, DateTimeKind.Utc);
            var utcJustAfterAmbiguousTime = new DateTime(2014, 10, 26, 2, 0, 0, DateTimeKind.Utc);
            var utcAmbiguousTimeStart = new DateTime(2014, 10, 26, 1, 0, 0, DateTimeKind.Utc);
            var utcAmbiguousTimeEnd = new DateTime(2014, 10, 26, 1, 0, 0, DateTimeKind.Utc);


            var localJustBeforeAmbiguousTime = TimeZoneInfo.ConvertTimeFromUtc(utcJustBeforeAmbiguousTime, _tzi);
            var localJustAfterAmbiguousTime = TimeZoneInfo.ConvertTimeFromUtc(utcJustAfterAmbiguousTime, _tzi);
            var localAmbiguousTimeStart = TimeZoneInfo.ConvertTimeFromUtc(utcAmbiguousTimeStart, _tzi);
            var localAmbiguousTimeEnd = TimeZoneInfo.ConvertTimeFromUtc(utcAmbiguousTimeEnd, _tzi);

            Assert.That(_tzi.IsDaylightSavingTime(localJustBeforeAmbiguousTime), Is.EqualTo(true));
            Assert.That(_tzi.IsAmbiguousTime(localJustBeforeAmbiguousTime), Is.EqualTo(false));


            Assert.That(_tzi.IsDaylightSavingTime(localJustAfterAmbiguousTime), Is.EqualTo(false));
            Assert.That(_tzi.IsAmbiguousTime(localJustAfterAmbiguousTime), Is.EqualTo(false));

            Assert.That(_tzi.IsDaylightSavingTime(localAmbiguousTimeStart), Is.EqualTo(false));
            Assert.That(_tzi.IsAmbiguousTime(localAmbiguousTimeStart), Is.EqualTo(true));

            Assert.That(_tzi.IsDaylightSavingTime(localAmbiguousTimeEnd), Is.EqualTo(false));
            Assert.That(_tzi.IsAmbiguousTime(localAmbiguousTimeEnd), Is.EqualTo(true));
        }

        [Test, RequiresSTA]
        public void ChangeDSTFromSummerToWinterWithAddOneHour()
        {
            var dateTime = new DateTime(2015, 10, 25, 0, 30, 0, DateTimeKind.Utc);
            var timeInput = new CnDTimeInput();
            timeInput.SelectedDateTime = dateTime.ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(true));
            timeInput.SelectedDateTime = dateTime.AddHours(1).ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(false));
        }


        [Test, RequiresSTA]
        public void ChangeDSTFromWinterToSummerWithRemoveOneHour()
        {
            var dateTime = new DateTime(2015, 10, 25, 1, 30, 0, DateTimeKind.Utc);
            var timeInput = new CnDTimeInput();
            timeInput.SelectedDateTime = dateTime.ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(false));
            timeInput.SelectedDateTime = dateTime.AddHours(-1).ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(true));
        }

        [Test, RequiresSTA]
        public void ChangeDSTFromWinterToSummerToWinter()
        {
            var dateTime = new DateTime(2015, 10, 25, 1, 30, 0, DateTimeKind.Utc);
            var timeInput = new CnDTimeInput();
            timeInput.SelectedDateTime = dateTime.ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(false));
            timeInput.SelectedDateTime = dateTime.AddHours(-1).ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(true));
            timeInput.SelectedDateTime = dateTime.ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(false));
        }

        [Test, RequiresSTA]
        public void ChangeDSTFromWinterToSummerWithDaylightChange()
        {
            var dateTime = new DateTime(2015, 10, 25, 1, 30, 0, DateTimeKind.Utc);
            var timeInput = new CnDTimeInput();
            timeInput.SelectedDateTime = dateTime.ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(false));
            timeInput.IsSummerPeriod = true;
            Assert.That(timeInput.SelectedDateTime, Is.EqualTo(dateTime.AddHours(-1).ToLocalTime()));
        }

        [Test, RequiresSTA]
        public void ChangeDSTFromSummerToWinterWithDaylightChange()
        {
            var dateTime = new DateTime(2015, 10, 25, 0, 30, 0, DateTimeKind.Utc);
            var timeInput = new CnDTimeInput();
            timeInput.SelectedDateTime = dateTime.ToLocalTime();
            Assert.That(timeInput.IsSummerPeriod, Is.EqualTo(true));
            timeInput.IsSummerPeriod = false;
            Assert.That(timeInput.SelectedDateTime, Is.EqualTo(dateTime.AddHours(1).ToLocalTime()));
        }
    }
}
