namespace CnDTimeControls.Timeline
{
    internal class CnDTimeLineDragBehavior : CnDTimeLineBehaviorBase
    {
        public CnDTimeLineDragBehavior(CnDTimeLine timeLine) : base(timeLine)
        {
        }

        public override double GetShifting()
        {
            var shift = -(CurrentMousePosition.X - PreviousPosition.X);
            PreviousPosition = CurrentMousePosition;
            return shift;
        }
    }
}