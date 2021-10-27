using System;
using DependencyInjectionContainer.DependencyProvider;

namespace DependencyInjectionContainer.DependenciesConfiguration.ImplementationData
{
    public class ImplementationsContainer
    {
        public Type ImplementationsType { get; }
        public ImplementationsTTL TimeToLive { get; }
        public ServiceImplementationNumber ImplNumber { get; }

        public ImplementationsContainer(Type implementationsType, ImplementationsTTL timeToLive,
            ServiceImplementationNumber implNumber)
        {
            this.ImplNumber = implNumber;
            this.ImplementationsType = implementationsType;
            this.TimeToLive = timeToLive;
        }
    }
}
