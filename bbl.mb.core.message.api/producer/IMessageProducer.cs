using bbl.mb.core.message.api.payload;

namespace bbl.mb.core.message.api.producer
{
    public interface IMessageProducer
    {
        Task<MessageActionResult> PostAsync(MessagePayload messagePayload);
    }
}