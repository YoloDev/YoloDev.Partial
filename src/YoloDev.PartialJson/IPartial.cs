using System;
using System.Reflection;

namespace YoloDev.PartialJson
{
  public interface IPartial
  {
    object Proxy { get; }
    void Populate(object target);
    void Populate(object target, IPropertyFilter propertyFilter);
    void Populate(object target, Predicate<PropertyInfo> propertyFilter);
  }

  public interface IPartial<T> : IPartial
  {
    new T Proxy { get; }
    void Populate(T target);
    void Populate(T target, IPropertyFilter propertyFilter);
    void Populate(T target, Predicate<PropertyInfo> propertyFilter);
  }
}
