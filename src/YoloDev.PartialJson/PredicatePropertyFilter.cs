using System;
using System.Reflection;

namespace YoloDev.PartialJson
{
  class PredicatePropertyFilter : IPropertyFilter
  {
    readonly Predicate<PropertyInfo> _predicate;

    public PredicatePropertyFilter(Predicate<PropertyInfo> predicate)
    {
      _predicate = predicate;
    }

    public bool IncludeProperty(PropertyInfo prop) =>
      _predicate(prop);
  }
}
