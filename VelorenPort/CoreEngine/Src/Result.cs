using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Generic result type replicating Rust's <c>Result&lt;T, E&gt;</c> pattern.
    /// </summary>
    public readonly struct Result<T, E> {
        private readonly T _ok;
        private readonly E _err;
        public bool IsOk { get; }
        public T Value => IsOk ? _ok : throw new InvalidOperationException();
        public E Error => !IsOk ? _err : throw new InvalidOperationException();
        private Result(T ok, E err, bool isOk) { _ok = ok; _err = err; IsOk = isOk; }
        public static Result<T, E> Ok(T val) => new(val, default!, true);
        public static Result<T, E> Err(E err) => new(default!, err, false);
    }
}
