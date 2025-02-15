// ReSharper disable InconsistentNaming

using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ.Events;
using EasyNetQ.Tests.Mocking;
using EasyNetQ.Topology;
using NSubstitute;
using Xunit;

namespace EasyNetQ.Tests.ConsumeTests
{
    public class When_a_consumer_is_cancelled_by_the_broker : IDisposable
    {
        private readonly MockBuilder mockBuilder;

        public When_a_consumer_is_cancelled_by_the_broker()
        {
            mockBuilder = new MockBuilder();

            var queue = new Queue("my_queue", false);

            mockBuilder.Bus.Advanced.Consume(
                queue,
                (_, _, _) => Task.Run(() => { }),
                c => c.WithConsumerTag("consumer_tag")
            );

            var are = new AutoResetEvent(false);
            mockBuilder.EventBus.Subscribe((in ConsumerModelDisposedEvent _) => are.Set());

            mockBuilder.Consumers[0].HandleBasicCancel("consumer_tag").GetAwaiter().GetResult();

            if (!are.WaitOne(5000))
            {
                throw new TimeoutException();
            }
        }

        public void Dispose()
        {
            mockBuilder.Dispose();
        }

        [Fact]
        public void Should_dispose_of_the_model()
        {
            mockBuilder.Consumers[0].Model.Received().Dispose();
        }
    }
}

// ReSharper restore InconsistentNaming
