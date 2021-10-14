using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using DependencyInjectionContainer.DependencyProvider;

namespace DependencyInjectionContainer
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DependencyKeyAttribute : System.Attribute
    {
        public ServiceImplementationNumber ImplNumber { get; private set; }

        public DependencyKeyAttribute(ServiceImplementationNumber number)
        {
            this.ImplNumber = number;
        }
    }
}
