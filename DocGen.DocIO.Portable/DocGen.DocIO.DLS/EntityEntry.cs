namespace DocGen.DocIO.DLS;

public class EntityEntry
{
	public Entity Current;

	public int Index;

	public EntityEntry(Entity ent)
	{
		Current = ent;
		Index = 0;
	}

	public bool Fetch()
	{
		if (Current != null && Current.Owner != null && Current.Owner.IsComposite)
		{
			ICompositeEntity compositeEntity = Current.Owner as ICompositeEntity;
			if (compositeEntity.ChildEntities.Count > Index + 1)
			{
				Index++;
				Current = compositeEntity.ChildEntities[Index];
				return true;
			}
		}
		Current = null;
		Index = -1;
		return false;
	}
}
