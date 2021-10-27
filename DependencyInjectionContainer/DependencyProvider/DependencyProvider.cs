using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using DependencyInjectionContainer.DependencyProvider.ConfigValidator;

namespace DependencyInjectionContainer.DependencyProvider
{
    public class DependencyProvider : IDependencyProvider
    {
        private readonly IDependenciesConfiguration _configuration;
        private Dictionary<Type, List<SingletonContainer>> _singletons;

        public DependencyProvider(IDependenciesConfiguration configuration)
        {
            IConfigValidator configValidator = new ConfigValidator.ConfigValidator(configuration);
            if (!configValidator.Validate())
            {
                throw new ArgumentException("Wrong configuration");
            }

            this._singletons = new Dictionary<Type, List<SingletonContainer>>();
            this._configuration = configuration;
        }

        public object Resolve(Type dependencyType, ServiceImplementationNumber number = ServiceImplementationNumber.Any)
        {
            object result;
            if (IsContainNamedSingleton(dependencyType, number))
            {
                result = this._singletons[dependencyType].FindLast(container => container.ImplNumber.HasFlag(number)).Instance;
            }
            else if (IsIEnumerable(dependencyType))
            {
                var remainingToCreateContainers = GetRemainingTypesToСreateContainers(dependencyType.GetGenericArguments()[0]);
                var implementations = CreateEnumerable(remainingToCreateContainers, dependencyType.GetGenericArguments()[0]);
                foreach (var container in remainingToCreateContainers.
                    Select(container => container).
                    Where(container => container.TimeToLive == ImplementationsTTL.Singleton))    ////// check inner instances
                {
                    object requiredImplementation = null;
                    foreach (var implementation in implementations)
                    {
                        if (implementation.GetType() == container.ImplementationsType)
                            requiredImplementation = implementation;
                    }

                    if (requiredImplementation is not null) 
                        this.AddToSingletons(dependencyType.GetGenericArguments()[0], requiredImplementation, number);
                }

                result = implementations;
            }
            else if (dependencyType.IsGenericType)
            {
                var container = GetImplementationsContainerLast(dependencyType, number);
                container ??= GetImplementationsContainerLast(dependencyType.GetGenericTypeDefinition(), number);
                result = CreateGeneric(dependencyType, container.ImplementationsType);
                if (container.TimeToLive == ImplementationsTTL.Singleton) 
                    this.AddToSingletons(dependencyType, result, number);
            }
            else
            {
                var container = GetImplementationsContainerLast(dependencyType, number);
                result = CreateInstance(container.ImplementationsType);
                if (container.TimeToLive == ImplementationsTTL.Singleton) 
                    this.AddToSingletons(dependencyType, result, number);
            }

            return result;
        }

        public TDependency Resolve<TDependency>(ServiceImplementationNumber number = ServiceImplementationNumber.Any)
            where TDependency : class
        {
            return (TDependency)Resolve(typeof(TDependency), number);
        }

        private bool IsIEnumerable(Type dependencyType)
        {
            return dependencyType.GetInterfaces().Any(i => i.Name == "IEnumerable");
        }

        private bool IsCustomType(Type type)
        {
            var systemTypes = typeof(Assembly).Assembly.GetExportedTypes().ToList();
            return (type.IsClass || type.IsInterface) && !type.IsArray && !systemTypes.Contains(type);
        }

        private ImplementationsContainer GetImplementationsContainerLast(Type dependencyType, ServiceImplementationNumber number)
        {
            if (this._configuration.DependenciesDictionary.ContainsKey(dependencyType))
            {
                return this._configuration.DependenciesDictionary[dependencyType].FindLast(container => number.HasFlag(container.ImplNumber));
            }

            return null;
        }

        private bool IsContainNamedSingleton(Type dependencyType, ServiceImplementationNumber number)
        {
            var lst = this._singletons.ContainsKey(dependencyType) ? this._singletons[dependencyType] : null;
            return lst?.Find(container => container.ImplNumber.HasFlag(number)) is not null;
        }

        private object CreateInstance(Type implementationType)
        {
            var constructor = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)?.FirstOrDefault();
            if (constructor is null)
            {
                throw new NullReferenceException("constructor is private");
            }

            var constructorParams = constructor.GetParameters();
            var generatedParams = new List<dynamic>();
            foreach (var parameterInfo in constructorParams)
            {
                dynamic parameter;
                if (IsCustomType(parameterInfo.ParameterType))
                {
                    var implementationContainer = GetImplementationsContainerLast(parameterInfo.ParameterType, ServiceImplementationNumber.Any);
                    parameter = CreateInstance(implementationContainer.ImplementationsType);
                }
                else
                {
                    parameter = Activator.CreateInstance(parameterInfo.ParameterType);
                }

                generatedParams.Add(parameter);
            }

            return constructor.Invoke(generatedParams.ToArray());
        }

        private void AddToSingletons(Type dependencyType, object implementation, ServiceImplementationNumber number)
        {
            if (this._singletons.ContainsKey(dependencyType))
            {
                this._singletons[dependencyType].Add(new SingletonContainer(implementation, number));
            }
            else
            {
                this._singletons = new Dictionary<Type, List<SingletonContainer>>()
                {
                    {
                        dependencyType, new List<SingletonContainer>() {new SingletonContainer(implementation, number)}
                    }
                };
            }
        }

        private List<ImplementationsContainer> GetRemainingTypesToСreateContainers(Type dependencyType)
        {
            var remainingImplTypes = new List<ImplementationsContainer>();
            var containers = this._configuration.DependenciesDictionary[dependencyType];
            foreach (var container in containers)
            {
                if (!(this._singletons.ContainsKey(dependencyType) && this._singletons[dependencyType].Any(obj => obj.Instance.GetType() == container.ImplementationsType)))
                {
                    remainingImplTypes.Add(container);
                }
            }

            return remainingImplTypes;
        }

        private IList CreateEnumerable(List<ImplementationsContainer> remainingToСreateContainers, Type dependencyType)
        {
            var implementationList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dependencyType));
            this._singletons.
                Where(keyValuePair => keyValuePair.Key == dependencyType).
                Select(keyValuePair => keyValuePair.Value).
                ToList().
                ForEach(list => list.
                    ForEach(singletonContainer => implementationList.Add(singletonContainer.Instance)));
            remainingToСreateContainers.ForEach(container => implementationList.Add(CreateInstance(container.ImplementationsType)));
            return implementationList;
        }

        private object CreateGeneric(Type dependencyType, Type implementationsType)
        {
            return CreateInstance(implementationsType.IsGenericTypeDefinition ?
                implementationsType.MakeGenericType(dependencyType.GetGenericArguments()) :
                implementationsType);
        }
    }
}