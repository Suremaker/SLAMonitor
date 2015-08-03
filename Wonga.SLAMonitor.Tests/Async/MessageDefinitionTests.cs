using System;
using NUnit.Framework;
using Wonga.SLAMonitor.Async;

namespace Wonga.SLAMonitor.Tests.Async
{
    [TestFixture]
    public class MessageDefinitionTests
    {
        class TestMessage
        {
            public Guid FirstId { get; set; }
            public Guid SecondId { get; set; }
        }

        [Test]
        public void Definition_should_contain_a_proper_message_type()
        {
            var definition = MessageDefinition.Create<TestMessage>(m => m.FirstId);
            Assert.That(definition.Type, Is.EqualTo(typeof(TestMessage)));
        }

        [Test]
        public void Definition_should_allow_to_retrieve_proper_correlation_id()
        {
            var message = new TestMessage { FirstId = Guid.NewGuid(), SecondId = Guid.NewGuid() };
            var def1 = MessageDefinition.Create<TestMessage>(m => m.FirstId);
            var def2 = MessageDefinition.Create<TestMessage>(m => m.SecondId);

            Assert.That(def1.GetCorrelationId(message), Is.EqualTo(message.FirstId), "It should use FirstId");
            Assert.That(def2.GetCorrelationId(message), Is.EqualTo(message.SecondId), "It should use SecondId");
        }

        [Test]
        public void Definition_should_not_allow_to_use_wrong_message_type_to_retrieve_correlation_id()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => MessageDefinition.Create<TestMessage>(x => x.FirstId).GetCorrelationId(""));

            Assert.That(exception.Message, Is.EqualTo(string.Format("The specified message is not of {0} type: {1}", typeof(TestMessage), typeof(string))));
        }

        [Test]
        public void Definition_should_not_allow_to_use_null_to_retrieve_correlation_id()
        {
            Assert.Throws<ArgumentNullException>(() => MessageDefinition.Create<TestMessage>(x => x.FirstId).GetCorrelationId(null));
        }
    }
}
