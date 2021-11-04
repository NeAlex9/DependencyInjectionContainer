using System;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;

namespace DependencyInjectionContainer
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DependencyKeyAttribute : Attribute
    {
        public ServiceImplementationNumber ImplNumber { get; }

        public DependencyKeyAttribute(ServiceImplementationNumber number)
        {
            this.ImplNumber = number;
        }
    }
}
