namespace DocGen.Office;

internal class MatrixColumnProperties : OwnerHolder
{
	private MathHorizontalAlignment m_alignment;

	private int m_count;

	internal int Count
	{
		get
		{
			return m_count;
		}
		set
		{
			m_count = value;
		}
	}

	internal MathHorizontalAlignment Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}

	internal MatrixColumnProperties(IOfficeMathEntity owner)
		: base(owner)
	{
	}

	internal MatrixColumnProperties Clone(IOfficeMathEntity owner)
	{
		MatrixColumnProperties obj = (MatrixColumnProperties)MemberwiseClone();
		obj.SetOwner(owner);
		return obj;
	}

	internal override void Close()
	{
		base.Close();
	}
}
