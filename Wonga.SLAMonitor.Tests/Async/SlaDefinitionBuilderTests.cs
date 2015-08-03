using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Wonga.SLAMonitor.Async;

namespace Wonga.SLAMonitor.Tests.Async
{
    [TestFixture]
    public class SlaDefinitionBuilderTests
    {
        public interface IRequest { Guid RequestId { get; set; } }
        public interface IResponse { Guid ResponseId { get; set; } }
        public interface IErrorResponse { Guid ResponseId { get; set; } }

        [Test]
        public void It_should_create_definitions()
        {
            var provider = new SlaProvider();

            SlaDefinitionBuilder.For<IRequest>(x => x.RequestId)
                .AddSla<IResponse>(TimeSpan.FromSeconds(1), x => x.ResponseId)
                .AddSla<IErrorResponse>(TimeSpan.FromSeconds(2), x => x.ResponseId)
                .Configure(provider);

            Assert.That(provider.GetRequestDefinitions(new Mock<IRequest>().Object).Count(), Is.EqualTo(1), "request mapping");

            Assert.That(provider.GetSlaDefinitionsFor(new Mock<IResponse>().Object).Single().Sla,Is.EqualTo(TimeSpan.FromSeconds(1)));
            Assert.That(provider.GetSlaDefinitionsFor(new Mock<IErrorResponse>().Object).Single().Sla, Is.EqualTo(TimeSpan.FromSeconds(2)));
        }
    }
}