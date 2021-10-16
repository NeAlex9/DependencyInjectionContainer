using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using DependencyInjectionContainer.DependencyProvider.ConfigValidator;

namespace DependencyInjectionContainer.DependencyProvider
{
    public class DependencyProvider : IDependencyProvider, IValidator
    {
        public IDependenciesConfiguration Configuration { get; private set; }
        public Dictionary<Type, object> Singletons { get; private set; }

        public DependencyProvider(IDependenciesConfiguration configuration)
        {
            if (!Validate())
            {
                throw new ArgumentException("Wrong configuration");
            }

            this.Singletons = new Dictionary<Type, object>();
            this.Configuration = configuration;
        }

        public TDependency Resolve<TDependency>(ServiceImplementationNumber number = ServiceImplementationNumber.None)
            where TDependency : class
        {
            return (TDependency) Resolve(typeof(TDependency), number);
        }

        public object Resolve(Type dependencyType, ServiceImplementationNumber number = ServiceImplementationNumber.None)
        {
            object result;
            if (this.Singletons.ContainsKey(dependencyType))
            {
                result = this.Singletons[dependencyType];
            } 
            else if (IsIEnumerable(dependencyType))
            {
                var implementations = this.Configuration.DependenciesDictionary[dependencyType].Select(container => container.ImplementationsType);
                result = implementations.Select(CreateInstance);
            }
            else
            {
                var container = GetImplementationsType(dependencyType, number);
                result = CreateInstance(container.ImplementationsType);
                if (container.TimeToLive == ImplementationsTTL.Singleton)
                {
                    this.Singletons.Add(dependencyType, result);
                }
            }

            return result;
        }

        protected ImplementationsContainer GetImplementationsType(Type dependencyType, ServiceImplementationNumber number)
        {
            return this.Configuration.DependenciesDictionary[dependencyType].Find(container => container.ImplNumber == number);
        }

        public bool IsIEnumerable(Type dependencyType)
        {
            return dependencyType.GetInterfaces().Any(i => i.Name == "IEnumerable");
        }

        protected object CreateInstance(Type implType)
        {
            var ctor = implType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)?.FirstOrDefault();
            var constructorParams = ctor.GetParameters();
            var generatedParams = new List<dynamic>();
            foreach (var param in constructorParams)
            {
                
            }

            return null;
        }

        public bool Validate()
        {
            return true;
        }
    }
}
