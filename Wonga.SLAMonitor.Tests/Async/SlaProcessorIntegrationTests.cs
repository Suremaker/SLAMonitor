using System;
using System.Collections;
using System.Linq;
using System.Threading;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NUnit.Framework;
using Wonga.SLAMonitor.Async;
using Wonga.SLAMonitor.Tests.Helpers;

namespace Wonga.SLAMonitor.Tests.Async
{
    [TestFixture]
    public class SlaProcessorIntegrationTests
    {
        class Request { public Guid Id { get; set; } }
        class Response { public Guid Id { get; set; } }

        private SlaProvider _provider;
        private SlaProcessor _processor;
        private readonly TimeSpan _sla = TimeSpan.FromMilliseconds(100);
        private InMemoryAppender _appender;

        [SetUp]
        public void SetUp()
        {
            _provider = new SlaProvider();
            _processor = new SlaProcessor(_provider);

            SlaDefinitionBuilder.For<Request>(r => r.Id)
                .AddSla<Response>(_sla, r => r.Id)
                .Configure(_provider);

            _appender = InjectInMemoryAppender();
        }

        private InMemoryAppender InjectInMemoryAppender()
        {
            var appender = new InMemoryAppender();
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.ResetConfiguration();
            hierarchy.Root.AddAppender(appender);
            hierarchy.Root.Level = Level.All;
            appender.ActivateOptions();
            hierarchy.Configured = true;
            return appender;
        }

        [Test]
        public void Response_within_sla_should_result_with_DEBUG_log_entry()
        {
            var id = Guid.Parse("cca0662a-b6fb-4b26-b6dd-97ba1e7d32c2");
            _processor.ProcessOutgoingMessage(new Request { Id = id });
            _processor.ProcessIncomingMessage(new Response { Id = id });

            Assert.That(_appender.Events.Count(), Is.EqualTo(1), "events count");
            var logEvent = _appender.Events.Single();
            Assert.That(logEvent.Level, Is.EqualTo(Level.Debug), "event level");

            Assert.That(logEvent.MessageObject.ToString(), Is.StringMatching("SLA=Wonga.SLAMonitor.Tests.Async.SlaProcessorIntegrationTests\\+Request Response=Wonga.SLAMonitor.Tests.Async.SlaProcessorIntegrationTests\\+Response ResponseTime=[0-9]+ milliseconds CorrelationId=cca0662a-b6fb-4b26-b6dd-97ba1e7d32c2"), "content");
        }

        [Test]
        public void Response_violating_sla_should_result_with_ERROR_log_entry()
        {
            var id = Guid.Parse("cca0662a-b6fb-4b26-b6dd-97ba1e7d32c2");
            _processor.ProcessOutgoingMessage(new Request { Id = id });
            Thread.Sleep(_sla.Add(TimeSpan.FromMilliseconds(100)));
            _processor.ProcessIncomingMessage(new Response { Id = id });

            Assert.That(_appender.Events.Count(), Is.EqualTo(1), "events count");
            var logEvent = _appender.Events.Single();
            Assert.That(logEvent.Level, Is.EqualTo(Level.Error), "event level");

            Assert.That(logEvent.MessageObject.ToString(), Is.StringMatching("SLA=Wonga.SLAMonitor.Tests.Async.SlaProcessorIntegrationTests\\+Request Response=Wonga.SLAMonitor.Tests.Async.SlaProcessorIntegrationTests\\+Response ResponseTime=[0-9]+ milliseconds CorrelationId=cca0662a-b6fb-4b26-b6dd-97ba1e7d32c2"), "content");
        }
    }
}