
using System.Diagnostics;

namespace DI
{
    internal abstract class Parameter
    {
        internal abstract void RemoveFromSet(TypeSet set);
        internal abstract bool TryAddToStack(TypeStack stack, ObjectDict services);
        internal abstract object GetArg(ObjectDict services);

        protected static bool HasCommonRoot(ServiceDescDict serviceDescriptors, Type childType, Type parentType)
        {
            var childRoots = serviceDescriptors[childType].roots;
            var parentRoots = serviceDescriptors[parentType].roots;
            return childRoots.Overlaps(parentRoots);
        }

        protected static Type[] FilterImplementsByRoot(TypeList implementList, ServiceDescDict serviceDescriptors, Type parentType)
        {
            var filteredList = implementList.FindAll(implType => HasCommonRoot(serviceDescriptors, implType, parentType));
            return filteredList.ToArray();
        }

        protected static bool TryAddTypeToStack(TypeStack stack, ObjectDict services, Type type)
        {
            if (services.ContainsKey(type))
                return false;

            stack.Push(type);
            return true;
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
                types = new Type[0];
        }

        internal override void RemoveFromSet(TypeSet set)
        {
            foreach (var type in types)
                set.Remove(type);
        }

        internal override bool TryAddToStack(TypeStack stack, ObjectDict services)
        {
            bool bAdded = false;

            foreach (var type in types)
                bAdded |= TryAddTypeToStack(stack, services, type);

            return bAdded;
        }

        internal override object GetArg(ObjectDict services)
        {
            var arg = Array.CreateInstance(baseType, types.Length);

            for (var i = 0; i < types.Length; i++)
                arg.SetValue(services[types[i]], i);

            return arg;
        }

        public Type[] types;
        public Type baseType;
    }

    internal class RegularParameter : Parameter
    {
        public RegularParameter(ImplementDict implementDict, ServiceDescDict serviceDescriptors, Type type, Type parentType)
        {
            if (type == parentType)
                throw new ContainerException("Service <" + parentType.Name + "> cannot depend on itself!");

            if (serviceDescriptors.ContainsKey(type) && HasCommonRoot(serviceDescriptors, type, parentType))
                this.type = type;
            else if (implementDict.TryGetValue(type, out var implementList))
                this.type = FilterImplementsByRoot(implementList, serviceDescriptors, parentType).First();
            else
                throw new ContainerException("Regular Parameter <" + type.Name + "> required by <" + parentType.Name + "> cannot be resolved!");
        }

        internal override void RemoveFromSet(TypeSet set)
        {
            set.Remove(type);
        }
        internal override bool TryAddToStack(TypeStack stack, ObjectDict services)
        {
            return TryAddTypeToStack(stack, services, type);
        }
        internal override object GetArg(ObjectDict services)
        {
            return services[type];
        }

        public Type type;
    };
}
