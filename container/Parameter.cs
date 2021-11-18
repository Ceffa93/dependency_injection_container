
using System.Diagnostics;

namespace DI
{
    internal abstract class Parameter
    {
        internal abstract object GetArg(ObjectDict services);
        internal abstract void AddDependencies(TypeSet set);

        protected static TypeList FilterImplementsByRoot(TypeList implementList, ServiceDescDict serviceDescriptors, Type parentType)
        {
            if (serviceDescriptors[parentType].roots.Count > 1)
                throw new ContainerException("Service <" + parentType.Name + "> has multiple roots, and has virtual dependencies. Both are not supported at the same time.");

            var parentRoot = serviceDescriptors[parentType].roots.First();

            return implementList.FindAll(implType => serviceDescriptors[implType].roots.Contains(parentRoot));
        }
    };

    internal class ArrayParameter : Parameter
    {
        public ArrayParameter(ImplementDict implementDict, ServiceDescDict serviceDescriptors, Type arrayType, Type parentType)
        {
            var baseType = arrayType.GetElementType();
            Debug.Assert(baseType is not null);
            this.baseType = baseType;

            if (implementDict.TryGetValue(baseType, out var implementList))
                types = FilterImplementsByRoot(implementList, serviceDescriptors, parentType);
            else
                types = new();
        }

        internal override void AddDependencies(TypeSet set)
        {
            set.UnionWith(types);
        }

        internal override object GetArg(ObjectDict services)
        {
            var arg = Array.CreateInstance(baseType, types.Count);

            for (var i = 0; i < types.Count; i++)
                arg.SetValue(services[types[i]], i);

            return arg;
        }

        internal readonly TypeList types;
        internal readonly Type baseType;
    }

    internal class RegularParameter : Parameter
    {
        public RegularParameter(ImplementDict implementDict, ServiceDescDict serviceDescriptors, Type type, Type parentType)
        {
            Type? res;
            if ((res = TryGetDirectType(serviceDescriptors, type, parentType)) is not null)
                this.type = res;
            else if ((res = TryGetImplementType(implementDict, serviceDescriptors, type, parentType)) is not null)
                this.type = res;
            else
                throw new ContainerException("Regular Parameter <" + type.Name + "> required by <" + parentType.Name + "> cannot be resolved!");
        }

        internal override void AddDependencies(TypeSet set)
        {
            set.Add(type);
        }
        internal override object GetArg(ObjectDict services)
        {
            return services[type];
        }

        private static Type? TryGetDirectType(ServiceDescDict serviceDescriptors, Type type, Type parentType)
        {
            if (!serviceDescriptors.ContainsKey(type))
                return null;

            var roots = serviceDescriptors[type].roots;
            var parentRoots = serviceDescriptors[parentType].roots;
            if (!roots.Overlaps(parentRoots))
                return null;

            return type;
        }
        private static Type? TryGetImplementType(ImplementDict implementDict, ServiceDescDict serviceDescriptors, Type type, Type parentType)
        {
            if (!implementDict.TryGetValue(type, out var implementList))
                return null;

            var implements = FilterImplementsByRoot(implementList, serviceDescriptors, parentType);

            if (implements.Count == 0)
                throw new ContainerException("No implementation found for type <" + type.Name + ">!");

            if (implements.Count > 1)
                throw new ContainerException("Type <" + type.Name + "> has multiple ambiguous implementations!");

            return implements.First();
        }

        internal readonly Type type;
    };
}
