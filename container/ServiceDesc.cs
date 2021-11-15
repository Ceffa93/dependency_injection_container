namespace DI
{
    public class ServiceDesc
    {
        public ServiceDesc(Type type)
        {
            this.type = type;
            implements = new();
            roots = new() { };
        }

        public ServiceDesc Is<T>()
           where T : class
        {
            var parentType = typeof(T);

            if (parentType == type)
                throw new ContainerException("Type <" + type.Name + "> cannot implement itself!");

            if (!parentType.IsAssignableFrom(type))
                throw new ContainerException("<" + parentType.Name + "> is not a parent class/interface of <" + type.Name + ">!");

            implements.Add(parentType);
            return this;
        }

        internal void AddImplementsToDict(ImplementDict implementDict)
        {
            foreach (var impl in implements)
                if (implementDict.TryGetValue(impl, out var list))
                    list.Add(type);
                else
                    implementDict[impl] = new() { type };
        }

        internal readonly Type type;
        internal readonly TypeSet implements;
        internal readonly TypeSet roots;
    };
}
