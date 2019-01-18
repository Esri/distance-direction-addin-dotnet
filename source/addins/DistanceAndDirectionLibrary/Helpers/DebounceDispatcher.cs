using System;
using System.Windows.Threading;

namespace DistanceAndDirectionLibrary.Helpers
{
    /// <summary>
    /// Provides Debounce() and ThrottleAndFireAtInterval() methods.
    /// Use these methods to ensure that events aren't handled too frequently.
    /// 
    /// ThrottleAndFireAtInterval() ensures that events are throttled by the interval specified.
    /// Only the last event in the interval sequence of events fires.
    /// 
    /// Debounce() fires an event only after the specified interval has passed
    /// in which no other pending event has fired. Only the last event in the
    /// sequence is fired.
    /// </summary>
    public class DebounceDispatcher
    {
        private DispatcherTimer timer;

        private DateTime _timerStarted = DateTime.UtcNow.AddYears(-1);
        private DateTime timerStarted { get { return timerStarted; } set { _timerStarted = value; } }

        /// <summary>
        /// Debounce an event by resetting the event timeout every time the event is 
        /// fired. The behavior is that the Action passed is fired only after events
        /// stop firing for the given timeout period.
        /// 
        /// Use Debounce when you want events to fire only after events stop firing
        /// after the given interval timeout period.
        /// 
        /// </summary>
        /// <param name="interval">Timeout in Milliseconds</param>
        /// <param name="action">Action<object> to fire when debounced event fires</object></param>
        /// <param name="param">optional parameter</param>
        /// <param name="priority">optional priorty for the dispatcher</param>
        /// <param name="disp">optional dispatcher. If not passed or null CurrentDispatcher is used.</param>        
        public void Debounce(int interval, Action<object> action,
            object param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher disp = null)
        {
            // kill pending timer and pending ticks
            timer.Stop();
            timer = null;

            if (disp == null)
                disp = Dispatcher.CurrentDispatcher;

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (timer == null)
                    return;

                timer.Stop();
                timer = null;
                action.Invoke(param);
            }, disp);

            timer.Start();
        }

        /// <summary>
        /// This method throttles events by allowing only 1 event to fire for each
        /// timeout period.
        /// 
        /// Use ThrottleAndFireAtInterval where you need to ensure that events fire at given intervals.
        /// </summary>
        /// <param name="interval">Timeout in Milliseconds</param>
        /// <param name="action">Action<object> to fire when debounced event fires</object></param>
        /// <param name="param">optional parameter</param>
        /// <param name="priority">optional priorty for the dispatcher</param>
        /// <param name="disp">optional dispatcher. If not passed or null CurrentDispatcher is used.</param>
        public void ThrottleAndFireAtInterval(int interval, Action<object> action,
            object param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher disp = null)
        {
            // kill pending timer and pending ticks
            timer.Stop();
            timer = null;

            if (disp == null)
                disp = Dispatcher.CurrentDispatcher;

            var curTime = DateTime.UtcNow;

            // if timeout is not up yet - adjust timeout to fire 
            // with potentially new Action parameters           

            if (curTime.Subtract(timerStarted).TotalMilliseconds >= interval)
            {
                timerStarted = curTime;
                action.Invoke(param);
                return;
            }

            interval -= (int)curTime.Subtract(timerStarted).TotalMilliseconds;

            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (timer == null)
                    return;

                timer.Stop();
                timer = null;
                action.Invoke(param);
                timerStarted = curTime;
            }, disp);

            timer.Start();
        }
    }
}
