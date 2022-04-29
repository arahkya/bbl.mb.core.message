namespace bbl.mb.core.message.api
{
    public interface IMessageProducer
    {
        Task<MessageActionResult> PostAsync<T>(MessagePayload<T> messagePayload) where T : class;
    }
}