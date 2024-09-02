using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.CompoundFile.DocIO.Net;

internal class MapCollection : IEnumerable
{
	internal delegate void NodeFunction(RBTreeNode node);

	private RBTreeNode m_MyHead;

	private int m_size;

	private IComparer m_comparer = Comparer<object>.Default;

	public RBTreeNode Empty => m_MyHead;

	public int Count => m_size;

	public object this[object key]
	{
		get
		{
			RBTreeNode rBTreeNode = LBound(key);
			if (m_comparer.Compare(rBTreeNode.Key, key) == 0)
			{
				return rBTreeNode.Value;
			}
			return null;
		}
		set
		{
			RBTreeNode rBTreeNode = LBound(key);
			if (rBTreeNode.IsNil || m_comparer.Compare(rBTreeNode.Key, key) != 0)
			{
				Add(key, value);
			}
			else
			{
				rBTreeNode.Value = value;
			}
		}
	}

	public MapCollection()
	{
		Initialize();
	}

	public MapCollection(IComparer comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		Initialize();
		m_comparer = comparer;
	}

	protected void Initialize()
	{
		m_MyHead = new RBTreeNode(null, null, null, null, null, NodeColor.Black);
		m_MyHead.IsNil = true;
		RBTreeNode myHead = m_MyHead;
		RBTreeNode myHead2 = m_MyHead;
		RBTreeNode rBTreeNode = (m_MyHead.Right = m_MyHead);
		RBTreeNode parent = (myHead2.Left = rBTreeNode);
		myHead.Parent = parent;
		m_size = 0;
	}

	public void Clear()
	{
		Erase(m_MyHead.Parent);
		RBTreeNode myHead = m_MyHead;
		RBTreeNode myHead2 = m_MyHead;
		RBTreeNode rBTreeNode = (m_MyHead.Right = m_MyHead);
		RBTreeNode parent = (myHead2.Left = rBTreeNode);
		myHead.Parent = parent;
		m_size = 0;
	}

	public void Add(object key, object value)
	{
		RBTreeNode rBTreeNode = m_MyHead.Parent;
		RBTreeNode rBTreeNode2 = m_MyHead;
		bool flag = true;
		while (!rBTreeNode.IsNil)
		{
			rBTreeNode2 = rBTreeNode;
			flag = m_comparer.Compare(key, rBTreeNode.Key) < 0;
			rBTreeNode = (flag ? rBTreeNode.Left : rBTreeNode.Right);
		}
		RBTreeNode node = rBTreeNode2;
		if (flag)
		{
			if (rBTreeNode2 == begin())
			{
				Insert(_addLeft: true, rBTreeNode2, key, value);
				return;
			}
			node = Dec(node);
		}
		Insert(flag, rBTreeNode2, key, value);
	}

	public bool Contains(object key)
	{
		RBTreeNode rBTreeNode = LBound(key);
		if (rBTreeNode != m_MyHead)
		{
			return m_comparer.Compare(rBTreeNode.Key, key) == 0;
		}
		return false;
	}

