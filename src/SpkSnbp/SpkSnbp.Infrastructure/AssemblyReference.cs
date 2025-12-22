using System.Reflection;

namespace SpkSnbp.Infrastructure;

public static class AssemblyReference
{
    public readonly static Assembly Assembly = typeof(AssemblyReference).Assembly;
}
