using Ipfs;
using Ipfs.Engine;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IPFS.Sample
{
    class Program
    {
        static string id;
        static void Main(string[] args)
        {
            id = "node";// + new Random().Next(1, 1000);
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        static async Task MainAsync(string[] args)
        {
            var id = "node" + new Random().Next(1, 1000);
            //var engine = new IpfsEngine("babakmahmodui".ToCharArray());
            var engine = new IpfsEngine(id.ToCharArray());
            engine.Options.Repository.Folder = Path.Combine(Path.GetTempPath(), id);
            engine.Options.KeyChain.DefaultKeySize = 512;
            engine.Config.SetAsync(
                "Addresses.Swarm",
                JToken.FromObject(new string[] { "/ip4/0.0.0.0/tcp/0" })
            ).Wait();
            await engine.StartAsync();
            Console.WriteLine($"Starting node:{id} Id: {(await engine.LocalPeer).Id}");
            engine.Options.Discovery.BootstrapPeers = new MultiAddress[] { };
            (await engine.SwarmService).PeerDiscovered += (s, p) =>
            {
                Console.WriteLine($"Peer Discovered: {p}");
            };
            await engine.PubSub.SubscribeAsync("chat.message", m =>
            {
                Console.WriteLine($"{m.Sender}: {Encoding.UTF8.GetString(m.DataBytes)}");
            }, default);

            while (true)
            {
                await engine.PubSub.PublishAsync("chat.message", $"Hi there from {id}");
                await Task.Delay(2000);
            }
        }
    }
}
