using System.Reflection;

namespace DI
{
    internal class Constructor
    {
        public Constructor(Type type, ServiceDescDict serviceDescriptors, ImplementDict implementDict)
        {
            constrInfo = GetConstructorInfo(type);

            var paramInfos = constrInfo.GetParameters();

            parameters = new(paramInfos.Length);

            foreach (var paramInfo in paramInfos)
                addParamDescriptor(paramInfo, implementDict, serviceDescriptors, type);
        }
      
        internal TypeSet GetDependencies()
        {
            var set = new TypeSet();

            foreach (var param in parameters)
                param.AddDependencies(set);

            return set;
        }

        internal object Construct(ObjectDict services)
        {
            var arguments = new List<object>(parameters.Count);

            foreach (var desc in parameters)
                arguments.Add(desc.GetArg(services));

            return constrInfo.Invoke(arguments.ToArray());
        }

        private static ConstructorInfo GetConstructorInfo(Type type)
        {
            var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            if (constrList.Length > 1)
                throw new ContainerException("Class <" + type.Name + "> must specify a single public constructor!");

            return constrList.First();
        }

        private void addParamDescriptor(ParameterInfo paramInfo, ImplementDict implementDict, ServiceDescDict serviceDescriptors, Type parentType)
        {
            var type = paramInfo.ParameterType;

            if (type.IsArray)
                parameters.Add(new ArrayParameter(implementDict, serviceDescriptors, type, parentType));
            else
                parameters.Add(new RegularParameter(implementDict, serviceDescriptors, type, parentType));
        }

        internal readonly ConstructorInfo constrInfo;
        internal readonly List<Parameter> parameters;
    };
}
