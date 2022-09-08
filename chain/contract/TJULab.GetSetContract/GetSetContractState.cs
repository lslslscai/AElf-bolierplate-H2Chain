using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace TJULab.GetSetContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public class GetSetContractState : ContractState
    {

        /// <summary>
        /// show the relationship between contracts and their author
        /// </summary>
        public MappedState<Address, ContractList> AuthorContractBase { get; set; }
        
        /// <summary>
        /// Set of contracts' state archived by contract's name and its author
        /// </summary>
        public MappedState<Author_Contract_Pair, StateList> ContractStateBase { get; set; }
        
        //Set of 
        public MappedState<StringValue, StringValue> ContractInfoMap { get; set; }
        
        public MappedState<StringValue, BinarySet> ContractInstances { get; set; }
    }
}