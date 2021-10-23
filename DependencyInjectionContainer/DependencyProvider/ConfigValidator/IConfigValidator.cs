using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;

namespace DependencyInjectionContainer.DependencyProvider.ConfigValidator
{
    public interface IConfigValidator
    {
        bool Validate();
    }
}
