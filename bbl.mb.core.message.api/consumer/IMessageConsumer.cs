namespace bbl.mb.core.message.api.consumer
{
    public interface IMessageConsumer : IObservable<string>
    {
        bool IsReady { get; }

        void StartConsume(MessageConsumerConfigure messageConsumerConfigure, CancellationToken cancellationToken);
    }
}