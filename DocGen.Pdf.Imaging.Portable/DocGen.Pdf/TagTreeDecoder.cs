using System;

namespace DocGen.Pdf;

internal class TagTreeDecoder
{
	internal int w;

	internal int h;

	internal int lvls;

	internal int[][] treeV;

	internal int[][] treeS;

	public virtual int Width => w;

	public virtual int Height => h;

	public TagTreeDecoder(int h, int w)
	{
		if (w < 0 || h < 0)
		{
			throw new ArgumentException();
		}
		this.w = w;
		this.h = h;
		if (w == 0 || h == 0)
		{
			lvls = 0;
		}
		else
		{
			lvls = 1;
			while (h != 1 || w != 1)
			{
				w = w + 1 >> 1;
				h = h + 1 >> 1;
				lvls++;
			}
		}
		treeV = new int[lvls][];
		treeS = new int[lvls][];
		w = this.w;
		h = this.h;
		for (int i = 0; i < lvls; i++)
		{
			treeV[i] = new int[h * w];
			ArrayUtil.intArraySet(treeV[i], int.MaxValue);
			treeS[i] = new int[h * w];
			w = w + 1 >> 1;
			h = h + 1 >> 1;
		}
	}

	public virtual int update(int m, int n, int t, PktHeaderBitReader in_Renamed)
	{
		if (m >= h || n >= w || t < 0)
		{
			throw new ArgumentException();
		}
		int num = lvls - 1;
		int num2 = treeS[num][0];
		int num3 = (m >> num) * (w + (1 << num) - 1 >> num) + (n >> num);
		int num5;
		while (true)
		{
			int num4 = treeS[num][num3];
			num5 = treeV[num][num3];
			if (num4 < num2)
			{
				num4 = num2;
			}
			while (t > num4)
			{
				if (num5 >= num4)
				{
					if (in_Renamed.readBit() == 0)
					{
						num4++;
					}
					else
					{
						num5 = num4++;
					}
					continue;
				}
				num4 = t;
				break;
			}
			treeS[num][num3] = num4;
			treeV[num][num3] = num5;
			if (num <= 0)
			{
				break;
			}
			num2 = ((num4 < num5) ? num4 : num5);
			num--;
			num3 = (m >> num) * (w + (1 << num) - 1 >> num) + (n >> num);
		}
		return num5;
	}

	public virtual int getValue(int m, int n)
	{
		if (m >= h || n >= w)
		{
			throw new ArgumentException();
		}
		return treeV[0][m * w + n];
	}
}
