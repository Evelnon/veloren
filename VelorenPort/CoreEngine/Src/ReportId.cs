using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Identifier for status or behavior reports.
    /// </summary>
    [Serializable]
    public readonly record struct ReportId(uint Value);
}
