using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messaging.Abstraction;

namespace Messaging
{
    public class MessagingService : IMessagingService
    {
        public MessagingService()
        {

        }

        public TReturnType SendMessage<TMessageType, TReturnType>(TMessageType message)
        {
            throw new NotImplementedException();
        }
    }
}
