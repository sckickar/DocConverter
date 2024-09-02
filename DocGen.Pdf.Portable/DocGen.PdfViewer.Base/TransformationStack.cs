using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class TransformationStack
{
	private Matrix currentTransform = Matrix.Identity;

	private Matrix initialTransform = Matrix.Identity;

	private Stack<Matrix> transformStack = new Stack<Matrix>();

	public Matrix CurrentTransform
	{
		get
		{
			if (transformStack.Count == 0)
			{
				return initialTransform;
			}
			return currentTransform * initialTransform;
		}
	}

	public TransformationStack()
	{
		initialTransform = Matrix.Identity;
	}

	public TransformationStack(Matrix initialTransform)
	{
		this.initialTransform = initialTransform;
	}

	public void PushTransform(Matrix transformMatrix)
	{
		transformStack.Push(transformMatrix);
		Matrix identity = Matrix.Identity;
		foreach (Matrix item in transformStack)
		{
			identity *= item;
		}
		currentTransform = identity;
	}

	public void PopTransform()
	{
		transformStack.Pop();
		Matrix identity = Matrix.Identity;
		foreach (Matrix item in transformStack)
		{
			identity *= item;
		}
		currentTransform = identity;
	}

	public void Clear()
	{
		transformStack.Clear();
	}
}
