using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using DependencyInjectionContainer.DependencyProvider;

namespace DependencyInjectionContainer.DependenciesConfiguration
{
    public interface IDependenciesConfiguration
    {
        public Dictionary<Type, List<ImplementationsContainer>> DependenciesDictionary { get; }

        void Register<TDependency, TImplementation>(ImplementationsTTL ttl,
            ServiceImplementationNumber number = ServiceImplementationNumber.None)  ///?????????
            where TDependency : class
            where TImplementation : TDependency;

        void Register(Type dependencyType, Type implementType,
            ImplementationsTTL ttl,
            ServiceImplementationNumber number = ServiceImplementationNumber.None);
    }
}
