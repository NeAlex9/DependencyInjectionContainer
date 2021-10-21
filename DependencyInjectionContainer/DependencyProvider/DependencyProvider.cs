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
        public Dictionary<Type, object> Singletons { get; private set; }

        public DependencyProvider(IDependenciesConfiguration configuration)
        {
            if (!Validate())
            {
                throw new ArgumentException("Wrong configuration");
            }

            this.Singletons = new Dictionary<Type, object>();
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

        private ImplementationsContainer GetImplementationsContainer(Type dependencyType, ServiceImplementationNumber number)
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
                    //parameterInfo.ParameterType.GetInterfaces()[0];
                    /*var dependencyType = parameterInfo.ParameterType.IsGenericParameter
                        ? parameterInfo.ParameterType.GetInterfaces()[0]
                        : parameterInfo.ParameterType;*/
                    var dependencyType = parameterInfo.ParameterType;
                    var implementationContainer = GetImplementationsContainer(dependencyType, number);
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

        public object Resolve(Type dependencyType, ServiceImplementationNumber number = ServiceImplementationNumber.None)
        {
            object result;
            if (this.Singletons.ContainsKey(dependencyType))
            {
                result = this.Singletons[dependencyType];
            }
            else if (IsIEnumerable(dependencyType))
            {
                var implementations = this.Configuration.DependenciesDictionary[dependencyType].Select(container => container.ImplementationsType);
                result = implementations.Select(type => CreateInstance(type, number));
            }
            else if (dependencyType.IsGenericType)
            { 
                var container = GetImplementationsContainer(dependencyType.GetGenericTypeDefinition(), number);
                container ??= GetImplementationsContainer(dependencyType, number);
                result = CreateInstance(container.ImplementationsType.IsGenericTypeDefinition ? container.ImplementationsType.MakeGenericType(dependencyType.GetGenericArguments()) : container.ImplementationsType, number);

                if (container.TimeToLive == ImplementationsTTL.Singleton)
                {
                    this.Singletons.Add(dependencyType, result);
                }
            }
            else 
            { 
                var container = GetImplementationsContainer(dependencyType, number);
                result = CreateInstance(container.ImplementationsType, number);
                if (container.TimeToLive == ImplementationsTTL.Singleton)
                {
                    this.Singletons.Add(dependencyType, result);
                }
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
