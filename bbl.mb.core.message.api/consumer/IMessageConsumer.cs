namespace bbl.mb.core.message.api.consumer
{
    public interface IMessageConsumer : IObservable<string>
    {
        void StartConsume(MessageConsumerConfigure messageConsumerConfigure, CancellationToken cancellationToken);
    }
}