using System;
using System.IO;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Newtonsoft.Json;

namespace TJULab.GetSetContract
{
    /// <summary>
    /// The C# implementation of the contract defined in get_set_contract.proto that is located in the "protobuf"
    /// folder.
    /// Notice that it inherits from the protobuf generated code. 
    /// </summary>
    public class GetSetContract : GetSetContractContainer.GetSetContractBase
    {
        /// <summary>
        /// The implementation of the Hello method. It takes no parameters and returns on of the custom data types
        /// defined in the protobuf definition file.
        /// </summary>
        /// <param name="input">Empty message (from Protobuf)</param>
        /// <returns>a HelloReturn</returns>
        /*public override Empty Initialize(Empty input)
        {
            State.UserMap[Context.Sender] = true;
            return new Empty();
        }*/
        public override GetReturn Get(GetInput input)
        {
            //Assert(State.UserMap[Context.Sender], "No Permissions");
            AddressName addressName = new AddressName();
            addressName.Address = Context.Sender;
            addressName.ClassName = input.ClassName;
            if (State.AddressNameListMap[Context.Sender] == null)
            {
                State.AddressNameListMap[Context.Sender] = new NameList();
            }
            State.AddressNameListMap[Context.Sender].NameList_.Add(input);
            if (State.AddressNameJsonStringListMap[addressName] == null)
            {
                State.AddressNameJsonStringListMap[addressName] = new JsonStringList();
                GetReturn getReturnNull = new GetReturn();
                getReturnNull.JsonString = "";
                return getReturnNull;
            }

            if (State.AddressNameJsonStringListMap[addressName].JsonStringList_.Count == 0)
            {
                GetReturn getReturnNull = new GetReturn();
                getReturnNull.JsonString = "";
                return getReturnNull;
            }
            GetReturn getReturn = State.AddressNameJsonStringListMap[addressName].JsonStringList_.Last();
            return getReturn;
        }

        public override Empty Set(SetInput input)
        {
            //Assert(State.UserMap[Context.Sender], "No Permissions");
            AddressName addressName = new AddressName();
            addressName.Address = Context.Sender;
            addressName.ClassName = input.ClassName;
            GetReturn getReturn = new GetReturn();
            getReturn.JsonString = input.JsonString;
            State.AddressNameJsonStringListMap[addressName].JsonStringList_.Add(getReturn);
            return new Empty();
        }
        
        public override Empty SetContract(SetContractInput input)
        {
            //(State.UserMap[Context.Sender], "No Permissions");
            
            SetContractKey setContractKey = new SetContractKey();
            setContractKey.ParamsHash = input.ParamsHash;
            
            SetContractValue setContractValue = new SetContractValue();
            setContractValue.ParamsJson = input.ParamsJson;

            State.ContractInfoMap[setContractKey] = setContractValue;
            return new Empty();
        }

        public override GetContractOutput GetContract(GetContractInput input)
        {
            //Assert(State.UserMap[Context.Sender], "No Permissions");
            
            SetContractKey setContractKey = new SetContractKey();
            setContractKey.ParamsHash = input.ParamsHash;
            Assert(State.ContractInfoMap[setContractKey]!=null, "No Matched");
            SetContractValue setContractValue = State.ContractInfoMap[setContractKey];
            GetContractOutput getContractOutput = new GetContractOutput();
            getContractOutput.ParamsJson = setContractValue.ParamsJson;
            return getContractOutput;
        }
    }
}