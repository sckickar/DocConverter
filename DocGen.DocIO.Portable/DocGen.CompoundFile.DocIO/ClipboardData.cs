using System.IO;
using DocGen.CompoundFile.DocIO.Net;

namespace DocGen.CompoundFile.DocIO;

public class ClipboardData : ICloneable
{
	public int Format;

	public byte[] Data;

	public object Clone()
	{
		ClipboardData obj = (ClipboardData)MemberwiseClone();
		obj.Data = CloneUtils.CloneByteArray(Data);
		return obj;
	}

	public int Serialize(Stream stream)
	{
		int num = Data.Length;
		int num2 = 0 + StreamHelper.WriteInt32(stream, num) + StreamHelper.WriteInt32(stream, Format);
		stream.Write(Data, 0, num);
		return num2 + num;
	}

	public void Parse(Stream stream)
	{
		byte[] buffer = new byte[4];
		int num = StreamHelper.ReadInt32(stream, buffer);
		Format = StreamHelper.ReadInt32(stream, buffer);
		Data = new byte[num];
		stream.Read(Data, 0, num);
	}
}
