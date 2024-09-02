using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;
using DocGen.Drawing;

namespace DocGen.Chart.Drawing;

[Serializable]
[TypeConverter(typeof(ColorListConverter))]
internal class BrushInfoColorArrayList : ArrayList, ISerializable
{
	private bool freeze;

	public new Color this[int index]
	{
		get
		{
			return (Color)base[index];
		}
		set
		{
			base[index] = value;
		}
	}

	internal bool Freeze
	{
		get
		{
			return freeze;
		}
		set
		{
			freeze = value;
		}
	}

	public BrushInfoColorArrayList()
	{
	}

	public BrushInfoColorArrayList(Color[] colors)
	{
		AddRange(colors);
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		for (int i = 0; i < Count; i++)
		{
			info.AddValue(i.ToString(), this[i]);
		}
	}

	internal void Add(Color color)
	{
		base.Add(color);
	}

	internal void AddRange(Color[] colors)
	{
		base.AddRange(colors);
	}
}
