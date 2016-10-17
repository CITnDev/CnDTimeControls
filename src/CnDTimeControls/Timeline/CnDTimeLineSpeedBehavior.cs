using System;

namespace CnDTimeControls.Timeline
{
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
}