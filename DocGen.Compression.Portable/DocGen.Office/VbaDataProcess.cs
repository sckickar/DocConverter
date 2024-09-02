using System;
using System.IO;

namespace DocGen.Office;

internal class VbaDataProcess
{
	internal static Stream Compress(Stream decompressed)
	{
		byte[] array = new byte[decompressed.Length];
		decompressed.Position = 0L;
		decompressed.Read(array, 0, (int)decompressed.Length);
		MemoryStream memoryStream = new MemoryStream();
		int num = 0;
		int num2 = (int)decompressed.Length;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		memoryStream.WriteByte(1);
		num3++;
		while (num < num2)
		{
			num4 = num3;
			num5 = num;
			int num6 = num4 + 4098;
			num3 = num4 + 2;
			int num7 = Math.Min(num5 + 4096, num2);
			while (num < num7 && num3 < num6)
			{
				int num8 = num3;
				byte b = 0;
				num3++;
				for (int i = 0; i <= 7; i++)
				{
					if (num >= num7 || num3 >= num6)
					{
						continue;
					}
					ushort num9 = 0;
					int num10 = num - 1;
					int num11 = 0;
					int num12 = 0;
					while (num10 >= num5)
					{
						int num13 = num10;
						int j = num;
						int num14 = 0;
						for (; j < num7 && array[num13] == array[j]; j++)
						{
							num14++;
							num13++;
						}
						if (num14 > num11)
						{
							num11 = num14;
							num12 = num10;
						}
						num10--;
					}
					ushort num15 = 0;
					if (num11 >= 3)
					{
						double num16 = Math.Log(num - num5, 2.0);
						ushort val = (ushort)((num16 % 1.0 == 0.0) ? num16 : ((double)(int)(ushort)(Math.Floor(num16) + 1.0)));
						val = Math.Max(val, (ushort)4);
						ushort val2 = (ushort)((ushort)(65535 >> (int)val) + 3);
						num15 = (ushort)Math.Min(num11, val2);
						num9 = (ushort)(num - num12);
					}
					else
					{
						num15 = 0;
						num9 = 0;
					}
					if (num9 != 0)
					{
						if (num3 + 1 < num6)
						{
							double num17 = Math.Log(num - num5, 2.0);
							ushort val3 = (ushort)((num17 % 1.0 == 0.0) ? num17 : ((double)(int)(ushort)(Math.Floor(num17) + 1.0)));
							val3 = Math.Max(val3, (ushort)4);
							_ = (ushort)(65535 >> (int)val3);
							byte[] bytes = BitConverter.GetBytes((ushort)(((ushort)(num9 - 1) << (int)(ushort)(16 - val3)) | (ushort)(num15 - 3)));
							memoryStream.Position = num3;
							memoryStream.Write(bytes, 0, 2);
							b = (byte)((b & ~(1 << i)) | (1 << i));
							num3 += 2;
							num += num15;
						}
						else
						{
							num3 = num6;
						}
					}
					else if (num3 < num6)
					{
						memoryStream.Position = num3;
						memoryStream.WriteByte(array[num]);
						num3++;
						num++;
					}
					else
					{
						num3 = num6;
					}
				}
				memoryStream.Position = num8;
				memoryStream.WriteByte(b);
			}
			int num18 = 0;
			if (num < num7)
			{
				int num19 = num7 - 1;
				num3 = num4 + 2;
				num = num5;
				int num20 = 4096;
				for (int k = num5; k <= num19; k++)
				{
					memoryStream.Position = num3;
					memoryStream.WriteByte(array[k]);
					num3++;
					num++;
					num20--;
				}
				for (int l = 1; l <= num20; l++)
				{
					memoryStream.Position = num3;
					memoryStream.WriteByte(0);
					num3++;
				}
				num18 = 0;
			}
			else
			{
				num18 = 1;
			}
			int num21 = num3 - num4;
			ushort value = (ushort)(((ushort)(((ushort)(0 | (num21 - 3)) & 0x7FFF) | (num18 << 15)) & 0x8FFF) | 0x3000);
			memoryStream.Position = num4;
			byte[] bytes2 = BitConverter.GetBytes(value);
			memoryStream.Write(bytes2, 0, bytes2.Length);
			memoryStream.Position = num3;
		}
		return memoryStream;
	}

