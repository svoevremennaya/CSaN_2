using System;
using System.Collections.Generic;
using System.Text;

namespace tracert_utility
{
    public class ICMP
    {
        public byte type;
        public byte code;
        public ushort checksum;
        public ushort id;
        public ushort sn;
        public int data_size;
        public byte[] data = new byte[1024];

        public ICMP()
        {
            type = 8;
            code = 0;
        }

        public ICMP(byte[] message, int size)
        {
            type = message[20];
            code = message[21];
            checksum = BitConverter.ToUInt16(message, 22);
            id = BitConverter.ToUInt16(message, 24);
            sn = BitConverter.ToUInt16(message, 26);
            data_size = size - 28;
            Buffer.BlockCopy(message, 28, data, 0, data_size);
        }

        public byte[] ICMPToBytes()
        {
            byte[] byte_data = new byte[data_size + 8];
            Buffer.BlockCopy(BitConverter.GetBytes(type), 0, byte_data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, byte_data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, byte_data, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(id), 0, byte_data, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(sn), 0, byte_data, 6, 2);
            Buffer.BlockCopy(data, 0, byte_data, 8, data_size);
            return byte_data;
        }

        public void GetCheckSum()
        {
            long checkSum = 0;
            byte[] data = ICMPToBytes();
            int index = 0, count = data_size;

            while (count > 1)
            {
                checkSum += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                count -= 2;
                index += 2;
            }
            if (count > 0)
                checkSum += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
            while (checkSum >> 16 != 0)
                checkSum = (checkSum & 0xffff) + (checkSum >> 16);

            checksum = (ushort)(~checkSum);
        }

        public void Sequence()
        {
            sn++;
            checksum = 0;
            GetCheckSum();
        }
    }
}
