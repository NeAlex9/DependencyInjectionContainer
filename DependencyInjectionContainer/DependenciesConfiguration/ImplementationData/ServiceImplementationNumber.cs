using System;

namespace DependencyInjectionContainer.DependenciesConfiguration.ImplementationData
{
    [Flags]
    public enum ServiceImplementationNumber
    {
        None = 1,
        First = 2,
        Second = 4,
        Any = None | First | Second,
    }
}
