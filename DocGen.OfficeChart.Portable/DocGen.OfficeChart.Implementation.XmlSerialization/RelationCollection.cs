using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class RelationCollection : IEnumerable, ICloneable
{
	private const string RelationIdStart = "rId";

	private static readonly int RelationIdStartLen = "rId".Length;

	private Dictionary<string, Relation> m_dicRelations = new Dictionary<string, Relation>();

	private string m_strItemPath;

	public Relation this[string id]
	{
		get
		{
			m_dicRelations.TryGetValue(id, out var value);
			return value;
		}
		set
		{
			m_dicRelations[id] = value;
		}
	}

	public int Count => m_dicRelations.Count;

	public string ItemPath
	{
		get
		{
			return m_strItemPath;
		}
		set
		{
			m_strItemPath = value;
		}
	}

	public void Remove(string id)
	{
		m_dicRelations.Remove(id);
	}

	public void RemoveByContentType(string contentType)
	{
		if (contentType == null || contentType.Length <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, Relation> dicRelation in m_dicRelations)
		{
			if (dicRelation.Value.Type == contentType)
			{
				m_dicRelations.Remove(dicRelation.Key);
				break;
			}
		}
	}

	public Relation FindRelationByContentType(string contentType, out string relationId)
	{
		Relation result = null;
		relationId = null;
		if (contentType != null && contentType.Length > 0)
		{
			foreach (KeyValuePair<string, Relation> dicRelation in m_dicRelations)
			{
				Relation value = dicRelation.Value;
				if (value.Type == contentType)
				{
					result = value;
					relationId = dicRelation.Key;
					break;
				}
			}
		}
		return result;
	}

	public string FindRelationByTarget(string itemName)
	{
		string result = null;
		if (itemName != null && itemName.Length > 0)
		{
			foreach (KeyValuePair<string, Relation> dicRelation in m_dicRelations)
			{
				Relation value = dicRelation.Value;
				if (value != null && value.Target == itemName)
				{
					result = dicRelation.Key;
					break;
				}
			}
		}
		return result;
	}

	public string GenerateRelationId()
	{
		string text = null;
		for (int i = 1; i < int.MaxValue; i++)
		{
			text = "rId" + i;
			if (!m_dicRelations.ContainsKey(text))
			{
				break;
			}
		}
		return text;
	}

	public string Add(Relation relation)
	{
		string empty = string.Empty;
		if (relation == null)
		{
			return empty;
		}
		empty = GenerateRelationId();
		this[empty] = relation;
		return empty;
	}

	public void Clear()
	{
		m_dicRelations.Clear();
	}

	public RelationCollection Clone()
	{
		RelationCollection obj = (RelationCollection)MemberwiseClone();
		obj.m_dicRelations = CloneUtils.CloneHash(m_dicRelations);
		return obj;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public void Dispose()
	{
		Clear();
		m_dicRelations = null;
	}

	public IEnumerator GetEnumerator()
	{
		return m_dicRelations.GetEnumerator();
	}
}
