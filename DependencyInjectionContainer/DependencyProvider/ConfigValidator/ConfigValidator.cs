using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;

namespace DependencyInjectionContainer.DependencyProvider.ConfigValidator
{
    public class ConfigValidator : IConfigValidator
    {
        public ConfigValidator(IDependenciesConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IDependenciesConfiguration Configuration { get; }
        public bool Validate()
        {
            throw new NotImplementedException();
        }
    }
}
