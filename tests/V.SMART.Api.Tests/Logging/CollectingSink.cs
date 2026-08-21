using Serilog.Core;
using Serilog.Events;

namespace V.SMART.Api.Tests.Logging
{
    /// <summary>
    /// M2-B11 — an in-memory Serilog sink, so the tests below assert on the ACTUAL
    /// <see cref="LogEvent"/> the pipeline produced, including the destructuring policy and
    /// the property names, rather than on a mock's recorded arguments. The distinction
    /// matters: the credential-leak test is only meaningful if it inspects what a real sink
    /// would have been handed.
    /// </summary>
    public sealed class CollectingSink : ILogEventSink
    {
        private readonly List<LogEvent> _events = new();

        public IReadOnlyList<LogEvent> Events
        {
            get { lock (_events) { return _events.ToList(); } }
        }

        public void Emit(LogEvent logEvent)
        {
            lock (_events) { _events.Add(logEvent); }
        }

        /// <summary>
        /// Every event rendered exactly as the compact-JSON file sink would render it —
        /// message, properties, exception and all. This is the text the leak tests search.
        /// </summary>
        public string RenderAll()
        {
            var writer = new StringWriter();
            var formatter = new Serilog.Formatting.Compact.CompactJsonFormatter();

            foreach (var logEvent in Events)
            {
                formatter.Format(logEvent, writer);
            }

            return writer.ToString();
        }
    }
}
