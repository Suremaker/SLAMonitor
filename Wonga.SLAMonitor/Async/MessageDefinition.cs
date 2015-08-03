using System;

namespace Wonga.SLAMonitor.Async
{
    /// <summary>
    /// Message definition class, containing message type and method to retrieve request-response correlation ID
    /// </summary>
    public class MessageDefinition
    {
        private readonly Func<object, Guid> _correlationFn;

        private MessageDefinition(Type type, Func<object, Guid> correlationFn)
        {
            Type = type;
            _correlationFn = correlationFn;
        }

        /// <summary>
        /// Receives correlation Id from message
        /// </summary>
        public Guid GetCorrelationId(object message)
        {
            if (!Type.IsInstanceOfType(message))
                throw new InvalidOperationException(string.Format("The specified message is not of {0} type: {1}", Type, message.GetType()));
            return _correlationFn(message);
        }

        /// <summary>
        /// Message type
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Creates a message definition of TMessage type with function to retrieve correlation ID
        /// </summary>
        public static MessageDefinition Create<TMessage>(Func<TMessage, Guid> correlationFn)
        {
            return new MessageDefinition(typeof(TMessage), m => correlationFn.Invoke((TMessage)m));
        }
    }
}
