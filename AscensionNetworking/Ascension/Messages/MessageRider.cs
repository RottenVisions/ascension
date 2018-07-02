using System;
using UnityEngine;
using System.Collections;

namespace Ascension.Networking
{
    public interface IMessageRider
    {
        void Read(Packet packet);
        void Write(Packet packet);
    }

    static class MessageRiderUtils
    {
        static byte[] tempBytes;
        static Packet tempPacket;

        public static byte[] ToByteArray(this IMessageRider token)
        {
            if (token == null)
            {
                return null;
            }

            if ((tempBytes == null) || (tempBytes.Length != (Core.Config.packetSize - 256)))
            {
                tempBytes = new byte[Core.Config.packetSize - 256];
            }

            if (tempPacket == null)
            {
                tempPacket = new Packet();
            }

            // clear data
            Array.Clear(tempBytes, 0, tempBytes.Length);

            // setup packet 
            tempPacket.Position = 0;
            tempPacket.ByteBuffer = tempBytes;
            tempPacket.Size = tempBytes.Length << 3;
            tempPacket.WriteByte(Factory.GetTokenId(token));

            // write token
            token.Write(tempPacket);

            return tempPacket.DuplicateData();
        }

        public static IMessageRider ToToken(this byte[] bytes)
        {
            if ((bytes == null) || (bytes.Length == 0))
            {
                return null;
            }

            if (tempPacket == null)
            {
                tempPacket = new Packet();
            }

            // setup packet
            tempPacket.Position = 8;
            tempPacket.ByteBuffer = bytes;
            tempPacket.Size = bytes.Length << 3;

            IMessageRider token;
            token = Factory.NewToken(bytes[0]);
            token.Read(tempPacket);

            return token;
        }

        public static void WriteToken(this Packet packet, IMessageRider token)
        {
            if (packet.WriteBool(token != null))
            {
                // write header byte
                packet.WriteByte(Factory.GetTokenId(token));

                // write token
                token.Write(packet);
            }
        }

        public static IMessageRider ReadToken(this Packet packet)
        {
            IMessageRider token = null;

            if (packet.ReadBool())
            {
                token = Factory.NewToken(packet.ReadByte());
                token.Read(packet);
            }

            return token;
        }

        public static void SerializeToken(this Packet packet, ref IMessageRider token)
        {
            if (packet.Write)
            {
                packet.WriteToken(token);
            }
            else
            {
                token = packet.ReadToken();
            }
        }
    }
}
