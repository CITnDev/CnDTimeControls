﻿using System;
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

        [Test]
        public void InAmbiguousTimeLocalTimesAreEqual()
        {
            var utcJustBeforeDST = new DateTime(2015, 10, 25, 0, 30, 0, DateTimeKind.Utc);
            var utcJustAfterDST = new DateTime(2015, 10, 25, 1, 30, 0, DateTimeKind.Utc);


            var localJustBeforeDST = TimeZoneInfo.ConvertTimeFromUtc(utcJustBeforeDST, _tzi);
            var localJustAfterDST = TimeZoneInfo.ConvertTimeFromUtc(utcJustAfterDST, _tzi);
            
            Assert.That(localJustBeforeDST == localJustAfterDST, Is.EqualTo(true));
            Assert.That(localJustBeforeDST.ToUniversalTime() == localJustAfterDST.ToUniversalTime(), Is.EqualTo(false));
        }


        [Test]
        public void France_DST2015EndLocalTimeAreEqualsBeforeAndAfterDST()
        {
            var utcTimeBeforeDST = new DateTime(2015, 10, 25, 1, 30, 0, DateTimeKind.Utc);
            var utcTimeAfterDST = new DateTime(2015, 10, 25, 1, 30, 0, DateTimeKind.Utc);
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");


            var localTimeBeforeDST = TimeZoneInfo.ConvertTimeFromUtc(utcTimeBeforeDST, tzi);
            var localTimeAfterDST = TimeZoneInfo.ConvertTimeFromUtc(utcTimeAfterDST, tzi);

            Assert.That(tzi.IsDaylightSavingTime(localTimeBeforeDST), Is.EqualTo(true));
            Assert.That(tzi.IsDaylightSavingTime(localTimeAfterDST), Is.EqualTo(true));

            Assert.That(localTimeBeforeDST == localTimeAfterDST, Is.EqualTo(true));
        }
    }
}