	public void Remove(object key)
	{
		RBTreeNode rBTreeNode = LBound(key);
		if (rBTreeNode.IsNil)
		{
			return;
		}
		RBTreeNode rBTreeNode2 = rBTreeNode;
		RBTreeNode rBTreeNode3;
		if (rBTreeNode2.Left.IsNil)
		{
			rBTreeNode3 = rBTreeNode2.Right;
		}
		else if (rBTreeNode2.Right.IsNil)
		{
			rBTreeNode3 = rBTreeNode2.Left;
		}
		else
		{
			rBTreeNode2 = Inc(rBTreeNode);
			rBTreeNode3 = rBTreeNode2.Right;
		}
		RBTreeNode rBTreeNode4;
		if (rBTreeNode2 == rBTreeNode)
		{
			rBTreeNode4 = rBTreeNode.Parent;
			if (!rBTreeNode3.IsNil)
			{
				rBTreeNode3.Parent = rBTreeNode4;
			}
			if (m_MyHead.Parent == rBTreeNode)
			{
				m_MyHead.Parent = rBTreeNode3;
			}
			else if (rBTreeNode4.Left == rBTreeNode)
			{
				rBTreeNode4.Left = rBTreeNode3;
			}
			else
			{
				rBTreeNode4.Right = rBTreeNode3;
			}
			if (m_MyHead.Left == rBTreeNode)
			{
				m_MyHead.Left = (rBTreeNode3.IsNil ? rBTreeNode4 : Min(rBTreeNode3));
			}
			if (m_MyHead.Right == rBTreeNode)
			{
				m_MyHead.Right = (rBTreeNode3.IsNil ? rBTreeNode4 : Max(rBTreeNode3));
			}
		}
		else
		{
			rBTreeNode.Left.Parent = rBTreeNode2;
			rBTreeNode2.Left = rBTreeNode.Left;
			if (rBTreeNode2 == rBTreeNode.Right)
			{
				rBTreeNode4 = rBTreeNode2;
			}
			else
			{
				rBTreeNode4 = rBTreeNode2.Parent;
				if (!rBTreeNode3.IsNil)
				{
					rBTreeNode3.Parent = rBTreeNode4;
				}
				rBTreeNode4.Left = rBTreeNode3;
				rBTreeNode2.Right = rBTreeNode.Right;
				rBTreeNode.Right.Parent = rBTreeNode2;
			}
			if (m_MyHead.Parent == rBTreeNode)
			{
				m_MyHead.Parent = rBTreeNode2;
			}
			else if (rBTreeNode.Parent.Left == rBTreeNode)
			{
				rBTreeNode.Parent.Left = rBTreeNode2;
			}
			else
			{
				rBTreeNode.Parent.Right = rBTreeNode2;
			}
			rBTreeNode2.Parent = rBTreeNode.Parent;
			NodeColor color = rBTreeNode.Color;
			rBTreeNode.Color = rBTreeNode2.Color;
			rBTreeNode2.Color = color;
		}
		if (rBTreeNode.Color == NodeColor.Black)
		{
			while (rBTreeNode3 != m_MyHead.Parent && rBTreeNode3.Color == NodeColor.Black)
			{
				if (rBTreeNode3 == rBTreeNode4.Left)
				{
					rBTreeNode2 = rBTreeNode4.Right;
					if (rBTreeNode2.Color == NodeColor.Red)
					{
						rBTreeNode2.Color = NodeColor.Black;
						rBTreeNode4.Color = NodeColor.Red;
						LRotate(rBTreeNode4);
						rBTreeNode2 = rBTreeNode4.Right;
					}
					if (rBTreeNode2.IsNil)
					{
						rBTreeNode3 = rBTreeNode4;
					}
					else
					{
						if (rBTreeNode2.Left.Color != NodeColor.Black || rBTreeNode2.Right.Color != NodeColor.Black)
						{
							if (rBTreeNode2.Right.Color == NodeColor.Black)
							{
								rBTreeNode2.Left.Color = NodeColor.Black;
								rBTreeNode2.Color = NodeColor.Red;
								RRotate(rBTreeNode2);
								rBTreeNode2 = rBTreeNode4.Right;
							}
							rBTreeNode2.Color = rBTreeNode4.Color;
							rBTreeNode4.Color = NodeColor.Black;
							rBTreeNode2.Right.Color = NodeColor.Black;
							LRotate(rBTreeNode4);
							break;
						}
						rBTreeNode2.Color = NodeColor.Red;
						rBTreeNode3 = rBTreeNode4;
					}
				}
				else
				{
					rBTreeNode2 = rBTreeNode4.Left;
					if (rBTreeNode2.Color == NodeColor.Red)
					{
						rBTreeNode2.Color = NodeColor.Black;
						rBTreeNode4.Color = NodeColor.Red;
						RRotate(rBTreeNode4);
						rBTreeNode2 = rBTreeNode4.Left;
					}
					if (rBTreeNode2.IsNil)
					{
						rBTreeNode3 = rBTreeNode4;
					}
					else
					{
						if (rBTreeNode2.Right.Color != NodeColor.Black || rBTreeNode2.Left.Color != NodeColor.Black)
						{
							if (rBTreeNode2.Left.Color == NodeColor.Black)
							{
								rBTreeNode2.Right.Color = NodeColor.Black;
								rBTreeNode2.Color = NodeColor.Red;
								LRotate(rBTreeNode2);
								rBTreeNode2 = rBTreeNode4.Left;
							}
							rBTreeNode2.Color = rBTreeNode4.Color;
							rBTreeNode4.Color = NodeColor.Black;
							rBTreeNode2.Left.Color = NodeColor.Black;
							RRotate(rBTreeNode4);
							break;
						}
						rBTreeNode2.Color = NodeColor.Red;
						rBTreeNode3 = rBTreeNode4;
					}
				}
				rBTreeNode4 = rBTreeNode3.Parent;
			}
			rBTreeNode3.Color = NodeColor.Black;
		}
		if (0 < m_size)
		{
			m_size--;
		}
	}

