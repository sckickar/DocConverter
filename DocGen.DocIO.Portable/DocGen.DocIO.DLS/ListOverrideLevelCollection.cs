using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

internal class ListOverrideLevelCollection : XDLSSerializableCollection
{
	private Dictionary<short, short> m_levelIndex;

	public OverrideLevelFormat this[int levelNumber] => (OverrideLevelFormat)base.InnerList[LevelIndex[(short)levelNumber]];

	private ListOverrideStyle OwnerStyle => base.OwnerBase as ListOverrideStyle;

	internal Dictionary<short, short> LevelIndex
	{
		get
		{
			if (m_levelIndex == null)
			{
				m_levelIndex = new Dictionary<short, short>();
			}
			return m_levelIndex;
		}
		set
		{
			m_levelIndex = value;
		}
	}

	internal ListOverrideLevelCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	internal int Add(int levelNumber, OverrideLevelFormat lfoLevel)
	{
		lfoLevel.SetOwner(OwnerStyle);
		short num = (short)base.InnerList.Add(lfoLevel);
		short key = (short)levelNumber;
		if (LevelIndex.ContainsKey(key))
		{
			LevelIndex[key] = num;
		}
		else
		{
			LevelIndex.Add(key, num);
		}
		return num;
	}

	internal int GetLevelNumber(OverrideLevelFormat levelFormat)
	{
		int num = base.InnerList.IndexOf(levelFormat);
		int result = num;
		foreach (KeyValuePair<short, short> item in LevelIndex)
		{
			if (item.Value == num)
			{
				result = item.Key;
				break;
			}
		}
		return result;
	}

	internal bool HasOverrideLevel(int levelNumber)
	{
		if (LevelIndex.Count > 0)
		{
			return LevelIndex.ContainsKey((short)levelNumber);
		}
		return false;
	}

	internal override void CloneToImpl(CollectionImpl collection)
	{
		base.CloneToImpl(collection);
		foreach (KeyValuePair<short, short> item in LevelIndex)
		{
			(collection as ListOverrideLevelCollection).LevelIndex.Add(item.Key, item.Value);
		}
	}

	protected override OwnerHolder CreateItem(IXDLSContentReader reader)
	{
		return new OverrideLevelFormat(base.Document);
	}

	protected override string GetTagItemName()
	{
		return "override-level";
	}

	internal override void Close()
	{
		base.Close();
		if (m_levelIndex != null)
		{
			m_levelIndex.Clear();
			m_levelIndex = null;
		}
	}

	internal bool Compare(ListOverrideLevelCollection listOverrideLevels)
	{
		if (LevelIndex.Count != listOverrideLevels.LevelIndex.Count)
		{
			return false;
		}
		foreach (KeyValuePair<short, short> item in LevelIndex)
		{
			if (!listOverrideLevels.LevelIndex.ContainsKey(item.Key))
			{
				return false;
			}
			if (!listOverrideLevels[item.Key].Compare(this[item.Key]))
			{
				return false;
			}
		}
		return true;
	}
}
