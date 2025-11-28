using System;

namespace account_calendar
{
    public class DateSelectedEventArgs : EventArgs
    {
        public DateTime SelectedDate { get; }

        public DateSelectedEventArgs(DateTime selectedDate)
        {
            SelectedDate = selectedDate.Date;
        }
    }
}