	private RBTreeNode begin()
	{
		return m_MyHead.Left;
	}

	public static RBTreeNode Min(RBTreeNode node)
	{
		while (!node.Left.IsNil)
		{
			node = node.Left;
		}
		return node;
	}

	public static RBTreeNode Max(RBTreeNode node)
	{
		while (!node.Right.IsNil)
		{
			node = node.Right;
		}
		return node;
	}

	public static RBTreeNode Inc(RBTreeNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (node.IsNil)
		{
			return node;
		}
		if (!node.Right.IsNil)
		{
			node = Min(node.Right);
		}
		else
		{
			RBTreeNode parent;
			while (!(parent = node.Parent).IsNil && node == parent.Right)
			{
				node = parent;
			}
			node = parent;
		}
		return node;
	}

	public static RBTreeNode Dec(RBTreeNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (node.IsNil)
		{
			node = node.Right;
		}
		else if (!node.Left.IsNil)
		{
			node = Max(node.Left);
		}
		else
		{
			RBTreeNode parent;
			while (!(parent = node.Parent).IsNil && parent == parent.Left)
			{
				node = parent;
			}
			if (!parent.IsNil)
			{
				node = parent;
			}
		}
		return node;
	}

	protected RBTreeNode LBound(object key)
	{
		RBTreeNode rBTreeNode = m_MyHead.Parent;
		RBTreeNode result = m_MyHead;
		while (!rBTreeNode.IsNil)
		{
			if (m_comparer.Compare(rBTreeNode.Key, key) < 0)
			{
				rBTreeNode = rBTreeNode.Right;
				continue;
			}
			result = rBTreeNode;
			rBTreeNode = rBTreeNode.Left;
		}
		return result;
	}

	protected RBTreeNode UBound(object key)
	{
		RBTreeNode rBTreeNode = m_MyHead.Parent;
		RBTreeNode result = m_MyHead;
		while (!rBTreeNode.IsNil)
		{
			if (m_comparer.Compare(key, rBTreeNode.Key) < 0)
			{
				result = rBTreeNode;
				rBTreeNode = rBTreeNode.Left;
			}
			else
			{
				rBTreeNode = rBTreeNode.Right;
			}
		}
		return result;
	}

	protected void LRotate(RBTreeNode _where)
	{
		RBTreeNode right = _where.Right;
		_where.Right = right.Left;
		if (!right.Left.IsNil)
		{
			right.Left.Parent = _where;
		}
		right.Parent = _where.Parent;
		if (_where == m_MyHead.Parent)
		{
			m_MyHead.Parent = right;
		}
		else if (_where == _where.Parent.Left)
		{
			_where.Parent.Left = right;
		}
		else
		{
			_where.Parent.Right = right;
		}
		right.Left = _where;
		_where.Parent = right;
	}

	protected void RRotate(RBTreeNode _where)
	{
		RBTreeNode left = _where.Left;
		_where.Left = left.Right;
		if (!left.Right.IsNil)
		{
			left.Right.Parent = _where;
		}
		left.Parent = _where.Parent;
		if (_where == m_MyHead.Parent)
		{
			m_MyHead.Parent = left;
		}
		else if (_where == _where.Parent.Right)
		{
			_where.Parent.Right = left;
		}
		else
		{
			_where.Parent.Left = left;
		}
		left.Right = _where;
		_where.Parent = left;
	}

