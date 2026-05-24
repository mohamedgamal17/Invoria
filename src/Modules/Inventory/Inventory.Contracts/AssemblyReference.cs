using System.Reflection;

namespace Invoria.Inventory.Contracts;

public class AssemblyReference
{
    public static Assembly Assembly { get; } = typeof(AssemblyReference).Assembly;
}
