namespace Identidade.Infraestrutura.RedisNotifier
{
    public static class RedisConstants
    {
        public static class Path
        {
            public const string REDIS_MESSAGE_ACTIVE = "MSG:ACTIVE:";
            public const string REDIS_IDENTITYCONSUMERS = "IDENTITYCONSUMERS:";
            public const string REDIS_IDENTITYRESTAPI = "IDENTITYRESTAPI:";
        }

        public static class Field
        {
            public const string REDIS_FIELD_STATUS = "STATUS";
            public const string REDIS_FIELD_MACHINE = "MACHINE";
            public const string REDIS_FIELD_STATUS_TIME = "STATUSTIME";
            public const string REDIS_FIELD_IDENTITYCONSUMERID = "IDENTITYCONSUMERID";
            public const string REDIS_FIELD_IDENTITYRESTAPIID = "IDENTITYRESTAPIID";
            public const string REDIS_FIELD_LASTALIVE = "LASTALIVE";
            public const string REDIS_FIELD_DBNUMBER = "DBNUMBER";
        }

        public static class Status
        {
            public const string REDIS_STATUS_STARTING = "STARTING";
            public const string REDIS_STATUS_IDLE = "IDLE";
            public const string REDIS_STATUS_WORKING = "WORKING";
        }
    }
}
