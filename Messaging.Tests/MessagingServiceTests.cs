using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messaging.Abstraction;
using NUnit.Framework;

namespace Messaging.Tests
{
    [TestFixture]
    public class MessagingServiceTests
    {
        private IMessagingService messagingService { get; set; }

        [SetUp]
        public void SetUpTest()
        {
			messagingService = new MessagingService();
        }

        [Test]
        public void CanSendTestMessage()
        {
			Assert.IsNotNull(messagingService.SendMessage<TestCommand, TestResponse>(new TestCommand()));
        }
    }
}
