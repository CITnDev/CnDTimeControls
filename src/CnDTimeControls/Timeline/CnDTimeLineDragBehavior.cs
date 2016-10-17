namespace CnDTimeControls.Timeline
{
    internal class CnDTimeLineDragBehavior : ICnDTimeLineBehavior
    {
        public double GetTimeShifting(double previousPosition, double currentPosition, double controlWidth, double pointDuration, int delayRefreshInMs)
        {
            return -(currentPosition - previousPosition) * pointDuration;
        }
    }
}