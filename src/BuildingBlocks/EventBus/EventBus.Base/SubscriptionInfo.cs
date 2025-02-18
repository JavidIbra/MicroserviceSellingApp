namespace EventBus.Base
{
    public class SubscriptionInfo
    {
        public Type HandlerType { get;  }

        public SubscriptionInfo(Type handlerType)
        {
            HandlerType = handlerType;
        }


        public static SubscriptionInfo Typed(Type handlertype)
        {
            return new SubscriptionInfo(handlertype);
        }
    }
}
