using AElf.Sdk.CSharp.State;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CreditTransferContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public class CreditTransferContractState : ContractState
    {
        // user data.
        public MappedState<StringValue, SRT> SRT_Base { get; set; }//学号--SRT对应关系，快速查找SRT
        public MappedState<StringValue, School> School_Base { get; set; }//学校编号--学校对应关系，快速查找学校
        public MappedState<StringValue, CourseInfo> CourseInfo_Base { get; set; }//课程编号--课程对应关系，快速查找课程
        public MappedState<StringValue, CourseRecord> CourseRecoed_Base { get; set; }//课程编号+学生编号--选课记录对应关系，快速查找选课记录

        // system data
        public BoolState Initialized { get; set; }//系统是否初始化

    }
}