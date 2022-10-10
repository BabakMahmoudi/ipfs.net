using Ipfs;
using Ipfs.Engine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IPFS.Net.Tests.Helpers
{
    public class ClientStartOptions : MarshalByRefObject
    {
        public string Passphrase { get; set; }
        public bool SkipMessagingServices { get; set; }
        public string EndpointName { get; set; }
        public string SignalRServerUrl { get; set; }
        public string RedisServer { get; set; }
        public bool SkipRedisTransport { get; set; }
        public bool AddNatsTransport { get; set; }

        public ClientStartOptions()
        {
            this.EndpointName = $"ENdpoint_{Guid.NewGuid().ToString()}";
            Passphrase = this.EndpointName;
        }


    }
    public class DomainAgent : MarshalByRefObject
    {

        private string Endpoint;
        private ClientStartOptions options;
        public List<string> Recived_Packets = new List<string>();
        public string Address;
        static DomainAgent()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        private Ipfs.Engine.IpfsEngine engine;



        public void Start(ClientStartOptions options)
        {
            this.options = options;
            this.engine = new IpfsEngine(this.options.Passphrase.ToCharArray());
            this.engine.Options.Repository.Folder = Path.Combine(Path.GetTempPath(), this.options.EndpointName);
            this.engine.Options.KeyChain.DefaultKeySize = 512;
            this.engine.Config.SetAsync(
                "Addresses.Swarm",
                JToken.FromObject(new string[] { "/ip4/0.0.0.0/tcp/0" })
            ).Wait();
            var p = this.engine.LocalPeer.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            this.engine.Start();
            this.Address = p.Addresses.FirstOrDefault().ToString();
            var tt = this.engine.SwarmService.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            tt.PeerDiscovered += (s, p1) =>
            {
                if (this.options.Passphrase == "proxy2")
                {
                    var fff = p1;
                }
            };


        }
        public void Stop()
        {
            //this.host.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            //this.host.Dispose();
            //this.host = null;
        }
        public void Publis(string topic, string message)
        {
            var engine = this.engine;
            engine.PubSub.PublishAsync(topic, message).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public void Subscribe(string topic)
        {
            var p = engine.Swarm.PeersAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            this.engine.PubSub.SubscribeAsync(topic, m =>
            {

                this.Recived_Packets.Add(System.Text.Encoding.UTF8.GetString(m.DataBytes));

            }, default).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName name = new AssemblyName(args.Name);
            return Assembly.Load(name.Name);
        }

    }
}
