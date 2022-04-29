namespace bbl.mb.core.message.api.consumer
{
    public interface IMessageConsumeObserver : IObserver<string>
    {
        MessageConsumerConfigure Configure { get; internal set; }
    }
}