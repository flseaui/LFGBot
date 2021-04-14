using System;
using System.Timers;

namespace LFGBot.Misc
{
    public class TimerPlus : Timer
    {
        private DateTime _dueTime;

        public TimerPlus() => Elapsed += ElapsedAction;

        protected new void Dispose()
        {
            Elapsed -= ElapsedAction;
            base.Dispose();
        }

        public double TimeLeft => (_dueTime - DateTime.Now).TotalMilliseconds;
        
        public new void Start()
        {
            _dueTime = DateTime.Now.AddMilliseconds(Interval);
            base.Start();
        }

        private void ElapsedAction(object sender, ElapsedEventArgs e)
        {
            if (AutoReset)
                _dueTime = DateTime.Now.AddMilliseconds(Interval);
        }
    }
}