namespace DocGen.Pdf.Graphics;

internal class DataType
{
	internal static readonly DataType Int8Unsigned = new DataType("BYTE", DataTypeID.Int8Unsigned, 1);

	internal static readonly DataType String = new DataType("STRING", DataTypeID.String, 1);

	internal static readonly DataType Int16Unsigned = new DataType("USHORT", DataTypeID.Int16Unsigned, 2);

	internal static readonly DataType Int32Unsigned = new DataType("ULONG", DataTypeID.Int32Unsigned, 4);

	internal static readonly DataType RationalUnsigned = new DataType("URATIONAL", DataTypeID.RationalUnsigned, 8);

	internal static readonly DataType Int8Signed = new DataType("SBYTE", DataTypeID.Int8Signed, 1);

	internal static readonly DataType Undefined = new DataType("UNDEFINED", DataTypeID.Undefined, 1);

	internal static readonly DataType Int16Signed = new DataType("SSHORT", DataTypeID.Int16Signed, 2);

	internal static readonly DataType Int32Signed = new DataType("SLONG", DataTypeID.Int32Signed, 4);

	internal static readonly DataType RationalSigned = new DataType("SRATIONAL", DataTypeID.RationalSigned, 8);

	internal static readonly DataType Single = new DataType("SINGLE", DataTypeID.Single, 4);

	internal static readonly DataType Double = new DataType("DOUBLE", DataTypeID.Double, 8);

	private string m_name;

	private int m_size;

	private DataTypeID m_type;

	internal string Name => m_name;

	internal int Size => m_size;

	internal DataTypeID Type => m_type;

	internal static DataType FromTiffFormatCode(DataTypeID type)
	{
		return type switch
		{
			DataTypeID.Int8Unsigned => Int8Unsigned, 
			DataTypeID.String => String, 
			DataTypeID.Int16Unsigned => Int16Unsigned, 
			DataTypeID.Int32Unsigned => Int32Unsigned, 
			DataTypeID.RationalUnsigned => RationalUnsigned, 
			DataTypeID.Int8Signed => Int8Signed, 
			DataTypeID.Undefined => Undefined, 
			DataTypeID.Int16Signed => Int16Signed, 
			DataTypeID.Int32Signed => Int32Signed, 
			DataTypeID.RationalSigned => RationalSigned, 
			DataTypeID.Single => Single, 
			DataTypeID.Double => Double, 
			_ => null, 
		};
	}

	private DataType(string name, DataTypeID type, int size)
	{
		m_name = name;
		m_type = type;
		m_size = size;
	}

	public override string ToString()
	{
		return Name;
	}
}
