using System;
using System.Reflection;

namespace YoloDev.PartialJson
{
  internal sealed class FuncNameConverter : INameConverter
  {
    readonly Func<PropertyInfo, string> _func;

    public FuncNameConverter(Func<PropertyInfo, string> func)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    public string GetName(PropertyInfo property) =>
      _func(property);
  }
}
