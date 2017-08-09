namespace YoloDev.PartialJson
{
  public class PartialProxy<T> : IPartialProxy<T>
  {
    readonly IPartial<T> _partial;

    public PartialProxy(IPartial<T> partial)
    {
      _partial = partial;
    }

    public IPartial<T> Partial => _partial;

    IPartial IPartialProxy.Partial => _partial;
  }
}
