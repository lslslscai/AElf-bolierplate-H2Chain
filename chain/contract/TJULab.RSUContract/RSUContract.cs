using System.Collections.Generic;
using AElf;
using Google.Protobuf.WellKnownTypes;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf.Collections;

namespace TJULab.RSUContract
{
    /// <summary>
    /// The C# implementation of the contract defined in rsu_contract.proto that is located in the "protobuf"
    /// folder.
    /// Notice that it inherits from the protobuf generated code. 
    /// </summary>
    public class RSUContract : RSUContractContainer.RSUContractBase
    {
        #region Action

        public override Empty SystemInitialize(Empty input)
        {
            if (State.Initialized.Value)
            {
                Assert(false,"already initialized");
            }
            // 初始化轮数
            State.Round.Value = new Int64Value{Value = 0};
            
            // 初始化管理者身份信息与系统存储
            State.ServerAddr.Value = Context.Sender;

            State.Initialized.Value = true;
            
            return new Empty();
        }
        
        /// <summary>
        /// 节点初始化，写入必要的常量
        /// </summary>
        /// <param name="input">Initialize message</param>
        public override Empty Initialize(InitializeInput input)
        {
            if (State.InitializeStates[Context.Sender] != null 
                &&State.InitializeStates[Context.Sender].Value)
            {
                return new Empty();
            }
            
            // 更新注册信息
            State.NodeInfo[Context.Sender] = new BasicInfo
            {
                Addr = Context.Sender,
                RegTime = input.Info.RegTime,
                ServerSign = input.Info.ServerSign
            };
            
            // 更新邻接表
            if (State.AdjList[Context.Sender] == null)
                State.AdjList[Context.Sender] = new NodeList();
            // 更新节点列表
            if (State.NodeList.Value == null)
            {
                State.NodeList.Value = new NodeList();
            }
            State.NodeList.Value.Nodes.Add(Context.Sender);
            if (input.AdjInfo.Nodes != null)
            {
                foreach (var addr in input.AdjInfo.Nodes)
                {
                    if (State.NodeList.Value.Nodes.Contains(addr))
                    {
                        State.AdjList[addr].Nodes.Add(Context.Sender);
                        State.AdjList[Context.Sender].Nodes.Add(addr);
                    }
                        
                }
            }

            // 更新初始化信息
            State.InitializeStates[Context.Sender] = new BoolValue{Value = true};
            

            return new Empty();
        }

        /// <summary>
        /// 下一轮初始化，更新所有检查列表
        /// </summary>
        /// <param name="input">next round's message</param>
        public override Empty NextRound(RoundInfoInput input)
        {
            foreach (var addrString in input.NodeResult.Keys)
            {
                
                var addr = Address.FromBase58(addrString);
                Assert(State.NodeCheckList[State.Round.Value][addr]!= null,"no check required in prior round");
                State.NodeCheckList[State.Round.Value][addr].Result = input.NodeResult[addrString];
            }
            // Assert(false,"breakpoint1.1" );
            // 回合数增加
            State.Round.Value.Value += 1;
            State.Timestamps[State.Round.Value] = Context.CurrentBlockTime;
            // 初始化下一回合列表
            if (input.CloudList != null)
            {
                // Assert(false,"breakpoint2.1");
                foreach (var addr in input.CloudList.Nodes)
                {
                    State.CloudCheckList[State.Round.Value][addr] = new CloudCheckRecord();
                    // Assert(false,"breakpoint2.2" + addr.ToBase58());
                }
                // Assert(false,"breakpoint2.3");

            }

            if (input.PositiveList != null)
            {
                foreach (var addr in input.PositiveList.Nodes)
                {
                    State.PositiveCheckList[State.Round.Value][addr] = new CarPosCheckRecord();
                }
                // Assert(false,"breakpoint3");

            }

            if (input.NodeList != null)
            {
                // Assert(input.NodeList.Nodes != null, "nodes is null!");
                foreach (var addr in input.NodeList.Nodes)
                {
                    State.NodeCheckList[State.Round.Value][addr] = new NodeCheckRecord();
                }
            }       
            
            // Assert(false,"breakpoint4");

            // 按照下一轮的信息更新检查列表
            return new Empty();
        }

        /// <summary>
        /// 发起对自己的节点检查
        /// </summary>
        public override Empty DeclareNodeCheck(StringValue input)
        {
            Assert(State.NodeCheckList[State.Round.Value][Context.Sender] != null, "no nodeCheck needed!");
            State.NodeCheckList[State.Round.Value][Context.Sender].DataHash = input.Value;
            foreach (var addr in State.AdjList[Context.Sender].Nodes)
            {
                State.NodeCheckList[State.Round.Value][Context.Sender].ReciteList[addr.ToBase58()] = 0;
            }
            
            return new Empty();
        }
        
