using System.Reflection;

namespace YoloDev.PartialJson
{
  class DefaultNameConverter : INameConverter
  {
    public static INameConverter Instance { get; } = new DefaultNameConverter();

    private DefaultNameConverter() { }

    public string GetName(PropertyInfo property) =>
      property.Name;
  }
}