	internal static Stream Decompress(Stream compressed)
	{
		byte[] array = new byte[compressed.Length];
		compressed.Position = 0L;
		compressed.Read(array, 0, (int)compressed.Length);
		MemoryStream memoryStream = new MemoryStream(4096);
		int num = array.Length;
		int num2 = 0;
		long num3 = memoryStream.Position;
		int num4 = 0;
		int num5 = 0;
		if (array[0] == 1)
		{
			num2++;
			while (num2 < num)
			{
				num4 = num2;
				ushort num6 = BitConverter.ToUInt16(array, num4);
				int num7 = (num6 & 0xFFF) + 3;
				int num8 = (num6 & 0x8000) >> 15;
				num5 = (int)num3;
				int num9 = Math.Min(num, num4 + num7);
				num2 = num4 + 2;
				if (num8 == 1)
				{
					while (num2 < num9)
					{
						byte b = array[num2];
						num2++;
						if (num2 >= num9)
						{
							continue;
						}
						for (int i = 0; i <= 7; i++)
						{
							if (num2 >= num9)
							{
								continue;
							}
							if (((b >> i) & 1) == 0)
							{
								memoryStream.Position = num3;
								memoryStream.WriteByte(array[num2]);
								num3++;
								num2++;
								continue;
							}
							ushort num10 = BitConverter.ToUInt16(array, num2);
							double num11 = Math.Log((int)num3 - num5, 2.0);
							ushort val = (ushort)((num11 % 1.0 == 0.0) ? num11 : ((double)(int)(ushort)(Math.Floor(num11) + 1.0)));
							val = Math.Max(val, (ushort)4);
							ushort num12 = (ushort)(65535 >> (int)val);
							ushort num13 = (ushort)(~num12);
							ushort num14 = (ushort)((num10 & num12) + 3);
							ushort num15 = (ushort)(((num10 & num13) >> 16 - val) + 1);
							int num16 = (int)num3 - num15;
							int num17 = (int)num3;
							for (int j = 1; j <= num14; j++)
							{
								memoryStream.Position = num17;
								memoryStream.WriteByte(memoryStream.ToArray()[num16]);
								num17++;
								num16++;
							}
							num3 += num14;
							num2 += 2;
						}
					}
				}
				else
				{
					MemoryStream memoryStream2 = new MemoryStream(4096);
					memoryStream2.Write(array, 0, array.Length);
					memoryStream2.WriteTo(memoryStream);
					num3 += 4096;
					num2 += 4096;
				}
			}
			return memoryStream;
		}
		throw new Exception("Stream is corrupted");
	}

	internal static byte[] Decrypt(byte[] data)
	{
		byte num = data[0];
		byte b = data[1];
		byte b2 = data[2];
		byte b3 = (byte)(num ^ b2);
		byte b4 = b2;
		byte b5 = b;
		int num2 = (num & 6) / 2;
		byte[] array = new byte[num2];
		int num3 = 0;
		int num4 = 3;
		while (num4 < num2 + 3)
		{
			array[num3] = data[num4];
			num4++;
			num3++;
		}
		byte[] array2 = array;
		foreach (byte b6 in array2)
		{
			byte num5 = (byte)(b6 ^ (b5 + b3));
			b5 = b4;
			b4 = b6;
			b3 = num5;
		}
		byte[] array3 = new byte[4];
		int num6 = 0;
		int num7 = num2 + 3;
		while (num7 < num2 + 7)
		{
			array3[num6] = data[num7];
			num7++;
			num6++;
		}
		byte b7 = 0;
		int num8 = 0;
		array2 = array3;
		foreach (byte num9 in array2)
		{
			byte b8 = (byte)(num9 ^ (b5 + b3));
			double num10 = Math.Pow(256.0, (int)b7);
			num10 *= (double)(int)b8;
			num8 += (int)num10;
			b5 = b4;
			b4 = num9;
			b3 = b8;
			b7++;
		}
		byte[] array4 = new byte[data.Length - (num2 + 7)];
		int num11 = 0;
		int num12 = num2 + 7;
		while (num12 < data.Length)
		{
			array4[num11] = data[num12];
			num12++;
			num11++;
		}
		MemoryStream memoryStream = new MemoryStream();
		array2 = array4;
		foreach (byte num13 in array2)
		{
			byte b9 = (byte)(num13 ^ (b5 + b3));
			memoryStream.WriteByte(b9);
			b5 = b4;
			b4 = num13;
			b3 = b9;
		}
		return memoryStream.ToArray();
	}

	internal static byte[] Encrypt(byte[] data, string clsID)
	{
		byte b = 2;
		int value = data.Length;
		byte b2 = 3;
		byte b3 = (byte)(b2 ^ b);
		byte b4 = 0;
		foreach (char c in clsID)
		{
			b4 += (byte)c;
		}
		byte b5 = (byte)(b2 ^ b4);
		byte b6 = b4;
		byte b7 = b5;
		byte b8 = b3;
		int num = (b2 & 6) / 2;
		byte[] array = new byte[num];
		for (int j = 1; j <= num; j++)
		{
			byte b9 = (array[j - 1] = (byte)(1u ^ (uint)(b8 + b6)));
			b8 = b7;
			b7 = b9;
			b6 = 1;
		}
		byte[] bytes = BitConverter.GetBytes(value);
		byte[] array2 = new byte[4];
		int num2 = 3;
		int num3 = 0;
		while (num2 >= 0)
		{
			byte num4 = bytes[num2];
			byte b10 = (array2[num3] = (byte)(num4 ^ (b8 + b6)));
			b8 = b7;
			b7 = b10;
			b6 = num4;
			num2--;
			num3++;
		}
		MemoryStream memoryStream = new MemoryStream();
		foreach (byte num5 in data)
		{
			byte b11 = (byte)(num5 ^ (b8 + b6));
			memoryStream.WriteByte(b11);
			b8 = b7;
			b7 = b11;
			b6 = num5;
		}
		byte[] array3 = memoryStream.ToArray();
		MemoryStream memoryStream2 = new MemoryStream();
		memoryStream2.WriteByte(b2);
		memoryStream2.WriteByte(b3);
		memoryStream2.WriteByte(b5);
		memoryStream2.Write(array, 0, array.Length);
		memoryStream2.Write(array2, 0, 4);
		memoryStream2.Write(array3, 0, array3.Length);
		return memoryStream2.ToArray();
	}
}
