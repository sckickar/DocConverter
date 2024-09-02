namespace DocGen.DocIO.DLS;

internal class WTableColumn
{
	private float m_preferredwidth;

	internal float m_endOffset;

	private ColumnSizeInfo sizeInfo;

	private bool m_hasMaximumWordWidth;

	internal float PreferredWidth
	{
		get
		{
			return m_preferredwidth;
		}
		set
		{
			m_preferredwidth = value;
		}
	}

	internal float EndOffset
	{
		get
		{
			return m_endOffset;
		}
		set
		{
			m_endOffset = value;
		}
	}

	internal float MinimumWordWidth
	{
		get
		{
			return sizeInfo.MinimumWordWidth;
		}
		set
		{
			sizeInfo.MinimumWordWidth = value;
		}
	}

	internal float MaximumWordWidth
	{
		get
		{
			return sizeInfo.MaximumWordWidth;
		}
		set
		{
			sizeInfo.MaximumWordWidth = value;
		}
	}

	internal float MinimumWidth
	{
		get
		{
			return sizeInfo.MinimumWidth;
		}
		set
		{
			sizeInfo.MinimumWidth = value;
		}
	}

	internal bool HasMaximumWordWidth
	{
		get
		{
			return m_hasMaximumWordWidth;
		}
		set
		{
			m_hasMaximumWordWidth = value;
		}
	}

	internal float MaxParaWidth
	{
		get
		{
			return sizeInfo.MaxParaWidth;
		}
		set
		{
			sizeInfo.MaxParaWidth = value;
		}
	}

	internal WTableColumn()
	{
		sizeInfo = new ColumnSizeInfo();
	}

	internal WTableColumn Clone()
	{
		return new WTableColumn
		{
			PreferredWidth = PreferredWidth,
			MinimumWidth = MinimumWidth,
			MinimumWordWidth = MinimumWordWidth,
			MaximumWordWidth = MaximumWordWidth
		};
	}

	internal void Dispose()
	{
		sizeInfo = null;
	}
}
