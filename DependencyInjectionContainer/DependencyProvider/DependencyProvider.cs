using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;

namespace DependencyInjectionContainer.DependencyProvider
{
    public class DependencyProvider : IDependencyProvider, IDependencyConfigurationValidator
    {
        public IDependenciesConfiguration Configuration { get; private set; }

        public DependencyProvider(IDependenciesConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public TDependency Resolve<TDependency>(ServiceImplementationNumber number = ServiceImplementationNumber.None)
            where TDependency : class
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type dependencyType, ServiceImplementationNumber number = ServiceImplementationNumber.None)
        {
            throw new NotImplementedException();
        }
    }
}
