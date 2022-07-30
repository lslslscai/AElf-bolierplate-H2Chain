using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace TJULab.GetSetContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public class GetSetContractState : ContractState
    {
        // state definitions go here.
        public MappedState<Address, NameList> AddressNameListMap { get; set; }
        public MappedState<AddressName, JsonStringList> AddressNameJsonStringListMap { get; set; }
        public MappedState<Address,bool> UserMap { get; set; }
        public MappedState<SetContractKey,SetContractValue> ContractInfoMap { get; set; }
    }
}