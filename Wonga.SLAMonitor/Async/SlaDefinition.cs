using System;

namespace Wonga.SLAMonitor.Async
{
    /// <summary>
    /// SLA definition.
    /// </summary>
    public class SlaDefinition
    {
        public SlaDefinition(MessageDefinition request, MessageDefinition response, TimeSpan sla)
        {
            Request = request;
            Response = response;
            Sla = sla;
        }

        public MessageDefinition Request { get; private set; }
        public MessageDefinition Response { get; private set; }
        public TimeSpan Sla { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} -> {1}: {2}", Request, Response, Sla);
        }
    }
}