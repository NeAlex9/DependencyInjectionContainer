using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;

namespace DependencyInjectionContainer.DependencyProvider.ConfigValidator
{
    public class ConfigValidator : IConfigValidator
    {
        private readonly Stack<Type> _nestedTypes;
        private readonly IDependenciesConfiguration _configuration;

        public ConfigValidator(IDependenciesConfiguration configuration)
        {
            this._configuration = configuration;
            this._nestedTypes = new Stack<Type>();
        }

        private bool IsInContainer(Type type)
        {
            return this._configuration.DependenciesDictionary.ContainsKey(type);
        }

        private bool CanBeCreated(Type instanceType)
        {
            this._nestedTypes.Push(instanceType);
            var constructors = instanceType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            bool canBeCreated = true;
            foreach (var constructor in constructors)
            {
                canBeCreated = true;
                var requiredParams = constructor.GetParameters();
                for (int i = 0; i < requiredParams.Length; i++)
                {
                    var parameterType = requiredParams[i].ParameterType.ContainsGenericParameters
                        ? requiredParams[i].ParameterType.GetInterfaces()[0]
                        : requiredParams[i].ParameterType;
                    if (!parameterType.IsInterface ||
                        !IsInContainer(parameterType) ||
                        this._nestedTypes.Contains(parameterType))
                    {
                        canBeCreated = false;
                        break;
                    }
                }

                if (canBeCreated) break;
            }

            this._nestedTypes.Pop();
            return canBeCreated;
        }

        public bool Validate()
        {
            return this._configuration.DependenciesDictionary.Values.
                All(implementations => implementations.
                    All(implementation => CanBeCreated(implementation.ImplementationsType)));
        }
    }
}
