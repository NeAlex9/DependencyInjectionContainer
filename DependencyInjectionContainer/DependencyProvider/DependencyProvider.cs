﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;

namespace DependencyInjectionContainer.DependencyProvider
{
    public class DependencyProvider : IDependencyProvider
    {
        public IDependenciesConfiguration Configuration { get; private set; }
        public Dictionary<Type, List<object>> Singletons { get; private set; }

        public DependencyProvider(IDependenciesConfiguration configuration)
        {
            if (!Validate())
            {
                throw new ArgumentException("Wrong configuration");
            }

            this.Singletons = new Dictionary<Type, List<object>>();
            this.Configuration = configuration;
        }

        private bool Validate()
        {
            return true;
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
            if (this.Configuration.DependenciesDictionary.ContainsKey(dependencyType))
            {
                return this.Configuration.DependenciesDictionary[dependencyType].FindLast(container => container.ImplNumber == number);
            }

            return null;
        }

        private object CreateInstance(Type implementationType, ServiceImplementationNumber number)
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
                    var implementationContainer = GetImplementationsContainerLast(parameterInfo.ParameterType, number);
                    parameter = CreateInstance(implementationContainer.ImplementationsType, number);
                }
                else
                {
                    parameter = Activator.CreateInstance(parameterInfo.ParameterType);
                }

                generatedParams.Add(parameter);
            }

            return constructor.Invoke(generatedParams.ToArray());
        }

        private void AddToSingletons(Type dependencyType, object implementation)
        {
            if (this.Singletons.ContainsKey(dependencyType))
            {
                this.Singletons[dependencyType].Add(implementation);
            }
            else
            {
                this.Singletons = new Dictionary<Type, List<object>>()
                {
                    {
                        dependencyType, new List<object>() {implementation}
                    }
                };
            }
        }

        private List<ImplementationsContainer> GetRemainingToСreateContainers(Type dependencyType)
        {
            var remainingImplTypes = new List<ImplementationsContainer>();
            var containers = this.Configuration.DependenciesDictionary[dependencyType];
            foreach (var container in containers)
            {
                if (!(this.Singletons.ContainsKey(dependencyType) && this.Singletons[dependencyType].Any(obj => obj.GetType() == container.ImplementationsType)))
                {
                    remainingImplTypes.Add(container);
                }
            }

            return remainingImplTypes;
        }

        private IList CreateEnumerable(List<ImplementationsContainer> remainingToСreateContainers, Type dependencyType, ServiceImplementationNumber number)
        {
            var implementationList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dependencyType));
            this.Singletons.
                Where(keyValuePair => keyValuePair.Key == dependencyType).
                Select(keyValuePair => keyValuePair.Value).
                ToList().
                ForEach(list => list.
                    ForEach(implement => implementationList.Add(implement)));
            remainingToСreateContainers.ForEach(container => implementationList.Add(CreateInstance(container.ImplementationsType, number)));
            return implementationList;
        }

        private object CreateGeneric(Type dependencyType, Type implementationsType, ServiceImplementationNumber number)
        {
            return CreateInstance(implementationsType.IsGenericTypeDefinition ?
                implementationsType.MakeGenericType(dependencyType.GetGenericArguments()) :
                implementationsType, number);
        }

        public object Resolve(Type dependencyType, ServiceImplementationNumber number = ServiceImplementationNumber.None)
        {
            object result;
            if (this.Singletons.ContainsKey(dependencyType))
            {
                result = this.Singletons[dependencyType][this.Singletons[dependencyType].Count - 1];
            }
            else if (IsIEnumerable(dependencyType))
            { 
                var remainingToCreateContainers = GetRemainingToСreateContainers(dependencyType.GetGenericArguments()[0]);
                var implementations = CreateEnumerable(remainingToCreateContainers, dependencyType.GetGenericArguments()[0], number);
                foreach (var container in remainingToCreateContainers.
                    Select(container => container).
                    Where(container => container.TimeToLive == ImplementationsTTL.Singleton))
                {
                    object requiredImplementation = null;
                    foreach (var implementation in implementations)
                    {
                        if (implementation.GetType() == container.ImplementationsType) requiredImplementation = implementation;
                    }

                    if (requiredImplementation is not null) this.AddToSingletons(dependencyType.GetGenericArguments()[0], requiredImplementation);
                }

                result = implementations;
            }
            else if (dependencyType.IsGenericType)
            {
                var container = GetImplementationsContainerLast(dependencyType, number);
                container ??= GetImplementationsContainerLast(dependencyType.GetGenericTypeDefinition(), number);
                result = CreateGeneric(dependencyType, container.ImplementationsType, number);
                if (container.TimeToLive == ImplementationsTTL.Singleton) this.AddToSingletons(dependencyType, result);
            }
            else
            {
                var container = GetImplementationsContainerLast(dependencyType, number);
                result = CreateInstance(container.ImplementationsType, number);
                if (container.TimeToLive == ImplementationsTTL.Singleton) this.AddToSingletons(dependencyType, result);
            }

            return result;
        }

        public TDependency Resolve<TDependency>(ServiceImplementationNumber number = ServiceImplementationNumber.None)
            where TDependency : class
        {
            return (TDependency)Resolve(typeof(TDependency), number);
        }
    }
}
