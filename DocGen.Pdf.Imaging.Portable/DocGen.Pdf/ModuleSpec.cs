using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class ModuleSpec
{
	public const byte SPEC_TYPE_COMP = 0;

	public const byte SPEC_TYPE_TILE = 1;

	public const byte SPEC_TYPE_TILE_COMP = 2;

	public const byte SPEC_DEF = 0;

	public const byte SPEC_COMP_DEF = 1;

	public const byte SPEC_TILE_DEF = 2;

	public const byte SPEC_TILE_COMP = 3;

	internal int specType;

	internal int nTiles;

	internal int nComp;

	internal byte[][] specValType;

	internal object def;

	internal object[] compDef;

	internal object[] tileDef;

	internal Dictionary<object, object> tileCompVal;

	public virtual ModuleSpec Copy => (ModuleSpec)Clone();

	public virtual object Clone()
	{
		ModuleSpec moduleSpec = null;
		try
		{
			moduleSpec = (ModuleSpec)MemberwiseClone();
		}
		catch (Exception)
		{
		}
		moduleSpec.specValType = new byte[nTiles][];
		for (int i = 0; i < nTiles; i++)
		{
			moduleSpec.specValType[i] = new byte[nComp];
		}
		for (int j = 0; j < nTiles; j++)
		{
			for (int k = 0; k < nComp; k++)
			{
				moduleSpec.specValType[j][k] = specValType[j][k];
			}
		}
		if (tileDef != null)
		{
			moduleSpec.tileDef = new object[nTiles];
			for (int l = 0; l < nTiles; l++)
			{
				moduleSpec.tileDef[l] = tileDef[l];
			}
		}
		if (tileCompVal != null)
		{
			moduleSpec.tileCompVal = new Dictionary<object, object>();
			IEnumerator enumerator = tileCompVal.Keys.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string key = (string)enumerator.Current;
				object value = tileCompVal[key];
				moduleSpec.tileCompVal[key] = value;
			}
		}
		return moduleSpec;
	}

	public virtual void rotate90(JPXImageCoordinates anT)
	{
		byte[][] array = new byte[nTiles][];
		JPXImageCoordinates jPXImageCoordinates = new JPXImageCoordinates(anT.y, anT.x);
		for (int i = 0; i < jPXImageCoordinates.y; i++)
		{
			for (int j = 0; j < jPXImageCoordinates.x; j++)
			{
				int num = j;
				int num2 = jPXImageCoordinates.y - i - 1;
				array[num * anT.x + num2] = specValType[i * jPXImageCoordinates.x + j];
			}
		}
		specValType = array;
		if (tileDef != null)
		{
			object[] array2 = new object[nTiles];
			for (int k = 0; k < jPXImageCoordinates.y; k++)
			{
				for (int l = 0; l < jPXImageCoordinates.x; l++)
				{
					int num = l;
					int num2 = jPXImageCoordinates.y - k - 1;
					array2[num * anT.x + num2] = tileDef[k * jPXImageCoordinates.x + l];
				}
			}
			tileDef = array2;
		}
		if (tileCompVal != null && tileCompVal.Count > 0)
		{
			Dictionary<object, object> dictionary = new Dictionary<object, object>();
			IEnumerator enumerator = tileCompVal.Keys.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string text = (string)enumerator.Current;
				object value = tileCompVal[text];
				int num3 = text.IndexOf('t');
				int num4 = text.IndexOf('c');
				int num5 = int.Parse(text.Substring(num3 + 1, num4 - (num3 + 1)));
				int num6 = num5 % jPXImageCoordinates.x;
				int num7 = num5 / jPXImageCoordinates.x;
				int num = num6;
				int num2 = jPXImageCoordinates.y - num7 - 1;
				dictionary["t" + (num2 + num * anT.x) + text.Substring(num4)] = value;
			}
			tileCompVal = dictionary;
		}
	}

	public ModuleSpec(int nt, int nc, byte type)
	{
		nTiles = nt;
		nComp = nc;
		specValType = new byte[nt][];
		for (int i = 0; i < nt; i++)
		{
			specValType[i] = new byte[nc];
		}
		switch (type)
		{
		case 1:
			specType = 1;
			break;
		case 0:
			specType = 0;
			break;
		case 2:
			specType = 2;
			break;
		}
	}

	public virtual void setDefault(object value_Renamed)
	{
		def = value_Renamed;
	}

	public virtual object getDefault()
	{
		return def;
	}

	public virtual void setCompDef(int c, object value_Renamed)
	{
		if (specType == 1)
		{
			_ = "Option whose value is '" + value_Renamed?.ToString() + "' cannot be specified for components as it is a 'tile only' specific option";
		}
		if (compDef == null)
		{
			compDef = new object[nComp];
		}
		for (int i = 0; i < nTiles; i++)
		{
			if (specValType[i][c] < 1)
			{
				specValType[i][c] = 1;
			}
		}
		compDef[c] = value_Renamed;
	}

	public virtual object getCompDef(int c)
	{
		_ = specType;
		_ = 1;
		if (compDef == null || compDef[c] == null)
		{
			return getDefault();
		}
		return compDef[c];
	}

	public virtual void setTileDef(int t, object value_Renamed)
	{
		if (specType == 0)
		{
			_ = "Option whose value is '" + value_Renamed?.ToString() + "' cannot be specified for tiles as it is a 'component only' specific option";
		}
		if (tileDef == null)
		{
			tileDef = new object[nTiles];
		}
		for (int i = 0; i < nComp; i++)
		{
			if (specValType[t][i] < 2)
			{
				specValType[t][i] = 2;
			}
		}
		tileDef[t] = value_Renamed;
	}

	public virtual object getTileDef(int t)
	{
		_ = specType;
		if (tileDef == null || tileDef[t] == null)
		{
			return getDefault();
		}
		return tileDef[t];
	}

	public virtual void setTileCompVal(int t, int c, object value_Renamed)
	{
		if (specType != 2)
		{
			string text = "Option whose value is '" + value_Renamed?.ToString() + "' cannot be specified for ";
			switch (specType)
			{
			case 1:
				text += "components as it is a 'tile only' specific option";
				break;
			case 0:
				text += "tiles as it is a 'component only' specific option";
				break;
			}
		}
		if (tileCompVal == null)
		{
			tileCompVal = new Dictionary<object, object>();
		}
		specValType[t][c] = 3;
		tileCompVal["t" + t + "c" + c] = value_Renamed;
	}

	public virtual object getTileCompVal(int t, int c)
	{
		_ = specType;
		_ = 2;
		return getSpec(t, c);
	}

	internal virtual object getSpec(int t, int c)
	{
		return specValType[t][c] switch
		{
			0 => getDefault(), 
			1 => getCompDef(c), 
			2 => getTileDef(t), 
			3 => tileCompVal["t" + t + "c" + c], 
			_ => throw new ArgumentException("Not recognized spec type"), 
		};
	}

	public virtual byte getSpecValType(int t, int c)
	{
		return specValType[t][c];
	}

	public virtual bool isCompSpecified(int c)
	{
		if (compDef == null || compDef[c] == null)
		{
			return false;
		}
		return true;
	}

	public virtual bool isTileSpecified(int t)
	{
		if (tileDef == null || tileDef[t] == null)
		{
			return false;
		}
		return true;
	}

	public virtual bool isTileCompSpecified(int t, int c)
	{
		if (tileCompVal == null || tileCompVal["t" + t + "c" + c] == null)
		{
			return false;
		}
		return true;
	}

	public static bool[] parseIdx(string word, int maxIdx)
	{
		int length = word.Length;
		char c = word[0];
		int num = -1;
		int num2 = -1;
		bool flag = false;
		bool[] array = new bool[maxIdx];
		for (int i = 1; i < length; i++)
		{
			c = word[i];
			if (char.IsDigit(c))
			{
				if (num == -1)
				{
					num = 0;
				}
				num = num * 10 + (c - 48);
				continue;
			}
			if (num == -1 || (c != ',' && c != '-'))
			{
				throw new ArgumentException("Bad construction for parameter: " + word);
			}
			if (num < 0 || num >= maxIdx)
			{
				throw new ArgumentException("Out of range index in parameter `" + word + "' : " + num);
			}
			if (c == ',')
			{
				if (flag)
				{
					for (int j = num2 + 1; j < num; j++)
					{
						array[j] = true;
					}
				}
				flag = false;
			}
			else
			{
				flag = true;
			}
			array[num] = true;
			num2 = num;
			num = -1;
		}
		if (num < 0 || num >= maxIdx)
		{
			throw new ArgumentException("Out of range index in parameter `" + word + "' : " + num);
		}
		if (flag)
		{
			for (int k = num2 + 1; k < num; k++)
			{
				array[k] = true;
			}
		}
		array[num] = true;
		return array;
	}
}
