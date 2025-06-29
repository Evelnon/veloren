using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Helpers for working with optional values.
    /// Port of common/src/util/option.rs.
    /// </summary>
    public static class OptionUtil
    {
        public static T? EitherWith<T>(T? opt1, T? opt2, Func<T, T, T> f) where T : struct
        {
            if (opt1.HasValue && opt2.HasValue) return f(opt1.Value, opt2.Value);
            return opt1.HasValue ? opt1 : opt2;
        }

        public static T? EitherWithRef<T>(T? opt1, T? opt2, Func<T, T, T> f) where T : class
        {
            if (opt1 != null && opt2 != null) return f(opt1, opt2);
            return opt1 ?? opt2;
        }
    }
}
