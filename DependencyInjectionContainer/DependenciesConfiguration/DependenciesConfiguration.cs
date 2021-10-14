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

        public void Register<TDependency, TImplementation>(ServiceImplementationNumber number = ServiceImplementationNumber.None,
            ImplementationsTTL ttl = ImplementationsTTL.InstancePerDependency)
            where TDependency : class
            where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), number, ttl);
        }

        public void Register(Type dependencyType, Type implementType, ServiceImplementationNumber number = ServiceImplementationNumber.None,
            ImplementationsTTL ttl = ImplementationsTTL.InstancePerDependency)
        {
            if (!dependencyType.IsAssignableFrom(implementType))
            {
                throw new ArgumentException("Incompatible parameters");
            }

            var implContainer = new ImplementationsContainer(implementType, ttl, number);
            if (this.DependenciesDictionary.TryGetValue(dependencyType, out List<ImplementationsContainer> implementations))
            {
                implementations.Add(implContainer);
            }
            else
            {
                this.DependenciesDictionary.Add(dependencyType, new List<ImplementationsContainer>() { implContainer });
            }
        }
    }
}
