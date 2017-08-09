using System.Reflection;

namespace YoloDev.PartialJson
{
  public class DefaultPropertyFilter : IPropertyFilter
  {
    public static DefaultPropertyFilter Instance { get; } =
      new DefaultPropertyFilter();

    private DefaultPropertyFilter() { }

    public bool IncludeProperty(PropertyInfo prop) => true;
  }
}
