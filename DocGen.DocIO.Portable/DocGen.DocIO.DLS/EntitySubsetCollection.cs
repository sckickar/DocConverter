using System;
using System.Collections;

namespace DocGen.DocIO.DLS;

public abstract class EntitySubsetCollection : IEntityCollectionBase, ICollectionBase, IEnumerable
{
	public class SubSetEnumerator : IEnumerator
	{
		private int m_currIndex = -1;

		private EntitySubsetCollection m_enColl;

		public object Current
		{
			get
			{
				if (m_currIndex < 0)
				{
					return null;
				}
				return m_enColl.m_coll[m_currIndex];
			}
		}

		public SubSetEnumerator(EntitySubsetCollection enColl)
		{
			m_enColl = enColl;
		}

		public bool MoveNext()
		{
			int nextOrPrevIndex = m_enColl.m_coll.GetNextOrPrevIndex(m_currIndex, m_enColl.m_type, next: true);
			if (nextOrPrevIndex < 0)
			{
				return false;
			}
			m_currIndex = nextOrPrevIndex;
			return true;
		}

		public void Reset()
		{
			m_currIndex = -1;
		}
	}

	private EntityCollection m_coll;

	private EntityType m_type;

	private int m_lastIndex = -1;

	private int m_lastBaseIndex = -1;

	private int m_count;

	public WordDocument Document => m_coll.Document;

	public Entity Owner => m_coll.Owner;

	public int Count => m_count;

	public Entity this[int index]
	{
		get
		{
			if (m_coll.Count < 1)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			ClearIndexes();
			return GetByIndex(index);
		}
	}

	internal EntitySubsetCollection(EntityCollection coll, EntityType type)
	{
		m_coll = coll;
		m_type = type;
		UpdateCount();
		coll.ChangeItemsHandlers.Add(BaseCollChangeItems);
	}

	public void Clear()
	{
		m_coll.InternalClearBy(m_type);
		m_count = 0;
		m_lastIndex = -1;
		m_lastBaseIndex = -1;
	}

	internal void Close()
	{
		if (m_coll != null)
		{
			m_coll.Close();
			m_coll = null;
		}
	}

	public IEnumerator GetEnumerator()
	{
		return new SubSetEnumerator(this);
	}

	internal int InternalAdd(Entity entity)
	{
		CheckType(entity);
		m_coll.Add(entity);
		return m_count - 1;
	}

	internal bool InternalContains(Entity entity)
	{
		CheckType(entity);
		return m_coll.Contains(entity);
	}

	internal int InternalIndexOf(Entity entity)
	{
		CheckType(entity);
		ClearIndexes();
		for (int i = 0; i < Count; i++)
		{
			if (GetByIndex(i) == entity)
			{
				return i;
			}
		}
		return -1;
	}

	internal int InternalInsert(int index, Entity entity)
	{
		int baseIndex = GetBaseIndex(index);
		m_coll.Insert(index, entity);
		return baseIndex + 1;
	}

	internal void InternalRemove(Entity entity)
	{
		CheckType(entity);
		m_coll.Remove(entity);
	}

	internal void InternalRemoveAt(int index)
	{
		int baseIndex = GetBaseIndex(index);
		m_coll.RemoveAt(baseIndex);
	}

	protected Entity GetByIndex(int index)
	{
		if (m_lastBaseIndex < 0 || index == m_lastIndex)
		{
			m_lastBaseIndex = GetBaseIndex(index);
			m_lastIndex = index;
		}
		else
		{
			bool flag = m_lastIndex < index;
			while (index != m_lastIndex)
			{
				m_lastBaseIndex = m_coll.GetNextOrPrevIndex(m_lastBaseIndex, m_type, flag);
				m_lastIndex += (flag ? 1 : (-1));
			}
		}
		return m_coll[m_lastBaseIndex];
	}

	private int GetBaseIndex(int index)
	{
		int num = 0;
		int i = 0;
		for (int count = m_coll.Count; i < count; i++)
		{
			if (((IEntityCollectionBase)m_coll)[i].EntityType == m_type)
			{
				if (num == index)
				{
					return i;
				}
				num++;
			}
		}
		return -1;
	}

	private void UpdateCount()
	{
		int num = -1;
		m_count = 0;
		while (true)
		{
			num = m_coll.GetNextOrPrevIndex(num, m_type, next: true);
			if (num >= 0)
			{
				m_count++;
				continue;
			}
			break;
		}
	}

	private void CheckType(Entity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		if (entity.EntityType != m_type)
		{
			throw new ArgumentException("Invalid entity type");
		}
	}

	private void BaseCollChangeItems(EntityCollection.ChangeItemsType type, Entity entity)
	{
		switch (type)
		{
		case EntityCollection.ChangeItemsType.Add:
			if (entity.EntityType == m_type)
			{
				m_count++;
			}
			break;
		case EntityCollection.ChangeItemsType.Remove:
			if (entity.EntityType == m_type)
			{
				m_count--;
			}
			break;
		case EntityCollection.ChangeItemsType.Clear:
			m_count = 0;
			break;
		}
	}

	internal void ClearIndexes()
	{
		m_lastIndex = -1;
		m_lastBaseIndex = -1;
	}
}
