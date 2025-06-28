using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Generic link interface for relationships between entities.
    /// Mirrors the behaviour of Rust's <c>Link</c> trait in a simplified form.
    /// </summary>
    public interface ILink<L, TError, TCreateData, TPersistData, TDeleteData>
        where L : class, ILink<L, TError, TCreateData, TPersistData, TDeleteData>
    {
        TError Create(LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> handle, ref TCreateData data);
        bool Persist(LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> handle, ref TPersistData data);
        void Delete(LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> handle, ref TDeleteData data);
    }

    /// <summary>
    /// Marker interface for roles associated with a link.
    /// </summary>
    public interface IRole<L> where L : class { }

    /// <summary>
    /// Component storing a reference to a link role.
    /// </summary>
    [Serializable]
    public class Is<R, L, TError, TCreateData, TPersistData, TDeleteData> : ICloneable
        where L : class, ILink<L, TError, TCreateData, TPersistData, TDeleteData>
        where R : IRole<L>
    {
        private readonly LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> _link;

        public Is(LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> link)
        {
            _link = link;
        }

        public void Delete(ref TDeleteData data) => _link.Value.Delete(_link, ref data);

        public LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> LinkHandle => _link;

        public Is<R, L, TError, TCreateData, TPersistData, TDeleteData> Clone()
        {
            return new Is<R, L, TError, TCreateData, TPersistData, TDeleteData>(_link.Clone());
        }

        object ICloneable.Clone() => Clone();
    }

    /// <summary>
    /// Strong reference to a link instance.
    /// </summary>
    [Serializable]
    public class LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> :
        IEquatable<LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData>>,
        ICloneable
        where L : class, ILink<L, TError, TCreateData, TPersistData, TDeleteData>
    {
        private readonly L _link;
        public LinkHandle(L link) { _link = link; }

        /// <summary>Access the underlying link.</summary>
        public L Value => _link;

        public static LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> FromLink(L link) => new(link);
        public Is<R, L, TError, TCreateData, TPersistData, TDeleteData> MakeRole<R>() where R : IRole<L> => new(this);
        public WeakLinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> Downgrade() => new(_link);

        public LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> Clone() => new(_link);

        object ICloneable.Clone() => Clone();

        public bool Equals(LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData>? other) =>
            other is not null && ReferenceEquals(_link, other._link);

        public override bool Equals(object? obj) => obj is LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> other && Equals(other);

        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_link);
    }

    /// <summary>
    /// Weak reference to a link instance.
    /// </summary>
    [Serializable]
    public class WeakLinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> :
        IEquatable<WeakLinkHandle<L, TError, TCreateData, TPersistData, TDeleteData>>,
        ICloneable
        where L : class, ILink<L, TError, TCreateData, TPersistData, TDeleteData>
    {
        private readonly WeakReference<L> _link;
        public WeakLinkHandle(L link) { _link = new WeakReference<L>(link); }
        internal WeakLinkHandle(WeakReference<L> link) { _link = link; }
        public LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData>? Upgrade() =>
            _link.TryGetTarget(out var t) ? new LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData>(t) : null;
        public DynWeakLinkHandle IntoDyn() => DynWeakLinkHandle.FromWeakReference<L, TError, TCreateData, TPersistData, TDeleteData>(_link);
        public bool Exists => _link.TryGetTarget(out _);

        public WeakLinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> Clone() =>
            new WeakLinkHandle<L, TError, TCreateData, TPersistData, TDeleteData>(_link);

        object ICloneable.Clone() => Clone();

        public bool Equals(WeakLinkHandle<L, TError, TCreateData, TPersistData, TDeleteData>? other)
        {
            if (other is null) return false;
            return _link.TryGetTarget(out var t1) && other._link.TryGetTarget(out var t2) && ReferenceEquals(t1, t2);
        }

        public override bool Equals(object? obj) => obj is WeakLinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> other && Equals(other);

        public override int GetHashCode()
        {
            if (_link.TryGetTarget(out var target))
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(target);
            }
            return 0;
        }
    }

    /// <summary>
    /// Type-erased weak link used for heterogeneous collections.
    /// </summary>
    [Serializable]
    public class DynWeakLinkHandle : IEquatable<DynWeakLinkHandle> {
        private readonly WeakReference<object> _inner;
        public DynWeakLinkHandle() { _inner = new WeakReference<object>(null); }
        internal DynWeakLinkHandle(WeakReference<object> inner) { _inner = inner; }

        internal static DynWeakLinkHandle FromWeakReference<L, TError, TCreateData, TPersistData, TDeleteData>(WeakReference<L> link)
            where L : class, ILink<L, TError, TCreateData, TPersistData, TDeleteData>
        {
            var target = link.TryGetTarget(out var t) ? (object)t : null;
            return new DynWeakLinkHandle(new WeakReference<object>(target));
        }

        public bool Exists => _inner.TryGetTarget(out _);

        public bool IsLink<L, TError, TCreateData, TPersistData, TDeleteData>(LinkHandle<L, TError, TCreateData, TPersistData, TDeleteData> link)
            where L : class, ILink<L, TError, TCreateData, TPersistData, TDeleteData>
        {
            return _inner.TryGetTarget(out var t) && ReferenceEquals(t, link.Value);
        }

        public bool Equals(DynWeakLinkHandle? other)
        {
            if (other is null) return false;
            return _inner.TryGetTarget(out var t1) && other._inner.TryGetTarget(out var t2) && ReferenceEquals(t1, t2);
        }

        public override bool Equals(object? obj) => obj is DynWeakLinkHandle other && Equals(other);

        public override int GetHashCode()
        {
            if (_inner.TryGetTarget(out var target))
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(target);
            }
            return 0;
        }
    }
}
