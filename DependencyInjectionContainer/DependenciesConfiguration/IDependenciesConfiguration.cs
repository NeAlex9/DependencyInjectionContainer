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
        void Register<TDependency, TImplementation>(ServiceImplementationNumber number = ServiceImplementationNumber.None,
            ImplementationsTTL ttl = ImplementationsTTL.InstancePerDependency)  ///?????????
            where TDependency : class
            where TImplementation : TDependency;

        void Register(Type dependencyType, Type implementType, ServiceImplementationNumber number = ServiceImplementationNumber.None,
            ImplementationsTTL ttl = ImplementationsTTL.InstancePerDependency);
    }
}
