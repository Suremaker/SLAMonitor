using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Wonga.SLAMonitor.Async;

namespace Wonga.SLAMonitor.Tests.Async
{
    [TestFixture]
    public class SlaProcessorTests
    {
        class TestableSlaProcessor : SlaProcessor
        {
            public TestableSlaProcessor(SlaProvider provider) : base(provider) { }
            public readonly ConcurrentQueue<Tuple<SlaDefinition, TimeSpan, Guid>> Results = new ConcurrentQueue<Tuple<SlaDefinition, TimeSpan, Guid>>();

            protected override void ProcessSla(SlaDefinition definition, TimeSpan elapsed, Guid correlationId)
            {
                Results.Enqueue(Tuple.Create(definition, elapsed, correlationId));
            }
        }

        private SlaProvider _slaProvider;
        private TestableSlaProcessor _slaProcessor;

        class Request { public Guid Id { get; set; } }
        class Response { public Guid Id { get; set; } }
        class ErrorResponse { public Guid Id { get; set; } }

        class Request2 { public Guid Id { get; set; } }
        class Response2 { public Guid Id { get; set; } }
        class ErrorResponse2 { public Guid Id { get; set; } }

        [SetUp]
        public void SetUp()
        {
            _slaProvider = new SlaProvider();
            _slaProcessor = new TestableSlaProcessor(_slaProvider);
        }

        [Test]
        public void Processor_should_measure_sla()
        {
            ConfigureSla();

            var id = Guid.NewGuid();
            var delayInMs = 200;

            _slaProcessor.ProcessOutgoingMessage(new Request { Id = id });

            Thread.Sleep((int)(delayInMs * 1.5));
            _slaProcessor.ProcessIncomingMessage(new Response { Id = id });

            Assert.That(_slaProcessor.Results.Count, Is.EqualTo(1), "results");
            var result = _slaProcessor.Results.Single();
            Assert.That(result.Item2.TotalMilliseconds, Is.GreaterThan(delayInMs).And.LessThan(2 * delayInMs), "elapsed");
            Assert.That(result.Item3, Is.EqualTo(id), "Correlation ID");
            Assert.That(result.Item1.Response.Type, Is.EqualTo(typeof(Response)), "response message");
        }

        [Test]
        public void Processor_should_measure_sla_once()
        {
            ConfigureSla();

            var id = Guid.NewGuid();

            _slaProcessor.ProcessOutgoingMessage(new Request { Id = id });
            _slaProcessor.ProcessIncomingMessage(new Response { Id = id });
            Assert.That(_slaProcessor.Results.Count, Is.EqualTo(1), "results");

            _slaProcessor.ProcessIncomingMessage(new Response { Id = id });
            _slaProcessor.ProcessIncomingMessage(new ErrorResponse { Id = id });
            Assert.That(_slaProcessor.Results.Count, Is.EqualTo(1), "results should still contain 1 entry");
        }

        [Test]
        public void Processor_should_be_able_to_process_multiple_messages_in_parallel()
        {
            ConfigureSla();
            var ids = Enumerable.Range(0, 50000).Select(i => Guid.NewGuid()).ToArray();

            ids.AsParallel().WithDegreeOfParallelism(64).ForAll(id =>
            {
                _slaProcessor.ProcessOutgoingMessage(new Request { Id = id });
                _slaProcessor.ProcessOutgoingMessage(new Request2 { Id = id });
            });

            ids.AsParallel().WithDegreeOfParallelism(64).ForAll(id =>
            {
                _slaProcessor.ProcessIncomingMessage(new Response { Id = id });
                _slaProcessor.ProcessIncomingMessage(new ErrorResponse2 { Id = id });
            });

            Assert.That(_slaProcessor.Results.Where(r => r.Item1.Request.Type == typeof(Request)).Select(r => r.Item3).Distinct().Count(), Is.EqualTo(ids.Count()), "Not all Request messages were processed");
            Assert.That(_slaProcessor.Results.Where(r => r.Item1.Request.Type == typeof(Request2)).Select(r => r.Item3).Distinct().Count(), Is.EqualTo(ids.Count()), "Not all Response messages were processed");
        }

        private void ConfigureSla()
        {
            SlaDefinitionBuilder.For<Request>(r => r.Id)
                .AddSla<Response>(TimeSpan.FromSeconds(1), r => r.Id)
                .AddSla<ErrorResponse>(TimeSpan.FromSeconds(1), r => r.Id)
                .Configure(_slaProvider);

            SlaDefinitionBuilder.For<Request2>(r => r.Id)
                .AddSla<Response2>(TimeSpan.FromSeconds(1), r => r.Id)
                .AddSla<ErrorResponse2>(TimeSpan.FromSeconds(1), r => r.Id)
                .Configure(_slaProvider);
        }
    }
}