using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using DependencyInjectionContainer.DependencyProvider;

namespace DependencyInjectionContainer.DependenciesConfiguration
{
    public class DependenciesConfiguration : IDependenciesConfiguration
    {
        public Dictionary<Type, List<ImplementationsContainer>> DependenciesDictionary { get; private set; }

        public DependenciesConfiguration()
        {
            this.DependenciesDictionary = new Dictionary<Type, List<ImplementationsContainer>>();
        }

        public void Register<TDependency, TImplementation>(ImplementationsTTL ttl,
            ServiceImplementationNumber number = ServiceImplementationNumber.None) where TDependency : class where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), ttl, number);
        }

        public void Register(Type dependencyType, Type implementType, ImplementationsTTL ttl,
            ServiceImplementationNumber number = ServiceImplementationNumber.None)
        {
            if (!IsDependency(implementType, dependencyType))
            {
                throw new ArgumentException("Incompatible parameters");
            }

            var implContainer = new ImplementationsContainer(implementType, ttl, number);
            if (this.DependenciesDictionary.ContainsKey(dependencyType))
            {
                var index = this.DependenciesDictionary[dependencyType]
                    .FindIndex(container => container.ImplementationsType == implementType);
                if (index != -1)
                {
                    this.DependenciesDictionary[dependencyType].RemoveAt(index);
                }

                this.DependenciesDictionary[dependencyType].Add(implContainer);
            }
            else
            {
                this.DependenciesDictionary.Add(dependencyType, new List<ImplementationsContainer>() { implContainer });
            }
        }

        public bool IsDependency(Type implementation, Type dependency)
        {
            return implementation.IsAssignableFrom(dependency) || implementation.GetInterfaces().Any(i => i.ToString() == dependency.ToString());
        }
    }
}
