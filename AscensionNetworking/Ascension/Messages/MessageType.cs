using System;

namespace Ascension.Networking
{
    public class MessageType
    {
        public const short ObjectDestroy = 1;
        public const short Rpc = 2;
        public const short ObjectSpawn = 3;
        public const short Owner = 4;
        public const short Command = 5;
        public const short LocalPlayerTransform = 6;

        public static string[] msgLabels = new string[]
        {
            "none",
            "ObjectDestroy",
            "Rpc",
            "ObjectSpawn",
            "Owner",
            "Command",
            "LocalPlayerTransform",
            "SyncEvent",
            "UpdateVars"
        };

        public static string MsgTypeToString(short value)
        {
            if (value < 0 || value > 46)
            {
                return string.Empty;
            }
            string text = msgLabels[value];
            if (string.IsNullOrEmpty(text))
            {
                text = "[" + value + "]";
            }
            return text;
        }
    }

}
