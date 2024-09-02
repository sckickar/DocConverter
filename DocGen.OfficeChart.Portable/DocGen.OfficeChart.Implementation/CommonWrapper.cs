using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class CommonWrapper : IOptimizedUpdate, ICloneParent
{
	private int m_iBeginCount;

	protected int BeginCallsCount => m_iBeginCount;

	public virtual void BeginUpdate()
	{
		m_iBeginCount++;
	}

	public virtual void EndUpdate()
	{
		if (m_iBeginCount > 0)
		{
			m_iBeginCount--;
		}
	}

	public virtual object Clone(object parent)
	{
		return MemberwiseClone();
	}
}
