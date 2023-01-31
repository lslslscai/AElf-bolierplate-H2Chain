using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace TJULab.RSUContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public class RSUContractState : ContractState
    {
        public SingletonState<Address> ServerAddr { get; set; }
        
        public BoolState Initialized { get; set; }//系统是否初始化
        public SingletonState<NodeList> NodeList { get; set; }
        
        public SingletonState<Int64Value> Round { get; set; }
        
        public MappedState<Int64Value, Timestamp> Timestamps { get; set; }
        
        // 各个节点的初始化情况
        public MappedState<Address, BoolValue> InitializeStates { get; set; }
        
        // 节点基本信息
        public MappedState<Address, BasicInfo> NodeInfo { get; set; }
        
        public MappedState<Address, NodeList> AdjList { get; set; }
        
        

        // 本轮的节点检查列表及其结果
        public MappedState<Int64Value, Address, NodeCheckRecord> NodeCheckList { get; set; }
        
        // 本轮的汽车主动检查列表及其结果
        public MappedState<Int64Value, Address, CarPosCheckRecord> PositiveCheckList { get; set; }

        // 本轮的云端检查列表及其结果
        public MappedState<Int64Value, Address, CloudCheckRecord> CloudCheckList { get; set; }

        
    }
}