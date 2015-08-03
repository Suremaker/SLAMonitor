namespace Wonga.SLAMonitor.Async
{
    /// <summary>
    /// Interface for SLA processing dedicated for asynchronous request-response model
    /// </summary>
    public interface ISlaProcessor
    {
        /// <summary>
        /// Captures outgoing (request) message time.
        /// </summary>
        void ProcessOutgoingMessage(object message);
        /// <summary>
        /// Captures incoming (response) message time and generates SLA violation logs if message has SLA defined and is has been violated.
        /// </summary>
        void ProcessIncomingMessage(object message);
    }
}