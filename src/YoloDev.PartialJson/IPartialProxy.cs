using Newtonsoft.Json;

namespace YoloDev.PartialJson
{
  public interface IPartialProxy
  {
    [JsonIgnore]
    IPartial Partial { get; }
  }

  public interface IPartialProxy<T> : IPartialProxy
  {
    [JsonIgnore]
    new IPartial<T> Partial { get; }
  }
}
