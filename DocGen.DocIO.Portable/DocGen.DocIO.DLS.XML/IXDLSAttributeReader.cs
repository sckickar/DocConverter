using System;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS.XML;

public interface IXDLSAttributeReader
{
	bool HasAttribute(string name);

	string ReadString(string name);

	int ReadInt(string name);

	short ReadShort(string name);

	float ReadFloat(string name);

	bool ReadBoolean(string name);

	byte ReadByte(string name);

	Enum ReadEnum(string name, Type enumType);

	Color ReadColor(string name);
}
