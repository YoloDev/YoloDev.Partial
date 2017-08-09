using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace YoloDev.PartialJson
{
  public interface IPartial
  {
    object Proxy { get; }
    void Populate(object target);
    void Populate(object target, IPropertyFilter propertyFilter);
    void Populate(object target, Predicate<PropertyInfo> propertyFilter);
    bool IsSet(PropertyInfo property);
    IImmutableDictionary<string, object> GetUpdates();
    IImmutableDictionary<string, object> GetUpdates(INameConverter nameConverter);
    IImmutableDictionary<string, object> GetUpdates(Func<PropertyInfo, string> nameConverter);
  }

  public interface IPartial<T> : IPartial
  {
    new T Proxy { get; }
    void Populate(T target);
    void Populate(T target, IPropertyFilter propertyFilter);
    void Populate(T target, Predicate<PropertyInfo> propertyFilter);
    bool IsSet<R>(Expression<Func<T, R>> property);
  }
}
