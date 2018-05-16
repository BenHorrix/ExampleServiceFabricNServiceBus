using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus;

namespace Messages
{
    public class CastVote : ICommand
    {
        public string Name { get; set; }
    }
}
