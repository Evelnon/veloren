using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Events that can occur in the in-game calendar.
    /// Mirrors CalendarEvent from the Rust code.
    /// </summary>
    public enum CalendarEvent : ushort {
        Christmas = 0,
        Halloween = 1,
        AprilFools = 2,
        Easter = 3,
    }

    /// <summary>
    /// Simple calendar that can detect seasonal events based on the current date.
    /// This is a lightweight version of the original implementation.
    /// </summary>
    public class Calendar {
        private readonly List<CalendarEvent> _events = new();

        public bool IsEvent(CalendarEvent ev) => _events.Contains(ev);
        public IEnumerable<CalendarEvent> Events => _events;

        public static Calendar FromEvents(IEnumerable<CalendarEvent> events) {
            var c = new Calendar();
            c._events.AddRange(events);
            return c;
        }

        public static Calendar FromTimezone(TimeZoneInfo? tz = null) {
            DateTime now = tz != null ? TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz) : DateTime.Now;
            var c = new Calendar();
            if (now.Month == 12 && now.Day >= 20 && now.Day <= 30) {
                c._events.Add(CalendarEvent.Christmas);
            }
            if (now.Month == 10 && now.Day >= 24 && now.Day <= 31) {
                c._events.Add(CalendarEvent.Halloween);
            }
            if (now.Month == 4 && now.Day == 1) {
                c._events.Add(CalendarEvent.AprilFools);
            }
            if ((now.Month == 3 && now.Day == 31) || (now.Month == 4 && now.Day >= 1 && now.Day <= 7)) {
                c._events.Add(CalendarEvent.Easter);
            }
            return c;
        }
    }
}
