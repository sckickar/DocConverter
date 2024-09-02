namespace DocGen.Pdf;

internal interface BinaryDataInput
{
	int ByteOrdering { get; }

	byte readByte();

	byte readUnsignedByte();

	short readShort();

	int readUnsignedShort();

	int readInt();

	long readUnsignedInt();

	long readLong();

	float readFloat();

	double readDouble();

	int skipBytes(int n);
}
