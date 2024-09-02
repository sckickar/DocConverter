namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontEnumerableSorter<TElement>
{
	internal abstract int CompareKeys(int index1, int index2);

	internal abstract void ComputeKeys(TElement[] elements, int count);

	private void QuickSort(int[] map, int left, int right)
	{
		do
		{
			int num = left;
			int num2 = right;
			int index = map[num + (num2 - num >> 1)];
			while (true)
			{
				if (num < map.Length && CompareKeys(index, map[num]) > 0)
				{
					num++;
					continue;
				}
				while (num2 >= 0 && CompareKeys(index, map[num2]) < 0)
				{
					num2--;
				}
				if (num > num2)
				{
					break;
				}
				if (num < num2)
				{
					int num3 = map[num];
					map[num] = map[num2];
					map[num2] = num3;
				}
				num++;
				num2--;
				if (num > num2)
				{
					break;
				}
			}
			if (num2 - left > right - num)
			{
				if (num < right)
				{
					QuickSort(map, num, right);
				}
				right = num2;
			}
			else
			{
				if (left < num2)
				{
					QuickSort(map, left, num2);
				}
				left = num;
			}
		}
		while (left < right);
	}

	internal int[] Sort(TElement[] elements, int count)
	{
		ComputeKeys(elements, count);
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = i;
		}
		QuickSort(array, 0, count - 1);
		return array;
	}
}
