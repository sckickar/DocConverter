namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class BooleanStyle : DataStyle
{
	private bool m_boolean;

	internal bool Boolean
	{
		get
		{
			return m_boolean;
		}
		set
		{
			m_boolean = value;
		}
	}
}
