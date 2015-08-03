using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using log4net;

namespace Wonga.SLAMonitor.Async
{
    /// <summary>
    /// SLA Processor dedicated for asynchronous request-response model
    /// </summary>
    public class SlaProcessor : ISlaProcessor
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SlaProcessor));
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, Stopwatch>> _tracers = new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, Stopwatch>>();
        private readonly SlaProvider _provider;

        public SlaProcessor(SlaProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Captures outgoing (request) message time.
        /// </summary>
        public void ProcessOutgoingMessage(object message)
        {
            var watch = new Stopwatch();
            watch.Start();
            foreach (var definition in _provider.GetRequestDefinitions(message))
            {
                var correlationId = definition.GetCorrelationId(message);

                _tracers.GetOrAdd(definition.Type, type => new ConcurrentDictionary<Guid, Stopwatch>())
                    .AddOrUpdate(correlationId, watch, (key, val) => watch);
            }
        }

        /// <summary>
        /// Captures incoming (response) message time and generates SLA violation logs if message has SLA defined and is has been violated.
        /// </summary>
        public void ProcessIncomingMessage(object message)
        {
            foreach (var definition in _provider.GetSlaDefinitionsFor(message))
            {
                ConcurrentDictionary<Guid, Stopwatch> tracingsForType;
                if (!_tracers.TryGetValue(definition.Request.Type, out tracingsForType))
                    continue;
                Stopwatch watch;
                var correlationId = definition.Response.GetCorrelationId(message);
                if (!tracingsForType.TryRemove(correlationId, out watch))
                    continue;

                watch.Stop();

                ProcessSla(definition, watch.Elapsed, correlationId);
            }
        }

        protected virtual void ProcessSla(SlaDefinition definition, TimeSpan elapsed, Guid correlationId)
        {
            Action<string, object[]> log = _logger.DebugFormat;
            if (elapsed > definition.Sla)
                log = _logger.ErrorFormat;

            log("SLA={0} Response={1} ResponseTime={2} milliseconds CorrelationId={3}",new object[] {definition.Request.Type, definition.Response.Type, (long)elapsed.TotalMilliseconds, correlationId});
        }
    }
}