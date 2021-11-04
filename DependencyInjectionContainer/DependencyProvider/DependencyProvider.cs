using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        private readonly Dictionary<Type, List<SingletonContainer>> _singletons;

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
            if (this.IsIEnumerable(dependencyType))
            {
                result = CreateEnumerable(dependencyType.GetGenericArguments()[0]);
            }
            else
            {
                ImplementationsContainer container = GetImplContainerByDependencyType(dependencyType, number);
                Type requiredType = GetGeneratedType(dependencyType, container.ImplementationsType);
                result = this.ResolveNonIEnumerable(container, dependencyType);
            }

            return result;
        }

        private object GetSingleton(ImplementationsContainer container, Type dependencyType)
        {
            if (!this._singletons.ContainsKey(dependencyType) ||
                !TryGetSingletonInstance(this._singletons[dependencyType], container.ImplementationsType, out object instance))
            {
                return CreateInstance(container.ImplementationsType);
            }

            return instance;
        }

        private object ResolveNonIEnumerable(ImplementationsContainer container, Type dependencyType)
        {
            object result;
            if (container.TimeToLive != ImplementationsTTL.Singleton)
            {
                result = CreateInstance(container.ImplementationsType);
            }
            else
            {
                lock (this._singletons)
                {
                    var singleton = this.GetSingleton(container, dependencyType);
                    if (!this._singletons.ContainsKey(dependencyType) || !TryGetSingletonInstance(this._singletons[dependencyType], container.ImplementationsType, out object instance))
                    {
                        this.AddToSingletons(dependencyType, singleton, container.ImplNumber);
                    }

                    result = singleton;
                }
            }

            return result;
        }

        public TDependency Resolve<TDependency>(ServiceImplementationNumber number = ServiceImplementationNumber.Any)
            where TDependency : class
        {
            return (TDependency)Resolve(typeof(TDependency), number);
        }

        private bool TryGetSingletonInstance(List<SingletonContainer> singletonContainers, Type requiredType, out object instance)
        {
            instance = singletonContainers.
                Find(singletonContainer => singletonContainer.Instance.GetType() == requiredType);
            return instance is null;
        }

        private ImplementationsContainer GetImplContainerByDependencyType(Type dependencyType, ServiceImplementationNumber number)
        {
            ImplementationsContainer container;
            if (dependencyType.IsGenericType)
            {
                container = GetImplementationsContainerLast(dependencyType, number);
                container ??= GetImplementationsContainerLast(dependencyType.GetGenericTypeDefinition(), number);

            }
            else
            {
                container = GetImplementationsContainerLast(dependencyType, number);
            }

            return container;
        }

        private bool IsIEnumerable(Type dependencyType)
        {
            return dependencyType.GetInterfaces().Any(i => i.Name == "IEnumerable");
        }

        private object CreateInstance(Type implementationType)
        {
            var constructors = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            foreach (var constructor in constructors)
            {
                var constructorParams = constructor.GetParameters();
                var generatedParams = new List<dynamic>();
                foreach (var parameterInfo in constructorParams)
                {
                    dynamic parameter;
                    if (parameterInfo.ParameterType.IsInterface)
                    {
                        var number = parameterInfo.GetCustomAttribute<DependencyKeyAttribute>()?.ImplNumber ?? ServiceImplementationNumber.Any;
                        parameter = Resolve(parameterInfo.ParameterType, number);
                    }
                    else
                    {
                        break;
                    }

                    generatedParams.Add(parameter);
                }

                return constructor.Invoke(generatedParams.ToArray());
            }

            throw new ArgumentException("Cannot create instance of class");
        }

        private Type GetGeneratedType(Type dependencyType, Type implementationType)
        {
            if (dependencyType.IsGenericType && implementationType.IsGenericTypeDefinition)
            {
                return implementationType.MakeGenericType(dependencyType.GetGenericArguments());

            }

            return implementationType;
        }

        private IList CreateEnumerable(Type dependencyType)
        {
            var implementationList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dependencyType));
            var implementationsContainers = this._configuration.DependenciesDictionary[dependencyType];
            foreach (var implementationContainer in implementationsContainers)
            {
                var instance = this.ResolveNonIEnumerable(implementationContainer, dependencyType);
                implementationList.Add(instance);
            }

            return implementationList;
        }

        private ImplementationsContainer GetImplementationsContainerLast(Type dependencyType,
            ServiceImplementationNumber number)
        {
            if (this._configuration.DependenciesDictionary.ContainsKey(dependencyType))
            {
                return this._configuration.DependenciesDictionary[dependencyType]
                    .FindLast(container => number.HasFlag(container.ImplNumber));
            }

            return null;
        }

        private void AddToSingletons(Type dependencyType, object implementation, ServiceImplementationNumber number)
        {
            if (this._singletons.ContainsKey(dependencyType))
            {
                this._singletons[dependencyType].Add(new SingletonContainer(implementation, number));
            }
            else
            {
                this._singletons.Add(dependencyType, new List<SingletonContainer>()
                {
                    new SingletonContainer(implementation, number)
                });
            }
        }
    }
}

