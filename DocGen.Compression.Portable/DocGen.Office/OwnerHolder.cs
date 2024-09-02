namespace DocGen.Office;

internal abstract class OwnerHolder : IOfficeMathEntity
{
	internal IOfficeMathEntity m_owner;

	public IOfficeMathEntity OwnerMathEntity => m_owner;

	internal OwnerHolder(IOfficeMathEntity owner)
	{
		m_owner = owner;
	}

	internal void SetOwner(IOfficeMathEntity owner)
	{
		m_owner = owner;
	}

	internal virtual void Close()
	{
		m_owner = null;
	}
}
