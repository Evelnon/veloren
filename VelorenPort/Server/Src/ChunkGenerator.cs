using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using Unity.Entities;
using Unity.Mathematics;
using VelorenPort.CoreEngine;
using VelorenPort.World;

namespace VelorenPort.Server {
    /// <summary>
    /// Generates terrain chunks asynchronously. Ported from server/src/chunk_generator.rs
    /// </summary>
    public class ChunkGenerator {
        private readonly Channel<ChunkGenResult> _channel = Channel.CreateUnbounded<ChunkGenResult>();
        private readonly Dictionary<int2, CancellationTokenSource> _pending = new();
        private readonly ChunkGenMetrics _metrics;

        public ChunkGenerator(ChunkGenMetrics metrics) {
            _metrics = metrics;
        }

        public void GenerateChunk(
            Entity? entity,
            int2 key,
            SlowJobPool slowJobPool,
            World.World world,
            object rtsim,
            TestWorld.IndexOwned index,
            (TimeOfDay, Calendar) time)
        {
            if (_pending.ContainsKey(key)) return;
            var cts = new CancellationTokenSource();
            _pending[key] = cts;
            _metrics.ChunksRequested.Inc();
            var writer = _channel.Writer;
            slowJobPool.Spawn("CHUNK_GENERATOR", () => {
                var res = world.GenerateChunk(index.AsIndexRef(), key, null, () => cts.IsCancellationRequested, time);
                if (cts.IsCancellationRequested) {
                    writer.TryWrite(new ChunkGenResult(key, entity));
                } else {
                    writer.TryWrite(new ChunkGenResult(key, res));
                }
            });
        }

        public (int2 key, Result<(Chunk, ChunkSupplement), Entity?> res)? RecvNewChunk() {
            while (_channel.Reader.TryRead(out var entry)) {
                if (_pending.Remove(entry.Key)) {
                    if (entry.Result.IsOk) _metrics.ChunksServed.Inc(); else _metrics.ChunksCanceled.Inc();
                    return (entry.Key, entry.Result);
                }
            }
            return null;
        }

        public IEnumerable<int2> PendingChunks() => _pending.Keys;
        public IEnumerable<int2> ParPendingChunks() => _pending.Keys; // no Rayon equivalent

        public void CancelIfPending(int2 key) {
            if (_pending.Remove(key, out var cts)) {
                cts.Cancel();
                _metrics.ChunksCanceled.Inc();
            }
        }

        public void CancelAll() {
            foreach (var kv in _pending) {
                kv.Value.Cancel();
                _metrics.ChunksCanceled.Inc();
            }
            _pending.Clear();
        }
    }

    public readonly struct ChunkGenResult {
        public int2 Key { get; }
        public Result<(Chunk Chunk, ChunkSupplement Supplement), Entity?> Result { get; }

        public ChunkGenResult(int2 key, (Chunk, ChunkSupplement) payload) {
            Key = key; Result = Result<(Chunk, ChunkSupplement), Entity?>.Ok(payload);
        }

        public ChunkGenResult(int2 key, Entity? entity) {
            Key = key; Result = Result<(Chunk, ChunkSupplement), Entity?>.Err(entity);
        }
    }

    public readonly struct Result<T, E> {
        private readonly T _ok;
        private readonly E _err;
        public bool IsOk { get; }
        public T OkValue => IsOk ? _ok : throw new InvalidOperationException();
        public E ErrValue => !IsOk ? _err : throw new InvalidOperationException();
        private Result(T ok, E err, bool isOk) { _ok = ok; _err = err; IsOk = isOk; }
        public static Result<T,E> Ok(T val) => new(val, default!, true);
        public static Result<T,E> Err(E err) => new(default!, err, false);
    }
}
