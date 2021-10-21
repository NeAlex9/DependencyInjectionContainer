using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;

namespace DependencyInjectionContainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new DependenciesConfiguration.DependenciesConfiguration();
            config.Register(typeof(IInterface<>), typeof(Ex<>), ImplementationsTTL.InstancePerDependency);
            //config.Register<IMessageSender, Letter>(ImplementationsTTL.Singleton);
            //config.Register(typeof(IMessageSender), typeof(Email), ImplementationsTTL.InstancePerDependency);
            //config.Register<IMessageSender, Letter>(ImplementationsTTL.Singleton);
            config.Register<IMessageSender, Email>(ImplementationsTTL.Singleton);
            config.Register<IMessageSender, Letter>(ImplementationsTTL.InstancePerDependency);
            config.Register<IRep, Rep>(ImplementationsTTL.Singleton);
            config.Register<IInterface<IRep>, Ex<IRep>>(ImplementationsTTL.InstancePerDependency);
            var dependencyProvider = new DependencyProvider.DependencyProvider(config);
            //dependencyProvider.Resolve(typeof(IInterface<>));
            var t = dependencyProvider.Resolve<IInterface<IRep>>();
            //dependencyProvider.Resolve(typeof(IEnumerable<IMessageSender>));
            var rs = dependencyProvider.Resolve<IMessageSender>();
            var ds = dependencyProvider.Resolve<IMessageSender>();
            var lst = dependencyProvider.Resolve<IMessageSender>();
            var s = rs == ds;
            Console.ReadLine();
        }
    }
}


// thread safe 
// validation
// obsession