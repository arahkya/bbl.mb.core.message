namespace bbl.mb.core.message.api
{
    public class MessageManager
    {
        private static Lazy<MessageManager> _instance = new Lazy<MessageManager>();

        public static MessageManager Instance
        {
            get => _instance.Value;
        }

        public void Setup(MessageConfigure messageConfigure)
        {
            
        }

        public async Task<MessageActionResult> PostAsync<T>(MessagePayload<T> messagePayload) where T : class
        {
            return await Task.FromResult(new MessageActionResult
            {
                MessageId = Guid.NewGuid(),
                IsSuccess = true
            });
        }
    }
}