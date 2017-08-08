namespace YoloDev.PartialJson
{
  public interface IPartialProxy
  {
    IPartial Partial { get; }
  }

  public interface IPartialProxy<T> : IPartialProxy
  {
    new IPartial<T> Partial { get; }
  }
}
