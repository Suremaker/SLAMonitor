using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net.Appender;
using log4net.Core;

namespace Wonga.SLAMonitor.Tests.Helpers
{
    internal class InMemoryAppender : AppenderSkeleton
    {
        private readonly ConcurrentQueue<LoggingEvent> _events = new ConcurrentQueue<LoggingEvent>();
        public IEnumerable<LoggingEvent> Events { get { return _events; } }
        protected override void Append(LoggingEvent loggingEvent)
        {
            _events.Enqueue(loggingEvent);
        }
    }
}