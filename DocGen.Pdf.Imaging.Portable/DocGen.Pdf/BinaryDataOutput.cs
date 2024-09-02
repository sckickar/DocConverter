namespace DocGen.Pdf;

internal interface BinaryDataOutput
{
	int ByteOrdering { get; }

	void writeByte(int v);

	void writeShort(int v);

	void writeInt(int v);

	void writeLong(long v);

	void writeFloat(float v);

	void writeDouble(double v);

	void flush();
}
