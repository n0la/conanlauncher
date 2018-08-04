using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace conanlauncher
{
    public class SourceServerInfo
    {
        public byte ProtocolVersion;
        public string Name;
        public string Map;
        public string Folder;
        public string Game;
        public int GameID;
        public byte Players;
        public byte MaxPlayers;
        public byte Bots;
        public byte ServerType;
        public byte Environment;
        public byte Visibility;
        public byte VAC;
        public string Version;
        public string ServerVersion;

        public ushort GamePort;
        public UInt64 ServerID;
    }
    public class RconQuery
    {
        public static byte A2S_INFO = 0x54;
        public static byte A2S_INFO_REPLY = 0x49;

        public static byte A2S_RULES = 0x56;

        public static byte A2S_RULES_REPLY = 0x41;

        public static byte A2S_RULES_CHALLENGE_REPLY = 0x45;

        private string hostname;
        private string queryport;

        public RconQuery(string hostname, string queryport)
        {
            this.hostname = hostname;
            this.queryport = queryport;
        }

        private string ReadCString(BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();
            byte data;

            while ((data = reader.ReadByte()) != 0x00) {
                bytes.Add(data);
            }

            return ASCIIEncoding.ASCII.GetString(bytes.ToArray());
        }

        private byte[] SendQuery(byte command, byte[] payload)
        {
            byte[] reply = null;

            using (UdpClient client = new UdpClient(hostname, int.Parse(queryport))) {
                byte[] data = null;
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                Stream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write((UInt32)0xFFFFFFFF);
                writer.Write((byte)command);
                writer.Write(payload);

                stream.Seek(0, SeekOrigin.Begin);
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);       

                client.Send(data, data.Length);
                reply = client.Receive(ref endpoint);
            }

            return reply;
        }
        public SourceServerInfo QueryInfo()
        {
            byte[] data = SendQuery(RconQuery.A2S_INFO, Encoding.ASCII.GetBytes("Source Engine Query\0"));

            if (data == null || data.Length == 0) {
                throw new ApplicationException("no query information returned");
            }

            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            SourceServerInfo info = new SourceServerInfo();

            // discard header  
            reader.ReadUInt32();
            if (reader.ReadByte() != A2S_INFO_REPLY) {
                throw new ApplicationException("invalid reply to INFO query");
            }

            info.ProtocolVersion = reader.ReadByte();
            info.Name = ReadCString(reader);
            info.Map = ReadCString(reader);
            info.Folder = ReadCString(reader);
            info.Game = ReadCString(reader);
            info.GameID = reader.ReadUInt16();
            info.Players = reader.ReadByte();
            info.MaxPlayers = reader.ReadByte();
            info.Bots = reader.ReadByte();
            info.ServerType = reader.ReadByte();
            info.Environment = reader.ReadByte();
            info.Visibility = reader.ReadByte();
            info.VAC = reader.ReadByte();
            info.Version = ReadCString(reader);
            byte extradata = reader.ReadByte();
            if ((extradata & 0x80) == 0x80) {
                info.GamePort = reader.ReadUInt16();
            }
            if ((extradata & 0x10) == 0x10) {
                info.ServerID = reader.ReadUInt64();
            }
            info.ServerVersion = ReadCString(reader);

            return info;
        }

        private byte[] QueryChallengeNumber()
        {
            byte[] challenge = new byte[]{0x00, 0x00, 0x00, 0x00};
            byte[] data = SendQuery(A2S_RULES, challenge);

            if (data == null || data.Length == 0) {
                throw new ApplicationException("no data returned to RULE query");
            }

            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            reader.ReadUInt32();
            if (reader.ReadByte() != A2S_RULES_REPLY) {
                throw new ApplicationException("invalid reply to RULE challenge query");
            }
            challenge = reader.ReadBytes(4);

            return challenge;
        }

        public Dictionary<string, string> QueryRules()
        {
            byte[] challenge = QueryChallengeNumber();
            byte[] data = SendQuery(A2S_RULES, challenge);

            if (data == null || data.Length == 0)
            {
                throw new ApplicationException("no data returned to RULE query");
            }

            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            // Discard header
            reader.ReadUInt32();
            if (reader.ReadByte() != A2S_RULES_CHALLENGE_REPLY)
            {
                throw new ApplicationException("invalid reply to RULE query");
            }

            int rulecount = reader.ReadUInt16();
            Dictionary<string, string> dict = new Dictionary<string, string>();

            for (int i = 0; i < rulecount; i++)
            {
                string name = ReadCString(reader);
                string value = ReadCString(reader);

                dict.Add(name, value);
            }

            return dict;
        }

        public static int QueryServerInfo(string host, string gameid)
        {
            byte[] reply = null;
            Stream stream = null;

            using (UdpClient client = new UdpClient("hl2master.steampowered.com", 27011)) {
                byte[] data = null;
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write((byte)0x31); // Find servers
                writer.Write((byte)0xFF); // Rest of the world
                writer.Write(ASCIIEncoding.ASCII.GetBytes("0.0.0.0:0\0"));
                string filter = String.Format("\\appid\\{0}\\gameaddr\\{1}\0", gameid, host);
                writer.Write(ASCIIEncoding.ASCII.GetBytes(filter));

                stream.Seek(0, SeekOrigin.Begin);
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);       

                client.Send(data, data.Length);
                reply = client.Receive(ref endpoint);
            }

            stream = new MemoryStream(reply);
            BinaryReader reader = new BinaryReader(stream);

            if (reader.ReadUInt32() != 0xFFFFFFFF) {
                throw new ApplicationException("invalid global query response");
            }

            if (reader.ReadByte() != 0x66) {
                throw new ApplicationException("invalid global query response");
            }

            if (reader.ReadByte() != 0x0A) {
                throw new ApplicationException("invalid global query response");
            }

            byte[] ip = null;
            short port = 0;

            ip = reader.ReadBytes(4);
            IPAddress addr = new IPAddress(ip);

            port = reader.ReadInt16();
            port = IPAddress.NetworkToHostOrder(port);

            return port;
        }
    }
}