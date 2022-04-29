namespace bbl.mb.core.message.api
{
    public interface IMessageConsumer : IObservable<string>
    {
        void StartConsume(MessageConsumerConfigure messageConsumerConfigure, CancellationToken cancellationToken);
    }
}