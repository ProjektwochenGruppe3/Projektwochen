using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace dcs.core
{
    public static class Networking
    {
        public static BinaryFormatter formatter = new BinaryFormatter();

        public static void SendPackage(object pack, NetworkStream stream)
        {
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, pack);
            byte[] data = ms.ToArray();

            int dataLength = data.Length;
            byte[] length = BitConverter.GetBytes(dataLength);

            byte[] fullData = new byte[sizeof(int) + dataLength];

            Array.Copy(length, 0, fullData, 0, length.Length);
            Array.Copy(data, 0, fullData, length.Length, data.Length);

            stream.Write(fullData, 0, fullData.Length);
            stream.Flush();

            ms.Close();
        }

        public static object RecievePackage(NetworkStream netstream)
        {
            byte[] length = new byte[sizeof(int)];

            try
            {
                BinaryReader reader = new BinaryReader(netstream);

                length = reader.ReadBytes(sizeof(int));
                int streamlength = BitConverter.ToInt32(length, 0);
                byte[] data = new byte[streamlength];

                data = reader.ReadBytes(streamlength);

                MemoryStream memstream = new MemoryStream(data);

                return formatter.Deserialize(memstream);
            }
            catch
            {
                return null;
            }
        }

        public static object RecieveServerPackage(NetworkStream netstream)
        {
            byte[] length = new byte[sizeof(int)];

            try
            {
                BinaryReader reader = new BinaryReader(netstream);
                
                byte code = reader.ReadByte();

                length = reader.ReadBytes(sizeof(int));
                int streamlength = BitConverter.ToInt32(length, 0);
                byte[] data = new byte[streamlength];

                data = reader.ReadBytes(streamlength);

                MemoryStream memstream = new MemoryStream(data);

                return formatter.Deserialize(memstream);
            }
            catch
            {
                return null;
            }
        }
    }
}
