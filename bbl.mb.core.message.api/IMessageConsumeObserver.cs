namespace bbl.mb.core.message.api
{
    public interface IMessageConsumeObserver : IObserver<string>
    {
        MessageConsumerConfigure Configure { get; internal set; }
    }
}