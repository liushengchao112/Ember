using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Utils
{
	//from UNetPkg
	public sealed class ByteStreamReader : IDisposable
	{
		
		private MemoryStream ms;
		
		public ByteStreamReader(byte[] data)
		{
			ms = new MemoryStream(data);
		}
		
		public ByteStreamReader(byte[] data, int start, int len)
		{
			ms = new MemoryStream(data, start, len);
		}
		
		public byte ReadByte()
		{
			int b = ms.ReadByte();
			return Convert.ToByte(b);
		}
		
		public byte[] ReadBytes(int len)
		{
			byte[] buff = new byte[len];
			ms.Read(buff, 0, len);//(int)ms.Position, len);
			
			return buff;
		}
		
		private byte[] ReadBytesForConvert(int len)
		{
			byte[] buff = new byte[len];
			ms.Read(buff, 0, len);//(int)ms.Position, len);
			if (BitConverter.IsLittleEndian)
         		buff = ReverseBytes(buff);
			
			return buff;
		}
		
		public bool ReadBool()
		{
			return BitConverter.ToBoolean(ReadBytes(1), 0);
		}
		
		public short ReadShort()
		{
//			int s = ms.ReadByte() << 8;
//			s += ms.ReadByte();
//			
//			return (short)Convert.ToUInt16(s);
			
			return BitConverter.ToInt16(ReadBytesForConvert(2), 0);
		}
		
		public int ReadInt()
		{
//			int i = ms.ReadByte() << 24;
//			i += ms.ReadByte() << 16;
//			i += ms.ReadByte() << 8;
//			i += ms.ReadByte();
//			
//			return (int)Convert.ToUInt32(i);
			
			return BitConverter.ToInt32(ReadBytesForConvert(4), 0); 
		}
		
		public long ReadLong()
		{
//			long _l = ms.ReadByte() << 24;
//			_l += ms.ReadByte() << 16;
//			_l += ms.ReadByte() << 8;
//			_l += ms.ReadByte();
//			
//			long l = ms.ReadByte() << 24;
//			l += ms.ReadByte() << 16;
//			l += ms.ReadByte() << 8;
//			l += ms.ReadByte();
//			
//			return (long)Convert.ToInt64(l + (_l<<32));
			
			return BitConverter.ToInt64(ReadBytesForConvert(8), 0); 
		}
		
		public float ReadFloat()
		{
			return BitConverter.ToSingle(ReadBytesForConvert(4), 0);
		}
		
		public double ReadDouble()
		{
			return BitConverter.ToDouble(ReadBytesForConvert(8), 0);
		}
		
		public string ReadUTF8(int len)
		{
			byte[] strByte = new byte[len];
			ms.Read(strByte, 0, len);
			string str = Encoding.UTF8.GetString(strByte);
			return str;
		}
		
		public string ReadUTF8WithLength()
		{
			short len = ReadShort();
			if(len <= 0)
			{
				return null;
			}
			return ReadUTF8(len);
		}
		
//		public float ReadFloatWithLength()
//		{
//			short len = ReadShort();
//			return float.Parse(ReadUTF8(len));
//		}
//		
//		public double ReadDoubleWithLength()
//		{
//			short len = ReadShort();
//			return double.Parse(ReadUTF8(len));
//		}
		
		public void Dispose ()
		{
			ms.Close();
			ms.Dispose();
		}
		
		private static byte[] ReverseBytes(byte[] inArray)
		{
			byte temp;
			int highCtr = inArray.Length - 1;
			
			for (int ctr = 0; ctr < inArray.Length / 2; ctr++)
			{
				temp = inArray[ctr];
				inArray[ctr] = inArray[highCtr];
				inArray[highCtr] = temp;
				highCtr -= 1;
			}
			return inArray;
		}
	}
	
	public sealed class ByteStreamWriter : IDisposable
	{
		private MemoryStream ms;
		
		public ByteStreamWriter()
		{
			ms = new MemoryStream();
		}
		
		public void WriteByte(byte v)
		{
			ms.WriteByte(v);
		}
		
		public void WriteBool(bool v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			if (BitConverter.IsLittleEndian)
         		bytes = ReverseBytes(bytes);
			
			WriteBytes(bytes);
			
			//ms.WriteByte(Convert.ToByte(v));
		}
		
		public void WriteShort(short v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			if (BitConverter.IsLittleEndian)
         		bytes = ReverseBytes(bytes);
			
			WriteBytes(bytes);
			
//			WriteByte(Convert.ToByte(v >> 8));
//			WriteByte(Convert.ToByte(v & 0xFF));
		}
		
		public void WriteShort(int v)
		{
			byte[] bytes = BitConverter.GetBytes((short)v);
			if (BitConverter.IsLittleEndian)
         		bytes = ReverseBytes(bytes);
			
			WriteBytes(bytes);
			
//			WriteByte(Convert.ToByte(v >> 8));
//			WriteByte(Convert.ToByte(v & 0xFF));
		}
		
		public void WriteInt(int v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			if (BitConverter.IsLittleEndian)
         		bytes = ReverseBytes(bytes);
			
			WriteBytes(bytes);
			
//			WriteShort(v >> 16);
//			WriteShort(v & 0xFFFF);
		}
		
		public void WriteFloat(float v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			if (BitConverter.IsLittleEndian)
         		bytes = ReverseBytes(bytes);
			
			WriteBytes(bytes);
		}
		
		public void WriteDouble(double v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			if (BitConverter.IsLittleEndian)
         		bytes = ReverseBytes(bytes);
			
			WriteBytes(bytes);
		}
		
		public void WriteLong(long v)
		{
			byte[] bytes = BitConverter.GetBytes(v);
			if (BitConverter.IsLittleEndian)
         		bytes = ReverseBytes(bytes);
			
			WriteBytes(bytes);
			
//			WriteByte(Convert.ToByte(v >> 56));
//			WriteByte(Convert.ToByte(v >> 48));
//			WriteByte(Convert.ToByte(v >> 40));
//			WriteByte(Convert.ToByte(v >> 32));
//			
//			WriteByte(Convert.ToByte(v >> 24));
//			WriteByte(Convert.ToByte(v >> 16));
//			WriteByte(Convert.ToByte(v >> 8));
//			WriteByte(Convert.ToByte(v & 0xFF));
		}
		
		public void WriteBytes(byte[] s, int len)
		{
			ms.Write(s, 0, len);
		}
		
		public void WriteUTF8(string s)
		{
			byte[] sb = Encoding.UTF8.GetBytes(s);
			int len = Encoding.UTF8.GetByteCount(s);
			ms.Write(sb, 0, len);
		}
		
		public void WriteUTF8WithLength(string s)
		{
			if(string.IsNullOrEmpty(s))
			{
				WriteShort(0);
			}else{
				short len = (short)Encoding.UTF8.GetByteCount(s);
				WriteShort(len);
				WriteUTF8(s);
			}
		}
		
//		public void WriteFloatWithLength(float v)
//		{
//			string s = v.ToString();
//			short len = (short)Encoding.UTF8.GetByteCount(s);
//			WriteShort(len);
//			WriteUTF8(s);
//		}
//		
//		public void WriteDoubleWithLength(double v)
//		{
//			string s = v.ToString();
//			short len = (short)Encoding.UTF8.GetByteCount(s);
//			WriteShort(len);
//			WriteUTF8(s);
//		}
		
		public void WriteBytes(byte[] v)
		{
			ms.Write(v, 0, v.Length);
		}
		
		public byte[] GetBuffer()
		{
			byte[] buff = new byte[ms.Length];
			long pos = ms.Position;
			ms.Seek(0, SeekOrigin.Begin);
			ms.Read(buff, 0, (int)ms.Length);
			ms.Seek(pos, SeekOrigin.Begin);
			return buff;
		}
		
		public void Dispose ()
		{
			ms.Close();
			ms.Dispose();
		}
		
		private static byte[] ReverseBytes(byte[] inArray)
		{
			byte temp;
			int highCtr = inArray.Length - 1;
			
			for (int ctr = 0; ctr < inArray.Length / 2; ctr++)
			{
				temp = inArray[ctr];
				inArray[ctr] = inArray[highCtr];
				inArray[highCtr] = temp;
				highCtr -= 1;
			}
			return inArray;
		}
	}
}