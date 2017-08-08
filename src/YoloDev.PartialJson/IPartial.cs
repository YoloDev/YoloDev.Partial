namespace YoloDev.PartialJson
{
  public interface IPartial
  {
    object Proxy { get; }
  }

  public interface IPartial<T> : IPartial
  {
    new T Proxy { get; }
  }
}
