using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zelda.Game.Primitives;

namespace Zelda.Game
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class IdAttribute : Attribute
    {
        public static readonly IdAttribute Default = new IdAttribute("");
        public string Id { get; }

        public IdAttribute(string id)
        {
            Id = id;
        }
    }

    public class ObjectCreator
    {
        public static Action<string> MissingTypeAction = s => { throw new InvalidOperationException("Cannot locate type: {0}".F(s)); };

        readonly Cache<string, Type> _typeCache;
        readonly Cache<Type, ConstructorInfo> _ctorCache;
        readonly Tuple<Assembly, string>[] _assemblies;

        public ObjectCreator(Mod mod)
        {
            _typeCache = new Cache<string,Type>(FindType);
            _ctorCache = new Cache<Type, ConstructorInfo>(GetCtor);

            var asms = new List<Tuple<Assembly, string>>();

            foreach (var asmFile in mod.Resources.Assemblies)
            {
                try
                {
                    var asm = Assembly.Load(mod.ModFiles.DataFileRead(asmFile));
                    asms.AddRange(asm.GetNamespaces().Select(ns => new Tuple<Assembly, string>(asm, ns)));
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to load assembly '{0}': {1}".F(asmFile, ex.Message), ex);
                }

                _assemblies = asms.ToArray();
            }
        }

        public Type GetTypeById<T>(string id)
        {
            var itemTypes = GetTypesImplementing<T>();
            if (!itemTypes.Any())
                return null;

            foreach (var itemType in itemTypes)
            {
                string idValue = itemType.GetCustomAttributes<IdAttribute>().DefaultIfEmpty(IdAttribute.Default).First().Id;
                if (idValue == id)
                    return itemType;
            }

            return null;
        }

        Type FindType(string className)
        {
            return _assemblies
                .Select(tuple => tuple.Item1.GetType(tuple.Item2 + "." + className, false))
                .FirstOrDefault(t => t != null);
        }

        ConstructorInfo GetCtor(Type type)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var ctors = type.GetConstructors(flags).Where(x => x.HasAttribute<UseCtorAttribute>());
            if (ctors.Count() > 1)
                throw new InvalidOperationException("ObjectCreator: UseCtor on multiple constructors; invalid.");
            return ctors.FirstOrDefault();
        }

        public T CreateObject<T>(string className)
        {
            return CreateObject<T>(className, new Dictionary<string, object>());
        }

        public T CreateObject<T>(string className, Dictionary<string, object> args)
        {
            Type type = _typeCache[className];
            if (type == null)
            {
                MissingTypeAction(className);
                return default(T);
            }

            ConstructorInfo ctor = _ctorCache[type];
            if (ctor == null)
                return (T)CreateBasic(type);
            else
                return (T)CreateUsingArgs(ctor, args);
        }

        public object CreateBasic(Type type)
        {
            return type.GetConstructor(new Type[0]).Invoke(new object[0]);
        }

        public object CreateUsingArgs(ConstructorInfo ctor, Dictionary<string, object> args)
        {
            ParameterInfo[] p = ctor.GetParameters();
            object[] a = new object[p.Length];
            for (int i = 0; i < p.Length; ++i)
            {
                string key = p[i].Name;
                if (!args.ContainsKey(key))
                    throw new InvalidOperationException("ObjectCreator: key '{0}' not found".F(key));
                a[i] = args[key];
            }

            return ctor.Invoke(a);
        }

        public IEnumerable<Type> GetTypesImplementing<T>()
        {
            Type type = typeof(T);
            return GetTypes().Where(t => t != type && type.IsAssignableFrom(t));
        }

        public IEnumerable<Type> GetTypes()
        {
            return _assemblies.Select(ma => ma.Item1).Distinct()
                .SelectMany(ma => ma.GetTypes());
        }

        [AttributeUsage(AttributeTargets.Constructor)]
        public sealed class UseCtorAttribute : Attribute { }
    }
}
