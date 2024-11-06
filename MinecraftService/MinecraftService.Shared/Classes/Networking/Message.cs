using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Classes.Networking {
    public struct Message : IEquatable<Message> {
        public byte[] Data { get; set; }
        public byte ServerIndex { get; set; }
        public MessageTypes Type { get; set; }
        public MessageFlags Flag { get; set; }
        
        public Message() {
            Data = [];
            ServerIndex = 0;
        }

        public Message(byte[] data) {
            try {
                this = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(data));
            } catch (Exception ex) {
                throw new Exception("Error creating message from byte array!");
            }
        }

        public Message(byte[] data, byte serverIndex, MessageTypes messageType) {
            Data = data;
            ServerIndex = serverIndex;
            Type = messageType;
        }

        public byte[] GetMessageBytes() {
            byte[] jsonData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
            byte[] packetData = new byte[jsonData.Length + sizeof(int)];
            Buffer.BlockCopy(BitConverter.GetBytes(jsonData.Length), 0, packetData, 0, sizeof(int));
            Buffer.BlockCopy(jsonData, 0, packetData, sizeof(int), jsonData.Length);
            return packetData;
        }

        public readonly string GetDataString() => Encoding.UTF8.GetString(Data);

        public static Message Empty(MessageTypes type) => new() { Type = type };
        public static Message Empty() => new();
        
        public static readonly Message EmptyUICallback = new() { Type = MessageTypes.UICallback };
        public static readonly Message Heartbeat = new() { Type = MessageTypes.Heartbeat };
        public static readonly Message Connect = new() { Type = MessageTypes.Connect };

        public override bool Equals(object obj) {
            return obj is Message message && Equals(message);
        }

        public bool Equals(Message other) {
            return EqualityComparer<byte[]>.Default.Equals(Data, other.Data) &&
                   ServerIndex == other.ServerIndex &&
                   Type == other.Type &&
                   Flag == other.Flag;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Data, ServerIndex, Type, Flag);
        }

        public static bool operator ==(Message left, Message right) {
            return left.Equals(right);
        }

        public static bool operator !=(Message left, Message right) {
            return !(left == right);
        }
    }
}
