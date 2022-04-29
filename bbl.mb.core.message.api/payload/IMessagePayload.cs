namespace bbl.mb.core.message.api.payload
{
    public interface IMessagePayload<T> where T : class
    {
        T Payload { get; }
    }
}