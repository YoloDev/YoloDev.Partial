using System;
using System.Reflection;

namespace YoloDev.PartialJson
{
  internal sealed class PredicatePropertyFilter : IPropertyFilter
  {
    readonly Predicate<PropertyInfo> _predicate;

    public PredicatePropertyFilter(Predicate<PropertyInfo> predicate)
    {
      _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    public bool IncludeProperty(PropertyInfo prop) =>
      _predicate(prop);
  }
}
