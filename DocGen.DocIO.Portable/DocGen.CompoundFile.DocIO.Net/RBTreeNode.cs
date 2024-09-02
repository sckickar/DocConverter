namespace DocGen.CompoundFile.DocIO.Net;

internal class RBTreeNode
{
	private RBTreeNode m_left;

	private RBTreeNode m_right;

	private RBTreeNode m_parent;

	private NodeColor m_color;

	private bool m_bIsNil;

	private object m_key;

	private object m_value;

	public RBTreeNode Left
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = value;
		}
	}

	public RBTreeNode Right
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = value;
		}
	}

	public RBTreeNode Parent
	{
		get
		{
			return m_parent;
		}
		set
		{
			m_parent = value;
		}
	}

	public NodeColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	public bool IsNil
	{
		get
		{
			return m_bIsNil;
		}
		set
		{
			m_bIsNil = value;
		}
	}

	public object Key
	{
		get
		{
			return m_key;
		}
		set
		{
			m_key = value;
		}
	}

	public object Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	public bool IsRed => Color == NodeColor.Red;

	public bool IsBlack => Color == NodeColor.Black;

	public RBTreeNode(RBTreeNode left, RBTreeNode parent, RBTreeNode right, object key, object value)
		: this(left, parent, right, key, value, NodeColor.Red)
	{
	}

	public RBTreeNode(RBTreeNode left, RBTreeNode parent, RBTreeNode right, object key, object value, NodeColor color)
	{
		m_left = left;
		m_parent = parent;
		m_right = right;
		m_key = key;
		m_value = value;
		m_color = color;
	}
}
