using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor
{
    public class AppBootstrapper : BootstrapperBase
    {
        protected CompositionContainer Container { get; set; }
        
        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            var composableCatalogs = AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>();
            Container = new CompositionContainer(new AggregateCatalog(composableCatalogs));

            var batch = new CompositionBatch();
            BindServices(batch);
            batch.AddExportedValue(Container);

            Container.Compose(batch);
        }

        protected virtual void BindServices(CompositionBatch batch)
        {
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(Container);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = String.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            IEnumerable<Lazy<object>> exports = Container.GetExports<object>(contract);

            if (exports.Any())
                return exports.First().Value;

            throw new Exception(String.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return Container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            Container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            DisplayRootViewFor<IMainWindow>();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetEntryAssembly() };
        }
    }
}
