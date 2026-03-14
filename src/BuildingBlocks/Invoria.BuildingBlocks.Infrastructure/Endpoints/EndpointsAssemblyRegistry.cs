using System.Reflection;

namespace Invoria.BuildingBlocks.Infrastructure.Endpoints;

public static class EndpointsAssemblyRegistry
{
    private static readonly HashSet<Assembly> _assemblies = new();

    public static void AddAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
    }

    public static void AddAssemblyFromType<T>()
    {
        _assemblies.Add(typeof(T).Assembly);
    }

    public static Assembly[] GetAssemblies()
    {
        if (_assemblies.Count == 0)
        {
            return new[] { Assembly.GetExecutingAssembly() };
        }
        return _assemblies.ToArray();
    }
}
