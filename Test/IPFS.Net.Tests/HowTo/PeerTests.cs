using Ipfs;
using Ipfs.Engine;
using IPFS.Net.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPFS.Net.Tests.HowTo
{
    [TestClass]
    public class PeerTests
    {
        [TestMethod]
        public async Task HowTo_Get_PeerId()
        {
             var ipfs = TestFixture.Ipfs;
            var pass = "babak";
            //var engine = new IpfsEngine("proxy1".ToCharArray());
            //await engine.StartAsync();
            await ipfs.StartAsync();
            //var cfg = ipfs.Config.SetAsync()
            var p = await ipfs.LocalPeer;
            var aaa = p.Addresses.ToList();
            var client1 = AppDomainHelper.Create("proxy1").GetProxy<DomainAgent>();
            client1.Start(new ClientStartOptions { Passphrase = "proxy1", EndpointName = "proxy1" });
            var client2 = AppDomainHelper.Create("proxy1").GetProxy<DomainAgent>();
            client2.Start(new ClientStartOptions { Passphrase = "proxy2", EndpointName = "proxy2" });
            await Task.Delay(1000);
            var peers = await ipfs.Swarm.PeersAsync();
            

            var serv = await ipfs.SwarmService;
            //await serv.StartAsync();
            var m = new MultiAddress(client1.Address);
            await serv.ConnectAsync(m);
            await serv.ConnectAsync(new MultiAddress(client2.Address));
            
            

            await ipfs.PubSub.SubscribeAsync("test.topic", m1 =>
            {

                var b = m1.DataBytes;
                var st = System.Text.Encoding.UTF8.GetString(b);


            }, default);

            client2.Subscribe("test.topic1");

            client1.Publis("test.topic", "hi");
            client1.Publis("test.topic1", "hi");
            await Task.Delay(1000);
            var f = client2.Recived_Packets;
            await ipfs.StopAsync();
            await Task.Delay(1000);
            client1.Publis("test.topic1", "hi");
            await Task.Delay(1000);
            f = client2.Recived_Packets;

            await Task.Delay(10000);

        }
    }
}
