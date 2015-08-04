using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
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
            //// 모든 어셈블리 AssemblySource에 추가
            //DirectoryCatalog directoryCatalog = new DirectoryCatalog(@"./");
            //AssemblySource.Instance.AddRange(
            //    directoryCatalog.Parts
            //        .Select(part => ReflectionModelServices.GetPartType(part).Value.Assembly)
            //        .Where(assembly => !AssemblySource.Instance.Contains(assembly)));

            //// 이 어셈블리를 우선
            //List<Assembly> priorityAssemblies = SelectAssemblies().ToList();
            //AggregateCatalog priorityCatalog = new AggregateCatalog(priorityAssemblies.Select(x => new AssemblyCatalog(x)));
            //CatalogExportProvider priorityProvider = new CatalogExportProvider(priorityCatalog);

            //// 나머지 어셈블리들 수집
            //AggregateCatalog mainCatalog = new AggregateCatalog(
            //    AssemblySource.Instance
            //        .Where(assembly => !priorityAssemblies.Contains(assembly))
            //        .Select(x => new AssemblyCatalog(x)));
            //CatalogExportProvider mainProvider = new CatalogExportProvider(mainCatalog);

            //Container = new CompositionContainer(priorityProvider, mainProvider);
            //priorityProvider.SourceProvider = Container;
            //mainProvider.SourceProvider = Container;

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
