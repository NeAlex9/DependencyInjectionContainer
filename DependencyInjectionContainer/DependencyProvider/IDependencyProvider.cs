using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;

namespace DependencyInjectionContainer.DependencyProvider
{
    public interface IDependencyProvider
    {
        TDependency Resolve<TDependency>(ServiceImplementationNumber number = ServiceImplementationNumber.None)
            where TDependency : class;

        object Resolve(Type dependencyType, ServiceImplementationNumber number = ServiceImplementationNumber.None);
    }
}
