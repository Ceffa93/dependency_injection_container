global using ServiceDescDict = System.Collections.Generic.Dictionary<System.Type, DI.ServiceDesc>;
global using ObjectDict = System.Collections.Generic.Dictionary<System.Type, object>;
global using TypeSet = System.Collections.Generic.HashSet<System.Type>;
global using TypeList = System.Collections.Generic.List<System.Type>;
global using ImplementDict = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Type>>;

namespace DI
{
    public class ContainerException : Exception
    {
        public ContainerException(string? message) : base(message) { }
    };
}