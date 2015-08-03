using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Wonga.SLAMonitor.Async;

namespace Wonga.SLAMonitor.Tests.Async
{
    [TestFixture]
    public class SlaProviderTests
    {
        public interface IRequestOne { Guid OneId { get; set; } }
        public interface IRequestTwo { Guid TwoId { get; set; } }
        public interface IExtendedRequestOne : IRequestOne { }
        public interface IResponseOne { Guid Id { get; set; } }
        public interface IResponseTwo { Guid Id { get; set; } }
        public interface IExtendedResponseOne : IResponseOne { }

        [Test]
        public void Provider_should_return_all_matching_request_definitions()
        {
            var provider = new SlaProvider();

            var def1 = new SlaDefinition(
                MessageDefinition.Create<IRequestOne>(r => r.OneId),
                MessageDefinition.Create<IResponseOne>(r => r.Id),
                TimeSpan.FromSeconds(1));

            var def2 = new SlaDefinition(
                MessageDefinition.Create<IRequestTwo>(r => r.TwoId),
                MessageDefinition.Create<IResponseTwo>(r => r.Id),
                TimeSpan.FromSeconds(2));

            var def3 = new SlaDefinition(
                MessageDefinition.Create<IExtendedRequestOne>(r => r.OneId),
                MessageDefinition.Create<IResponseOne>(r => r.Id),
                TimeSpan.FromSeconds(3));

            provider.Add(def1);
            provider.Add(def2);
            provider.Add(def3);

            var definitions = provider.GetRequestDefinitions(new Mock<IExtendedRequestOne>().Object);

            Assert.That(definitions.ToArray(), Is.EquivalentTo(new[] { def1.Request, def3.Request }));
        }

        [Test]
        public void Provider_should_return_all_matching_sla_definitions_for_given_response()
        {
            var provider = new SlaProvider();

            var def1 = new SlaDefinition(
                MessageDefinition.Create<IRequestOne>(r => r.OneId),
                MessageDefinition.Create<IResponseOne>(r => r.Id),
                TimeSpan.FromSeconds(1));

            var def2 = new SlaDefinition(
                MessageDefinition.Create<IRequestTwo>(r => r.TwoId),
                MessageDefinition.Create<IResponseTwo>(r => r.Id),
                TimeSpan.FromSeconds(2));

            var def3 = new SlaDefinition(
                MessageDefinition.Create<IExtendedRequestOne>(r => r.OneId),
                MessageDefinition.Create<IResponseOne>(r => r.Id),
                TimeSpan.FromSeconds(3));

            var def4 = new SlaDefinition(
                MessageDefinition.Create<IRequestOne>(r => r.OneId),
                MessageDefinition.Create<IExtendedResponseOne>(r => r.Id),
                TimeSpan.FromSeconds(3));

            provider.Add(def1);
            provider.Add(def2);
            provider.Add(def3);
            provider.Add(def4);

            var definitions = provider.GetSlaDefinitionsFor(new Mock<IExtendedResponseOne>().Object);

            Assert.That(definitions.ToArray(), Is.EquivalentTo(new[] { def1, def3, def4 }));
        }

        [Test]
        public void Provider_should_return_one_request_definition_if_multiple_slas_are_referring_to_the_same_request()
        {
            var provider = new SlaProvider();

            var requestDefinition = MessageDefinition.Create<IRequestOne>(r => r.OneId);

            var def1 = new SlaDefinition(
                requestDefinition,
                MessageDefinition.Create<IResponseOne>(r => r.Id),
                TimeSpan.FromSeconds(1));

            var def2 = new SlaDefinition(
                requestDefinition,
                MessageDefinition.Create<IResponseTwo>(r => r.Id),
                TimeSpan.FromSeconds(2));

            provider.Add(def1);
            provider.Add(def2);

            Assert.That(provider.GetRequestDefinitions(new Mock<IRequestOne>().Object).ToArray(), Is.EqualTo(new[] { requestDefinition }));
        }
    }
}