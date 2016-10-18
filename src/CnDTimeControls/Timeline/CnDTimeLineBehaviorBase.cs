using System;
using System.Windows;
using System.Windows.Input;

namespace CnDTimeControls.Timeline
{
    internal abstract class CnDTimeLineBehaviorBase : IDisposable
    {
        private volatile bool _isDisposed = false;
        private volatile bool _loadedEventHook = false;
        protected Point PreviousPosition;

        protected readonly CnDTimeLine TimeLine;
        protected Point CurrentMousePosition;
    
        internal double ControlWidth;

        #region Constructors & Destructors

        protected CnDTimeLineBehaviorBase(CnDTimeLine timeLine)
        {
            TimeLine = timeLine;
            if (!TimeLine.IsLoaded)
            {
                _loadedEventHook = true;
                TimeLine.Loaded += CnDTimeLine_Loaded;
            }
            else
                CnDTimeLine_Loaded(TimeLine, null);
            TimeLine.Unloaded += CnDTimeLine_Unloaded;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_isDisposed)
                    return;
                _isDisposed = true;
            }
            if (_loadedEventHook)
                TimeLine.Loaded -= CnDTimeLine_Loaded;
            TimeLine.Unloaded -= CnDTimeLine_Unloaded;
        }
        #endregion


        #region Attach & Detach mouse and keyboard events
        private void CnDTimeLine_Unloaded(object sender, RoutedEventArgs e)
        {
            TimeLine.RemoveHandler(UIElement.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            TimeLine.RemoveHandler(UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }

        private void CnDTimeLine_Loaded(object sender, RoutedEventArgs e)
        {
            ControlWidth = TimeLine.ActualWidth;
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }

        internal void OnMouseDown()
        {
            CurrentMousePosition = Mouse.GetPosition(TimeLine);
            PreviousPosition = CurrentMousePosition;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
                CurrentMousePosition = args.GetPosition(TimeLine);
        }
        #endregion

        public abstract double GetShifting();
    }
}