using System;

namespace BitMiracle.LibTiff.Classic;

public struct FieldValue
{
	private object m_value;

	public object Value => m_value;

	internal FieldValue(object o)
	{
		m_value = o;
	}

	internal static FieldValue[] FromParams(params object[] list)
	{
		FieldValue[] array = new FieldValue[list.Length];
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i] is FieldValue)
			{
				array[i] = new FieldValue(((FieldValue)list[i]).Value);
			}
			else
			{
				array[i] = new FieldValue(list[i]);
			}
		}
		return array;
	}

	internal void Set(object o)
	{
		m_value = o;
	}

	public byte ToByte()
	{
		return Convert.ToByte(m_value);
	}

	public short ToShort()
	{
		Type type = m_value.GetType();
		if (type == typeof(ushort))
		{
			return (short)(ushort)m_value;
		}
		if (type == typeof(short))
		{
			return (short)m_value;
		}
		return Convert.ToInt16(m_value);
	}

	[CLSCompliant(false)]
	public ushort ToUShort()
	{
		Type type = m_value.GetType();
		if (type == typeof(ushort))
		{
			return (ushort)m_value;
		}
		if (type == typeof(short))
		{
			return (ushort)(short)m_value;
		}
		return Convert.ToUInt16(m_value);
	}

	public int ToInt()
	{
		Type type = m_value.GetType();
		if (type == typeof(uint))
		{
			return (int)(uint)m_value;
		}
		if (type == typeof(int))
		{
			return (int)m_value;
		}
		return Convert.ToInt32(m_value);
	}

	[CLSCompliant(false)]
	public uint ToUInt()
	{
		Type type = m_value.GetType();
		if (type == typeof(uint))
		{
			return (uint)m_value;
		}
		if (type == typeof(int))
		{
			return (uint)(int)m_value;
		}
		return Convert.ToUInt32(m_value);
	}

	public long ToLong()
	{
		Type type = m_value.GetType();
		if (type == typeof(ulong))
		{
			return (long)(ulong)m_value;
		}
		if (type == typeof(long))
		{
			return (long)m_value;
		}
		return Convert.ToInt64(m_value);
	}

	public float ToFloat()
	{
		return Convert.ToSingle(m_value);
	}

	public double ToDouble()
	{
		return Convert.ToDouble(m_value);
	}

	public override string ToString()
	{
		if (m_value is byte[] bytes)
		{
			return Tiff.Latin1Encoding.GetString(bytes);
		}
		return Convert.ToString(m_value);
	}

	public byte[] GetBytes()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is byte[])
			{
				return m_value as byte[];
			}
			if (m_value is short[])
			{
				short[] obj = m_value as short[];
				byte[] array = new byte[obj.Length * 2];
				Buffer.BlockCopy(obj, 0, array, 0, array.Length);
				return array;
			}
			if (m_value is ushort[])
			{
				ushort[] obj2 = m_value as ushort[];
				byte[] array2 = new byte[obj2.Length * 2];
				Buffer.BlockCopy(obj2, 0, array2, 0, array2.Length);
				return array2;
			}
			if (m_value is int[])
			{
				int[] obj3 = m_value as int[];
				byte[] array3 = new byte[obj3.Length * 4];
				Buffer.BlockCopy(obj3, 0, array3, 0, array3.Length);
				return array3;
			}
			if (m_value is uint[])
			{
				uint[] obj4 = m_value as uint[];
				byte[] array4 = new byte[obj4.Length * 4];
				Buffer.BlockCopy(obj4, 0, array4, 0, array4.Length);
				return array4;
			}
			if (m_value is long[])
			{
				long[] obj5 = m_value as long[];
				byte[] array5 = new byte[obj5.Length * 8];
				Buffer.BlockCopy(obj5, 0, array5, 0, array5.Length);
				return array5;
			}
			if (m_value is ulong[])
			{
				ulong[] obj6 = m_value as ulong[];
				byte[] array6 = new byte[obj6.Length * 8];
				Buffer.BlockCopy(obj6, 0, array6, 0, array6.Length);
				return array6;
			}
			if (m_value is float[])
			{
				float[] obj7 = m_value as float[];
				byte[] array7 = new byte[obj7.Length * 4];
				Buffer.BlockCopy(obj7, 0, array7, 0, array7.Length);
				return array7;
			}
			if (m_value is double[])
			{
				double[] obj8 = m_value as double[];
				byte[] array8 = new byte[obj8.Length * 8];
				Buffer.BlockCopy(obj8, 0, array8, 0, array8.Length);
				return array8;
			}
		}
		else
		{
			if (m_value is string)
			{
				return Tiff.Latin1Encoding.GetBytes(m_value as string);
			}
			if (m_value is int)
			{
				return BitConverter.GetBytes((int)m_value);
			}
		}
		return null;
	}

	public byte[] ToByteArray()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is byte[])
			{
				return m_value as byte[];
			}
			if (m_value is short[])
			{
				short[] array = m_value as short[];
				byte[] array2 = new byte[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = (byte)array[i];
				}
				return array2;
			}
			if (m_value is ushort[])
			{
				ushort[] array3 = m_value as ushort[];
				byte[] array4 = new byte[array3.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					array4[j] = (byte)array3[j];
				}
				return array4;
			}
			if (m_value is int[])
			{
				int[] array5 = m_value as int[];
				byte[] array6 = new byte[array5.Length];
				for (int k = 0; k < array5.Length; k++)
				{
					array6[k] = (byte)array5[k];
				}
				return array6;
			}
			if (m_value is uint[])
			{
				uint[] array7 = m_value as uint[];
				byte[] array8 = new byte[array7.Length];
				for (int l = 0; l < array7.Length; l++)
				{
					array8[l] = (byte)array7[l];
				}
				return array8;
			}
		}
		else if (m_value is string)
		{
			return Tiff.Latin1Encoding.GetBytes(m_value as string);
		}
		return null;
	}

	public short[] ToShortArray()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is short[])
			{
				return m_value as short[];
			}
			if (m_value is byte[])
			{
				byte[] array = m_value as byte[];
				if (array.Length % 2 != 0)
				{
					return null;
				}
				int num = array.Length / 2;
				short[] array2 = new short[num];
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					short num3 = BitConverter.ToInt16(array, num2);
					array2[i] = num3;
					num2 += 2;
				}
				return array2;
			}
			if (m_value is ushort[])
			{
				ushort[] array3 = m_value as ushort[];
				short[] array4 = new short[array3.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					array4[j] = (short)array3[j];
				}
				return array4;
			}
			if (m_value is int[])
			{
				int[] array5 = m_value as int[];
				short[] array6 = new short[array5.Length];
				for (int k = 0; k < array5.Length; k++)
				{
					array6[k] = (short)array5[k];
				}
				return array6;
			}
			if (m_value is uint[])
			{
				uint[] array7 = m_value as uint[];
				short[] array8 = new short[array7.Length];
				for (int l = 0; l < array7.Length; l++)
				{
					array8[l] = (short)array7[l];
				}
				return array8;
			}
		}
		return null;
	}

	[CLSCompliant(false)]
	public ushort[] ToUShortArray()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is ushort[])
			{
				return m_value as ushort[];
			}
			if (m_value is byte[])
			{
				byte[] array = m_value as byte[];
				if (array.Length % 2 != 0)
				{
					return null;
				}
				int num = array.Length / 2;
				ushort[] array2 = new ushort[num];
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					ushort num3 = BitConverter.ToUInt16(array, num2);
					array2[i] = num3;
					num2 += 2;
				}
				return array2;
			}
			if (m_value is short[])
			{
				short[] array3 = m_value as short[];
				ushort[] array4 = new ushort[array3.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					array4[j] = (ushort)array3[j];
				}
				return array4;
			}
			if (m_value is int[])
			{
				int[] array5 = m_value as int[];
				ushort[] array6 = new ushort[array5.Length];
				for (int k = 0; k < array5.Length; k++)
				{
					array6[k] = (ushort)array5[k];
				}
				return array6;
			}
			if (m_value is uint[])
			{
				uint[] array7 = m_value as uint[];
				ushort[] array8 = new ushort[array7.Length];
				for (int l = 0; l < array7.Length; l++)
				{
					array8[l] = (ushort)array7[l];
				}
				return array8;
			}
		}
		return null;
	}

	public int[] ToIntArray()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is int[])
			{
				return m_value as int[];
			}
			if (m_value is byte[])
			{
				byte[] array = m_value as byte[];
				if (array.Length % 4 != 0)
				{
					return null;
				}
				int num = array.Length / 4;
				int[] array2 = new int[num];
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					int num3 = BitConverter.ToInt32(array, num2);
					array2[i] = num3;
					num2 += 4;
				}
				return array2;
			}
			if (m_value is short[])
			{
				short[] array3 = m_value as short[];
				int[] array4 = new int[array3.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					array4[j] = array3[j];
				}
				return array4;
			}
			if (m_value is ushort[])
			{
				ushort[] array5 = m_value as ushort[];
				int[] array6 = new int[array5.Length];
				for (int k = 0; k < array5.Length; k++)
				{
					array6[k] = array5[k];
				}
				return array6;
			}
			if (m_value is uint[])
			{
				uint[] array7 = m_value as uint[];
				int[] array8 = new int[array7.Length];
				for (int l = 0; l < array7.Length; l++)
				{
					array8[l] = (int)array7[l];
				}
				return array8;
			}
			if (m_value is ulong[])
			{
				ulong[] array9 = m_value as ulong[];
				int[] array10 = new int[array9.Length];
				for (int m = 0; m < array9.Length; m++)
				{
					array10[m] = (int)array9[m];
				}
				return array10;
			}
		}
		return null;
	}

	[CLSCompliant(false)]
	public uint[] ToUIntArray()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is uint[])
			{
				return m_value as uint[];
			}
			if (m_value is byte[])
			{
				byte[] array = m_value as byte[];
				if (array.Length % 4 != 0)
				{
					return null;
				}
				int num = array.Length / 4;
				uint[] array2 = new uint[num];
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					uint num3 = BitConverter.ToUInt32(array, num2);
					array2[i] = num3;
					num2 += 4;
				}
				return array2;
			}
			if (m_value is short[])
			{
				short[] array3 = m_value as short[];
				uint[] array4 = new uint[array3.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					array4[j] = (uint)array3[j];
				}
				return array4;
			}
			if (m_value is ushort[])
			{
				ushort[] array5 = m_value as ushort[];
				uint[] array6 = new uint[array5.Length];
				for (int k = 0; k < array5.Length; k++)
				{
					array6[k] = array5[k];
				}
				return array6;
			}
			if (m_value is int[])
			{
				int[] array7 = m_value as int[];
				uint[] array8 = new uint[array7.Length];
				for (int l = 0; l < array7.Length; l++)
				{
					array8[l] = (uint)array7[l];
				}
				return array8;
			}
		}
		return null;
	}

	public long[] TolongArray()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is long[])
			{
				return m_value as long[];
			}
			if (m_value is byte[])
			{
				byte[] array = m_value as byte[];
				if (array.Length % 8 != 0)
				{
					return null;
				}
				int num = array.Length / 8;
				long[] array2 = new long[num];
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					long num3 = BitConverter.ToUInt32(array, num2);
					array2[i] = num3;
					num2 += 8;
				}
				return array2;
			}
			if (m_value is short[])
			{
				short[] array3 = m_value as short[];
				long[] array4 = new long[array3.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					array4[j] = array3[j];
				}
				return array4;
			}
			if (m_value is ushort[])
			{
				ushort[] array5 = m_value as ushort[];
				long[] array6 = new long[array5.Length];
				for (int k = 0; k < array5.Length; k++)
				{
					array6[k] = array5[k];
				}
				return array6;
			}
			if (m_value is int[])
			{
				int[] array7 = m_value as int[];
				long[] array8 = new long[array7.Length];
				for (int l = 0; l < array7.Length; l++)
				{
					array8[l] = array7[l];
				}
				return array8;
			}
		}
		return null;
	}

	public float[] ToFloatArray()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is float[])
			{
				return m_value as float[];
			}
			if (m_value is double[])
			{
				double[] array = m_value as double[];
				float[] array2 = new float[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = (float)array[i];
				}
				return array2;
			}
			if (m_value is byte[])
			{
				byte[] array3 = m_value as byte[];
				if (array3.Length % 4 != 0)
				{
					return null;
				}
				int num = 0;
				int num2 = array3.Length / 4;
				float[] array4 = new float[num2];
				for (int j = 0; j < num2; j++)
				{
					float num3 = BitConverter.ToSingle(array3, num);
					array4[j] = num3;
					num += 4;
				}
				return array4;
			}
		}
		return null;
	}

	public double[] ToDoubleArray()
	{
		if (m_value == null)
		{
			return null;
		}
		if (m_value.GetType().IsArray)
		{
			if (m_value is double[])
			{
				return m_value as double[];
			}
			if (m_value is float[])
			{
				float[] array = m_value as float[];
				double[] array2 = new double[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = array[i];
				}
				return array2;
			}
			if (m_value is byte[])
			{
				byte[] array3 = m_value as byte[];
				if (array3.Length % 8 != 0)
				{
					return null;
				}
				int num = 0;
				int num2 = array3.Length / 8;
				double[] array4 = new double[num2];
				for (int j = 0; j < num2; j++)
				{
					double num3 = BitConverter.ToDouble(array3, num);
					array4[j] = num3;
					num += 8;
				}
				return array4;
			}
		}
		return null;
	}
}
