using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace Handlers
{
    public class CastVoteHandler : IHandleMessages<CastVote>
    {
        static ILog log = LogManager.GetLogger<CastVoteHandler>();

        public Task Handle(CastVote message, IMessageHandlerContext context)
        {
            log.Info($"Received PlaceOrder, OrderId = {message.Name}");
            return Task.CompletedTask;
        }
    }
}