	protected void Erase(RBTreeNode _root)
	{
		RBTreeNode rBTreeNode = _root;
		while (!rBTreeNode.IsNil)
		{
			Erase(rBTreeNode.Right);
			rBTreeNode = rBTreeNode.Left;
			_root = rBTreeNode;
		}
	}

	protected void Insert(bool _addLeft, RBTreeNode _where, object key, object value)
	{
		RBTreeNode rBTreeNode = new RBTreeNode(m_MyHead, _where, m_MyHead, key, value);
		m_size++;
		if (_where == m_MyHead)
		{
			RBTreeNode myHead = m_MyHead;
			RBTreeNode myHead2 = m_MyHead;
			RBTreeNode rBTreeNode3 = (m_MyHead.Right = rBTreeNode);
			RBTreeNode parent = (myHead2.Left = rBTreeNode3);
			myHead.Parent = parent;
		}
		else if (_addLeft)
		{
			_where.Left = rBTreeNode;
			if (_where == m_MyHead.Left)
			{
				m_MyHead.Left = rBTreeNode;
			}
		}
		else
		{
			_where.Right = rBTreeNode;
			if (_where == m_MyHead.Right)
			{
				m_MyHead.Right = rBTreeNode;
			}
		}
		RBTreeNode rBTreeNode5 = rBTreeNode;
		while (rBTreeNode5.Parent.Color == NodeColor.Red)
		{
			if (rBTreeNode5.Parent == rBTreeNode5.Parent.Parent.Left)
			{
				_where = rBTreeNode5.Parent.Parent.Right;
				if (_where.Color == NodeColor.Red)
				{
					rBTreeNode5.Parent.Color = NodeColor.Black;
					_where.Color = NodeColor.Black;
					rBTreeNode5.Parent.Parent.Color = NodeColor.Red;
					rBTreeNode5 = rBTreeNode5.Parent.Parent;
					continue;
				}
				if (rBTreeNode5 == rBTreeNode5.Parent.Right)
				{
					rBTreeNode5 = rBTreeNode5.Parent;
					LRotate(rBTreeNode5);
				}
				rBTreeNode5.Parent.Color = NodeColor.Black;
				rBTreeNode5.Parent.Parent.Color = NodeColor.Red;
				RRotate(rBTreeNode5.Parent.Parent);
				continue;
			}
			_where = rBTreeNode5.Parent.Parent.Left;
			if (_where.Color == NodeColor.Red)
			{
				rBTreeNode5.Parent.Color = NodeColor.Black;
				_where.Color = NodeColor.Black;
				rBTreeNode5.Parent.Parent.Color = NodeColor.Red;
				rBTreeNode5 = rBTreeNode5.Parent.Parent;
				continue;
			}
			if (rBTreeNode5 == rBTreeNode5.Parent.Left)
			{
				rBTreeNode5 = rBTreeNode5.Parent;
				RRotate(rBTreeNode5);
			}
			rBTreeNode5.Parent.Color = NodeColor.Black;
			rBTreeNode5.Parent.Parent.Color = NodeColor.Red;
			LRotate(rBTreeNode5.Parent.Parent);
		}
		m_MyHead.Parent.Color = NodeColor.Black;
	}

	public IEnumerator GetEnumerator()
	{
		return new MapEnumerator(m_MyHead.Left);
	}

	internal void ForAll(NodeFunction function)
	{
		ForAll(m_MyHead, function);
	}

	private void ForAll(RBTreeNode startNode, NodeFunction function)
	{
		if (startNode != null && (!startNode.IsNil || !startNode.Left.IsNil || !startNode.Right.IsNil))
		{
			if (!startNode.IsNil)
			{
				function(startNode);
			}
			ForAll(startNode.Left, function);
			ForAll(startNode.Right, function);
		}
	}
}
