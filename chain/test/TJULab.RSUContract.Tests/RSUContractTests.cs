using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace TJULab.RSUContract
{
    public class StubSet : RSUContractTestBase
    {
        private readonly ECKeyPair server;
        private readonly RSUContractContainer.RSUContractStub serverStub;
        
        private readonly ECKeyPair nodeA;
        private readonly RSUContractContainer.RSUContractStub nodeAStub;
        
        private readonly ECKeyPair nodeB;
        private readonly RSUContractContainer.RSUContractStub nodeBStub;
        
        private readonly ECKeyPair car;
        private readonly RSUContractContainer.RSUContractStub carStub;

        public StubSet()
        {
            server = SampleAccount.Accounts[1].KeyPair;
            serverStub = GetRSUContractStub(server);
            
            nodeA = SampleAccount.Accounts[2].KeyPair;
            nodeAStub = GetRSUContractStub(nodeA);
            
            nodeB = SampleAccount.Accounts[3].KeyPair;
            nodeBStub = GetRSUContractStub(nodeB);
            
            car = SampleAccount.Accounts[4].KeyPair;
            carStub = GetRSUContractStub(car);

        }

        public ECKeyPair getKeyPair(string key)
        {
            var ret = key switch
            {
                "car" => car,
                "server" => server,
                "nodeA" => nodeA,
                "nodeB" => nodeB,
                _ => server
            };

            return ret;
        }

        internal RSUContractContainer.RSUContractStub getStub(string key)
        {
            var ret = key switch
            {
                "car" => carStub,
                "server" => serverStub,
                "nodeA" => nodeAStub,
                "nodeB" => nodeBStub,
                _ => serverStub
            };
            return ret;
        }

        internal BasicInfo getBasicInfo(string key)
        {
            var ret = new BasicInfo();
            switch (key)
            {
                case "nodeA":
                    ret.Addr = Address.FromPublicKey(nodeA.PublicKey);
                    ret.RegTime = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime());
                    ret.ServerSign = server.ToString();
                    break;
                case "nodeB":
                    ret.Addr = Address.FromPublicKey(nodeB.PublicKey);
                    ret.RegTime = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime());
                    ret.ServerSign = server.ToString();
                    break;
            }

            return ret;
        }

        internal Address getAddress(string key)
        {
            var ret = key switch
            {
                "car" => Address.FromPublicKey(car.PublicKey),
                "server" => Address.FromPublicKey(server.PublicKey),
                "nodeA" => Address.FromPublicKey(nodeA.PublicKey),
                "nodeB" => Address.FromPublicKey(nodeB.PublicKey),
                _ => Address.FromPublicKey(server.PublicKey)
            };

            return ret;
        }
    }
    
    public class RSUContractTest : RSUContractTestBase
    {
        public async Task Initialize(StubSet stubSet)
        {
            await stubSet.getStub("server").SystemInitialize.SendAsync(new Empty());
            await stubSet.getStub("nodeA").Initialize.SendAsync(new InitializeInput
            {
                Info = stubSet.getBasicInfo("nodeA"),
                AdjInfo = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeB") }
                }
            });
            await stubSet.getStub("nodeB").Initialize.SendAsync(new InitializeInput
            {
                Info = stubSet.getBasicInfo("nodeB"),
                AdjInfo = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeA") }
                }
            });
        }

        public async Task StartRound(StubSet stubSet)
        {
            await Initialize(stubSet);

            await stubSet.getStub("server").NextRound.SendAsync(new RoundInfoInput
            {
                NodeList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeA") }
                },
                CloudList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeA") }
                },
                PositiveList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeB") }
                }
            });
        }

        public async Task RoundCheck(StubSet stubSet)
        {
            await StartRound(stubSet);
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            await stubSet.getStub("nodeA").DeclareNodeCheck.SendAsync(new StringValue{Value = "asdf"});
            await stubSet.getStub("nodeB").ReciteNode.SendAsync(new ReciteInput
            {
                Result = true,
                Round = RoundResult.Result.Round,
                To = stubSet.getAddress("nodeA")
            });
            
            await stubSet.getStub("server").UploadPositiveCheckResult.SendAsync(new PosCheckResult
            {
                DataHash = "asdf",
                Round = RoundResult.Result.Round,
                To = stubSet.getAddress("nodeB"),
                Results = {{stubSet.getAddress("car").ToBase58(), true}}
            });
            
            await stubSet.getStub("server").UploadCloudCheckResult.SendAsync(new CloudCheckInput
            {
                DataHash = "qwer",
                Result = true,
                Round = RoundResult.Result.Round,
                ServerSign = stubSet.getKeyPair("server").ToString(),
                To = stubSet.getAddress("nodeA")
            });
            await stubSet.getStub("server").NextRound.SendAsync(new RoundInfoInput
            {
                NodeResult = { {stubSet.getAddress("nodeA").ToBase58(),1} },
                NodeList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeA") }
                },
                CloudList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeA") }
                },
                PositiveList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeB") }
                }
            });
        }
        [Fact]
        public async Task InitializeTest()
        {
            // Get a stub for testing.
            var stubSet = new StubSet();

            await Initialize(stubSet);
            
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            RoundResult.Result.Round.ShouldBe(0);
            RoundResult.Result.Nodes.Count.ShouldBe(2);

            var AdjInfo = stubSet.getStub("server").GetAdjList
                .CallAsync(Address.FromPublicKey(stubSet.getKeyPair("nodeA").PublicKey));
            AdjInfo.Result.Nodes.ShouldContain(Address.FromPublicKey(stubSet.getKeyPair("nodeB").PublicKey));
            AdjInfo.Result.Nodes.Count.ShouldBe(1);
            
            AdjInfo = stubSet.getStub("server").GetAdjList
                .CallAsync(Address.FromPublicKey(stubSet.getKeyPair("nodeB").PublicKey));
            AdjInfo.Result.Nodes.ShouldContain(Address.FromPublicKey(stubSet.getKeyPair("nodeA").PublicKey));
            AdjInfo.Result.Nodes.Count.ShouldBe(1);
        }
        
        [Fact]
        public async Task RoundTest()
        {
            // Get a stub for testing.
            var stubSet = new StubSet();
            await Initialize(stubSet);

            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            RoundResult.Result.Round.ShouldBe(0);
            await stubSet.getStub("server").NextRound.SendAsync(new RoundInfoInput
            {
                NodeList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeA") }
                },
                CloudList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeA") }
                },
                PositiveList = new NodeList
                {
                    Nodes = { stubSet.getAddress("nodeB") }
                }
            });
            
            RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            var CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            RoundResult.Result.Round.ShouldBe(1);
            CheckList.Result.CloudCheckList.Count.ShouldBe(1);
            CheckList.Result.CloudCheckList.Keys.ShouldContain(stubSet.getAddress("nodeA").ToBase58());
            CheckList.Result.NodeCheckList.Count.ShouldBe(1);
            CheckList.Result.NodeCheckList.Keys.ShouldContain(stubSet.getAddress("nodeA").ToBase58());
            CheckList.Result.CarPosCheckList.Count.ShouldBe(1);
            CheckList.Result.CarPosCheckList.Keys.ShouldContain(stubSet.getAddress("nodeB").ToBase58());
        }
        
        [Fact]
        public async Task NodeReciteRejectTest()
        {
            // Get a stub for testing.
            var stubSet = new StubSet();
            await StartRound(stubSet);

            await stubSet.getStub("nodeA").DeclareNodeCheck.SendAsync(new StringValue{Value = "asdf"});
            
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            var CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            CheckList.Result.NodeCheckList[stubSet.getAddress("nodeA").ToBase58()]
                .ReciteList.Keys.ShouldContain(stubSet.getAddress("nodeB").ToBase58());

            await stubSet.getStub("nodeB").ReciteNode.SendAsync(new ReciteInput
            {
                Result = false,
                Round = RoundResult.Result.Round,
                To = stubSet.getAddress("nodeA")
            });
            CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            CheckList.Result.NodeCheckList[stubSet.getAddress("nodeA").ToBase58()]
                .ReciteList[stubSet.getAddress("nodeB").ToBase58()].ShouldBe(-1);
        }
        
        [Fact]
        public async Task NodeRecitePassTest()
        {
            // Get a stub for testing.
            var stubSet = new StubSet();
            await StartRound(stubSet);

            await stubSet.getStub("nodeA").DeclareNodeCheck.SendAsync(new StringValue{Value = "asdf"});
            
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            var CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            CheckList.Result.NodeCheckList[stubSet.getAddress("nodeA").ToBase58()]
                .ReciteList.Keys.ShouldContain(stubSet.getAddress("nodeB").ToBase58());
            CheckList.Result.NodeCheckList[stubSet.getAddress("nodeA").ToBase58()].DataHash.ShouldBe("asdf");

            await stubSet.getStub("nodeB").ReciteNode.SendAsync(new ReciteInput
            {
                Result = true,
                Round = RoundResult.Result.Round,
                To = stubSet.getAddress("nodeA")
            });
            CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            CheckList.Result.NodeCheckList[stubSet.getAddress("nodeA").ToBase58()]
                .ReciteList[stubSet.getAddress("nodeB").ToBase58()].ShouldBe(1);
        }
        
        [Fact]
        public async Task CarPosRejectTest()
        {
            // Get a stub for testing.
            var stubSet = new StubSet();
            await StartRound(stubSet);
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            await stubSet.getStub("server").UploadPositiveCheckResult.SendAsync(new PosCheckResult
            {
                DataHash = "asdf",
                Round = RoundResult.Result.Round,
                To = stubSet.getAddress("nodeB"),
                Results = {{stubSet.getAddress("car").ToBase58(), false}}
            });
            
            var CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            CheckList.Result.CarPosCheckList[stubSet.getAddress("nodeB").ToBase58()].
                CheckResult[stubSet.getAddress("car").ToBase58()].ShouldBe(-1);
        }
        
        [Fact]
        public async Task CarPosPassTest()
        {
            var stubSet = new StubSet();
            await StartRound(stubSet);
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            await stubSet.getStub("server").UploadPositiveCheckResult.SendAsync(new PosCheckResult
            {
                DataHash = "asdf",
                Round = RoundResult.Result.Round,
                To = stubSet.getAddress("nodeB"),
                Results = {{stubSet.getAddress("car").ToBase58(), true}}
            });
            
            var CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            CheckList.Result.CarPosCheckList[stubSet.getAddress("nodeB").ToBase58()].
                CheckResult[stubSet.getAddress("car").ToBase58()].ShouldBe(1);
        }
        
        [Fact]
        public async Task CloudPassTest()
        {
            var stubSet = new StubSet();
            await StartRound(stubSet);
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            var CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            CheckList.Result.CloudCheckList.Count.ShouldBe(1);
            CheckList.Result.CloudCheckList.Keys.ShouldContain(stubSet.getAddress("nodeA").ToBase58());
            await stubSet.getStub("server").UploadCloudCheckResult.SendAsync(new CloudCheckInput
            {
                DataHash = "adsf",
                Result = true,
                Round = RoundResult.Result.Round,
                ServerSign = stubSet.getKeyPair("server").ToString(),
                To = stubSet.getAddress("nodeA")
                
            });
            
            CheckList = stubSet.getStub("server").GetCheckResult
                .CallAsync(new Int64Value {Value = RoundResult.Result.Round});
            CheckList.Result.CloudCheckList[stubSet.getAddress("nodeA").ToBase58()].Result.ShouldBe(true);
        }
        
        [Fact]
        public async Task UploadLongTermTest()
        {
            var stubSet = new StubSet();
            await RoundCheck(stubSet);
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());

            await stubSet.getStub("nodeA").UpdateLongTermCache.SendAsync(new LongTermCacheInput
            {
                DataHash = "asdf"
            });

            var uploadResult = stubSet.getStub("nodeA").GetDataHash.CallAsync(new Int64Value
            {
                Value = RoundResult.Result.Round-1
            });
            uploadResult.Result.Value.ShouldBe("asdf");
        }

        [Fact]
        public async Task EmptyRoundTest()
        {
            var stubSet = new StubSet();
            await Initialize(stubSet);
            
            var RoundResult = stubSet.getStub("server").GetStatus.CallAsync(new Empty());
            RoundResult.Result.Round.ShouldBe(0);
            await stubSet.getStub("server").NextRound.SendAsync(new RoundInfoInput());
        }
    }
}