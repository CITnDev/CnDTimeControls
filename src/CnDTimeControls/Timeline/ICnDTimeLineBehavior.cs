namespace CnDTimeControls.Timeline
{
    internal interface ICnDTimeLineBehavior
    {
        double GetTimeShifting(double previousPosition, double currentPosition, double controlWidth, double pointDuration, int delayRefreshInMs);
    }
}