namespace DocGen.Drawing;

internal class PreservationLogger
{
	private PreservedFlag m_flag;

	internal PreservationLogger()
	{
		m_flag = (PreservedFlag)0;
	}

	internal bool CheckFlag(PreservedFlag flag)
	{
		return (m_flag & flag) != 0;
	}

	internal void SetFlag(PreservedFlag flag)
	{
		m_flag |= flag;
	}

	internal void ResetFlag()
	{
		m_flag = (PreservedFlag)0;
	}

	internal bool GetPreservedItem(PreservedFlag flag)
	{
		return flag switch
		{
			PreservedFlag.Fill => CheckFlag(PreservedFlag.Fill), 
			PreservedFlag.Line => CheckFlag(PreservedFlag.Line), 
			PreservedFlag.RichText => CheckFlag(PreservedFlag.RichText), 
			_ => false, 
		};
	}
}