/*
public object Resolve(Type dependencyType, ServiceImplementationNumber number = ServiceImplementationNumber.Any)
        {
            object result;
            lock(this._configuration)
            {
                if (DefineObjectType(dependencyType, number) == "Singleton")
                {
                    result = this._singletons[dependencyType]
                        .FindLast(container => container.ImplNumber.HasFlag(number)).Instance;
                }
                else if (DefineObjectType(dependencyType, number) == "IEnumerable")
                {
                    var remainingToCreateContainers =
                        GetRemainingTypesToСreateContainers(dependencyType.GetGenericArguments()[0]);
                    var implementations = CreateEnumerable(remainingToCreateContainers,
                        dependencyType.GetGenericArguments()[0]);
                    foreach (var container in remainingToCreateContainers.Select(container => container)
                        .Where(container => container.TimeToLive == ImplementationsTTL.Singleton))
                    {
                        object requiredImplementation = null;
                        foreach (var implementation in implementations)
                        {

                            if (implementation.GetType() == container.ImplementationsType)
                                requiredImplementation = implementation;
                        }

                        if (requiredImplementation is not null)
                            this.AddToSingletons(dependencyType.GetGenericArguments()[0], requiredImplementation,
                                number);
                    }

                    result = implementations;
                }
                else if (DefineObjectType(dependencyType, number) == "Generic")
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
            }

            return result;
        }

        public TDependency Resolve<TDependency>(ServiceImplementationNumber number = ServiceImplementationNumber.Any)
            where TDependency : class
        {
            return (TDependency)Resolve(typeof(TDependency), number);
        }

        private string DefineObjectType(Type type, ServiceImplementationNumber number)
        {
            if (this.IsContainNamedSingleton(type, number))
            {
                return "Singleton";
            }

            if (IsIEnumerable(type))
            {
                return "IEnumerable";
            }

            if (type.IsGenericType)
            {
                return "Generic";
            }

            return "Simple";
        }

        private bool IsIEnumerable(Type dependencyType)
        {
            return dependencyType.GetInterfaces().Any(i => i.Name == "IEnumerable");
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
            var constructors = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            foreach (var constructor in constructors)
            {
                var constructorParams = constructor.GetParameters();
                var generatedParams = new List<dynamic>();
                foreach (var parameterInfo in constructorParams)
                {
                    dynamic parameter;
                    if (parameterInfo.ParameterType.IsInterface)
                    {
                        var number = parameterInfo.GetCustomAttribute<DependencyKeyAttribute>()?.ImplNumber ?? ServiceImplementationNumber.Any;
                        parameter = Resolve(parameterInfo.ParameterType, number);
                    }
                    else
                    {
                        break;
                    }

                    generatedParams.Add(parameter);
                }

                return constructor.Invoke(generatedParams.ToArray());
            }

            throw new ArgumentException("Cannot create instance of class");
        }

        private void AddToSingletons(Type dependencyType, object implementation, ServiceImplementationNumber number)
        {
            if (this._singletons.ContainsKey(dependencyType))
            {
                this._singletons[dependencyType].Add(new SingletonContainer(implementation, number));
            }
            else
            {
                this._singletons.Add(dependencyType, new List<SingletonContainer>()
                {
                    new SingletonContainer(implementation, number)
                });
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
            remainingToСreateContainers.ForEach(container => implementationList.Add(CreateInstance(container.ImplementationsType)));  ////!!!!!
            return implementationList;
        }

        private object CreateGeneric(Type dependencyType, Type implementationsType)
        {
            return CreateInstance(implementationsType.IsGenericTypeDefinition ?
                implementationsType.MakeGenericType(dependencyType.GetGenericArguments()) :
                implementationsType);
        }
*/
