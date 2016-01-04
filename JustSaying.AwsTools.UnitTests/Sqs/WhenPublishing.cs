using System.Collections.Generic;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using JustBehave;
using JustSaying.Messaging.MessageSerialisation;
using JustSaying.TestingFramework;
using NSubstitute;

namespace JustSaying.AwsTools.UnitTests.Sqs
{
    public class WhenPublishing : BehaviourTest<SqsPublisher>
    {
        private readonly IMessageSerialisationRegister _serialisationRegister = Substitute.For<IMessageSerialisationRegister>();
        private readonly IAmazonSQS _sqs = Substitute.For<IAmazonSQS>();
        private const string Url = "https://blablabla/" + QueueName;
        private readonly GenericMessage _message = new GenericMessage {Content = "Hello"};
        private const string QueueName = "queuename";

        protected override SqsPublisher CreateSystemUnderTest()
        {
            return new SqsPublisher(RegionEndpoint.EUWest1, QueueName, _sqs, 0, _serialisationRegister);
        }

        protected override void Given()
        {
            _sqs.ListQueues(Arg.Any<ListQueuesRequest>()).Returns(new ListQueuesResponse { QueueUrls = new List<string> { Url } });
            _sqs.GetQueueAttributes(Arg.Any<GetQueueAttributesRequest>()).Returns(new GetQueueAttributesResponse());
            _serialisationRegister.Serialise(_message, false).Returns("serialized_contents");
        }

        protected override void When()
        {
            SystemUnderTest.Publish(_message);
        }

        [Then]
        public void MessageIsPublishedToQueue()
        {
            // ToDo: Can be better...
            _sqs.Received().SendMessage(Arg.Is<SendMessageRequest>(x => x.MessageBody.Equals("serialized_contents")));
        }

        [Then]
        public void MessageIsPublishedToCorrectLocation()
        {
            _sqs.Received().SendMessage(Arg.Is<SendMessageRequest>(x => x.QueueUrl == Url));
        }
    }
}