using System.Reflection;

namespace YoloDev.PartialJson
{
  public interface INameConverter
  {
    string GetName(PropertyInfo property);
  }
}
