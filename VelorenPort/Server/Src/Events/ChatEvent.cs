using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server.Events;

/// <summary>
/// Event carrying chat message data. Mirrors `ChatEvent` from the
/// Rust server but in simplified form.
/// </summary>
public readonly record struct ChatEvent(UnresolvedChatMsg Msg, bool FromClient);
