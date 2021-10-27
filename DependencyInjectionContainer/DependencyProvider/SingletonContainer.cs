using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;

namespace DependencyInjectionContainer.DependencyProvider
{
    public class SingletonContainer
    {
        public readonly ServiceImplementationNumber ImplNumber;

        public readonly object Instance;

        public SingletonContainer(object instance, ServiceImplementationNumber number)
        {
            this.ImplNumber = number;
            this.Instance = instance;
        }
    }
}
