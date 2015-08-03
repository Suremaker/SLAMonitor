using System;
using System.Collections.Generic;

namespace Wonga.SLAMonitor.Async
{
    /// <summary>
    /// A fluent interface for building SLA definitions.
    /// </summary>
    public class SlaDefinitionBuilder
    {
        private readonly MessageDefinition _request;
        private readonly List<SlaDefinition> _definitions = new List<SlaDefinition>();

        private SlaDefinitionBuilder(MessageDefinition request)
        {
            _request = request;
        }

        /// <summary>
        /// Defines SLA for TRequest - TResponse messages
        /// </summary>
        /// <typeparam name="TResponse">Type of response message</typeparam>
        /// <param name="sla">SLA</param>
        /// <param name="correlationFn">Method to retrieve request-response correlation ID for TResponse</param>
        public SlaDefinitionBuilder AddSla<TResponse>(TimeSpan sla, Func<TResponse, Guid> correlationFn)
        {
            _definitions.Add(new SlaDefinition(_request, MessageDefinition.Create(correlationFn), sla));
            return this;
        }

        /// <summary>
        /// Configures SlaProvider instance with SLA definitions.
        /// </summary>
        public void Configure(SlaProvider provider)
        {
            foreach (var definition in _definitions)
                provider.Add(definition);
        }

        /// <summary>
        /// Begins a SLA definition configuration for TRequest request message.
        /// </summary>
        /// <typeparam name="TRequest">Type of request message</typeparam>
        /// <param name="correlationFn">Method to retrieve request-response correlation ID for TRequest</param>
        public static SlaDefinitionBuilder For<TRequest>(Func<TRequest, Guid> correlationFn)
        {
            return new SlaDefinitionBuilder(MessageDefinition.Create(correlationFn));
        }

        /// <summary>
        /// Defines SLA in-line.
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="sla">SLA</param>
        /// <param name="requestCorrelator">Method to retrieve request-response correlation ID for TRequest</param>
        /// <param name="responseCorrelator">Method to retrieve request-response correlation ID for TResponse</param>
        /// <param name="provider">SLA provider to configure</param>
        public static void AddSla<TRequest, TResponse>(
            TimeSpan sla,
            Func<TRequest, Guid> requestCorrelator,
            Func<TResponse, Guid> responseCorrelator, 
            SlaProvider provider)
        {
            For(requestCorrelator).AddSla(sla, responseCorrelator).Configure(provider);
        }
    }
}