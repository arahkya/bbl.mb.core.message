namespace bbl.mb.core.message.api
{
    public interface IMessageManager
    {
        Task<MessageActionResult> PostAsync<T>(MessagePayload<T> messagePayload) where T : class;
    }
}