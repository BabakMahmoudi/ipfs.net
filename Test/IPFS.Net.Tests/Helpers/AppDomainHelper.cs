using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IPFS.Net.Tests.Helpers
{
    class AppDomainHelper
    {
        public static List<AppDomainHelper> domains = new List<AppDomainHelper>();
        public string Name { get; set; }
        private AppDomain appDomain;
        private DomainMainClass main;
        private Dictionary<Type, object> proxies = new Dictionary<Type, object>();

        private AppDomainHelper(string name)
        {
            this.Name = name;
        }

        public T GetProxy<T>(bool refresh = false) where T : MarshalByRefObject
        {
            object result = null;

            if (!proxies.TryGetValue(typeof(T), out result) || refresh || result == null)
            {
                result = GetAppDomain().CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location,
                     typeof(T).FullName);
                proxies.Add(typeof(T), result);
            }
            return (T)result;
        }
        public DomainMainClass GetMain()
        {
            if (this.appDomain == null || this.main == null)
            {
                this.appDomain = GetAppDomain();
                this.main = (DomainMainClass)
                     this.appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location,
                     typeof(DomainMainClass).FullName);
            }
            return this.main;
        }

        public bool IsAppDomainUnloaded(AppDomain domain)
        {
            try
            {
                return !(domain != null && domain.Id.ToString() != "");

            }
            catch
            {
                return true;
            }
        }

        public AppDomain GetAppDomain()
        {
            if (this.appDomain == null)
            {
                this.appDomain = AppDomain.CreateDomain(this.Name);
            }
            return this.appDomain;

        }


		public int Execute(params string[] args)
        {
            return this.GetMain().Execute(args);
        }

        public void UnLoad()
        {
            if (this.appDomain != null)
            {
                AppDomain.Unload(this.appDomain);
            }
            this.appDomain = null;
        }

        public static AppDomainHelper Create(string name)
        {
            var result = new AppDomainHelper(name);
            domains.Add(result);
            return result;

        }
        public static void KillAll()
        {
            domains.ForEach(x =>
            {
                if (x.appDomain != null)
                {
                    try { AppDomain.Unload(x.appDomain); } catch { }
                }
            });
            domains = new List<AppDomainHelper>();
        }


    }
    [Serializable]
    public class DomainMainClass : MarshalByRefObject
    {

        public DomainMainClass()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        }
        public int Execute(params string[] args)
        {
            var resut = 0;

            return resut;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
