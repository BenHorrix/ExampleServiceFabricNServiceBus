using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;

namespace Messaging.Abstraction
{
    public class TestCommand : ICommand
    {
        public string Hello { get; set; }
    }
}
