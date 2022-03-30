using AElf.Sdk.CSharp.State;

namespace AElf.Contracts.CreditTransferContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public class CreditTransferContractState : ContractState
    {
        // state definitions go here.
        public MappedState<UInt32State, SRT> SRT_Base { get; set; }//学号--SRT对应关系
        public MappedState<UInt32State, School> School_Base { get; set; }//学校编号--学校对应关系
        public MappedState<UInt32State, CourseInfo> Course_base { get; set; }//课程编号--课程对应关系
    }
}