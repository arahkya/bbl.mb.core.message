namespace bbl.mb.core.message.api
{
    public interface IMessagePayload<T> where T : class
    {
        T Payload { get; }
    }
}