        /// <summary>
        /// 发布对于特定节点的检查结果
        /// </summary>
        /// <param name="input">next round's message (from Protobuf)</param>
        public override Empty ReciteNode(ReciteInput input)
        {
            Assert(input.Round == State.Round.Value.Value, "not current round's check!");
            Assert (State.NodeCheckList[State.Round.Value][input.To] != null, "no nodeCheck needed!");
            Assert(State.AdjList[input.To].Nodes.Contains(Context.Sender), "not adjacent!");
            Assert(State.NodeCheckList[State.Round.Value][input.To].ReciteList[Context.Sender.ToBase58()] == 0,
                "already checked!");

            State.NodeCheckList[State.Round.Value][input.To].ReciteList[Context.Sender.ToBase58()] = input.Result
                ? 1
                : -1;
            return new Empty();
        }

        /// <summary>
        /// 下一轮初始化，更新所有检查列表
        /// </summary>
        /// <param name="input">next round's message (from Protobuf)</param>
        public override Empty UploadPositiveCheckResult(PosCheckResult input)
        {
            Assert(Context.Sender == State.ServerAddr.Value, "not allowed");
            Assert (State.PositiveCheckList[State.Round.Value][input.To] != null, "no positiveCheck needed!");
            Assert(input.Round == State.Round.Value.Value, "not current round's check!");
            
            foreach (var addrString in input.Results.Keys)
            {
                State.PositiveCheckList[State.Round.Value][input.To].CheckResult[addrString] = input.Results[addrString]
                    ? 1
                    : -1;
            }
            
            return new Empty();
        }
        /// <summary>
        /// 下一轮初始化，更新所有检查列表
        /// </summary>
        /// <param name="input">next round's message (from Protobuf)</param>
        public override Empty UploadCloudCheckResult(CloudCheckInput input)
        {
            Assert (State.CloudCheckList[State.Round.Value][input.To] != null, "no cloudCheck needed!");
            Assert(input.Round == State.Round.Value.Value, "not current round's check!");

            State.CloudCheckList[State.Round.Value][input.To].Result = input.Result;
            State.CloudCheckList[State.Round.Value][input.To].DataHash = input.DataHash;
            State.CloudCheckList[State.Round.Value][input.To].ServerSign = input.ServerSign;

            return new Empty();
        }
        
        /// <summary>
        /// 下一轮初始化，更新所有检查列表
        /// </summary>
        /// <param name="input">next round's message (from Protobuf)</param>
        public override Empty UpdateLongTermCache(LongTermCacheInput input)
        {
            var flag = false;
            for(var i = 0; i < 10 ; i+=1)
            {
                var round = new Int64Value {Value = State.Round.Value.Value - i};
                if (State.NodeCheckList[round][Context.Sender] != null)
                {
                    if (State.NodeCheckList[round][Context.Sender].Result > 0
                        && State.NodeCheckList[round][Context.Sender].DataHash == input.DataHash)
                    {

                        State.NodeCheckList[round][Context.Sender].IsUploaded = true;
                        flag = true;
                        break;
                    }
                }
            }
            
            Assert(flag, "Previous NodeCheck Expired!");
            
            return new Empty();
        }
        #endregion

        #region Viewer

        public override RoundResult GetCheckResult(Int64Value input)
        {

            var result = new RoundResult();

            foreach (var addr in State.NodeList.Value.Nodes)
            {
                if (State.NodeCheckList[input][addr] != null)
                    result.NodeCheckList[addr.ToBase58()] = State.NodeCheckList[input][addr];
                if (State.PositiveCheckList[input][addr] != null)
                    result.CarPosCheckList[addr.ToBase58()] = State.PositiveCheckList[input][addr];
                if (State.CloudCheckList[input][addr] != null)
                    result.CloudCheckList[addr.ToBase58()] = State.CloudCheckList[input][addr];
            }

            return result;
        }

        public override DataHashSet GetCloudHashByTime(GetDataHashInput input)
        {
            var flag = false;
            Assert(input.EndTime <= Context.CurrentBlockTime, "invalid end time!");
            Assert(input.StartTime >= State.Timestamps[new Int64Value{Value = 1}], "invalid start time!");
            
            var datahash = new DataHashSet();
            for (var i = 0;i <= 1000; i++)
            {
                var round = new Int64Value {Value = State.Round.Value.Value - i};
                if (State.Timestamps[round] < input.EndTime)
                    flag = true;
                
                if (flag && State.NodeCheckList[round][Context.Sender].IsUploaded)
                    datahash.HashSet.Add(State.NodeCheckList[round][Context.Sender].DataHash);

                if (State.Timestamps[round] < input.StartTime)
                    break;

            }
            return datahash;
        }

        public override StringValue GetDataHash(Int64Value input)
        {
            var ret = State.NodeCheckList[input][Context.Sender].Result > 0
                ? State.NodeCheckList[input][Context.Sender].DataHash
                : "invalid dataHash!";

            return new StringValue {Value = ret};
        }

        public override StatusResult GetStatus(Empty input)
        {
           
            var ret = new StatusResult
            {
                Round = State.Round.Value.Value,
            };

            foreach (var node in State.NodeList.Value.Nodes)
            {
                ret.Nodes.Add(node);
            }
            return ret;
        }

        public override NodeList GetAdjList(Address input)
        {
            return State.AdjList[input];
        }
        #endregion

        #region Helper
        #endregion
    }
}