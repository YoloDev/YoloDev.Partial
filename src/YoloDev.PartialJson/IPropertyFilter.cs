using System.Reflection;

namespace YoloDev.PartialJson
{
  public interface IPropertyFilter
  {
    bool IncludeProperty(PropertyInfo prop);
  }
}