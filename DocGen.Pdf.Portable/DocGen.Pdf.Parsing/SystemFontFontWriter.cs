using System;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Parsing;

internal class SystemFontFontWriter : IDisposable
{
	private readonly BinaryWriter writer;

	private readonly Stream stream;

	public SystemFontFontWriter()
	{
		stream = new MemoryStream();
		writer = new BinaryWriter(stream);
	}

	private void WriteBE(byte[] buffer, int count)
	{
		for (int num = count - 1; num >= 0; num--)
		{
			Write(buffer[num]);
		}
	}

	public void Write(byte b)
	{
		writer.Write(b);
	}

	public void WriteChar(sbyte ch)
	{
		Write((byte)ch);
	}

	public void WriteUShort(ushort us)
	{
		WriteBE(BitConverter.GetBytes(us), 2);
	}

	public void WriteShort(short s)
	{
		WriteBE(BitConverter.GetBytes(s), 2);
	}

	public void WriteULong(uint ul)
	{
		WriteBE(BitConverter.GetBytes(ul), 4);
	}

	public void WriteLong(int l)
	{
		WriteBE(BitConverter.GetBytes(l), 4);
	}

	public void WriteString(string str)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		Write((byte)bytes.Length);
		for (int i = 0; i < bytes.Length; i++)
		{
			Write(bytes[i]);
		}
	}

	public byte[] GetBytes()
	{
		return SystemFontExtensions.ReadAllBytes(stream);
	}

	public void Dispose()
	{
		writer.Flush();
		stream.Dispose();
		writer.Dispose();
	}
}
