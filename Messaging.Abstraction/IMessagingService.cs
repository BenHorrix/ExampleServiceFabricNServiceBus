using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Abstraction
{
    public interface IMessagingService
    {
        TReturnType SendMessage<TMessageType, TReturnType>(TMessageType message);
    }
}
