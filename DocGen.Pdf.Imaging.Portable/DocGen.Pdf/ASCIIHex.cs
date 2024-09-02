using System;
using System.IO;
using System.Text;

namespace DocGen.Pdf;

internal class ASCIIHex
{
	public byte[] Decode(byte[] data)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		StreamReader streamReader = null;
		MemoryStream memoryStream = null;
		memoryStream = new MemoryStream(data);
		streamReader = new StreamReader(memoryStream);
		if (streamReader != null)
		{
			while (true)
			{
				string text = streamReader.ReadLine();
				if (text == null)
				{
					break;
				}
				stringBuilder2.Append(text);
			}
		}
		if (streamReader != null)
		{
			streamReader.Close();
			memoryStream.Close();
		}
		int length = stringBuilder2.Length;
		int num = 0;
		int num2 = 0;
		MemoryStream memoryStream2 = new MemoryStream();
		do
		{
			char c = stringBuilder2[num];
			if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
			{
				stringBuilder.Append(c);
				if (num2 == 1)
				{
					int num3 = Convert.ToInt32(stringBuilder.ToString(), 16);
					memoryStream2.Write(new byte[4]
					{
						(byte)num3,
						(byte)(num3 >> 8),
						(byte)(num3 >> 16),
						(byte)(num3 >> 24)
					}, 0, 4);
					num2 = 0;
					stringBuilder = new StringBuilder();
				}
				else
				{
					num2++;
				}
			}
			if (c == '>')
			{
				break;
			}
			num++;
		}
		while (num != length);
		if (num2 == 1)
		{
			stringBuilder.Append('0');
			int num4 = Convert.ToInt32(stringBuilder.ToString(), 16);
			memoryStream2.Write(new byte[4]
			{
				(byte)num4,
				(byte)(num4 >> 8),
				(byte)(num4 >> 16),
				(byte)(num4 >> 24)
			}, 0, 4);
		}
		memoryStream2.Close();
		return memoryStream2.GetBuffer();
	}
}
