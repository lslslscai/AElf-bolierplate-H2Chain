using System;
using System.IO;
using System.Linq;
using AElf.Sdk.CSharp;
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
        #region Action
        
        public override Empty TestEnvInitialize(StringValue input)
        {
            ContractStateInitialize(input.Value, Context.Sender);
            return new Empty();
        }
        
        public override Empty Set(SetInput input)
        {

            var addressName = new Author_Contract_Pair
            {
                Address = Context.Sender,
                ContractName = input.ContractName
            };
            
            State.ContractStateBase[addressName].State.Add(input.JsonString);
            
            return new Empty();
        }
        
        public override Empty SetContract(SetContractInput input)
        {
            
            State.ContractInfoMap[new StringValue {Value = input.CodeInfoHash}] = new StringValue
            {
                Value = input.CodeInfo
            };
            ContractStateInitialize(input.ContractName, input.Author);
            
            return new Empty();
        }

        public override Empty BinaryUpload(BinaryUploadInput input)
        {
            State.ContractInstances[new StringValue {Value = input.Name}] = new BinarySet();//不保留上一次上传的代码
            
            State.ContractInstances[new StringValue {Value = input.Name}].Fragments.Add(input);

            return new Empty();
        }
        
        #endregion

        #region View

        public override GetReturn Get(GetInput input)
        {
            var addressName = new Author_Contract_Pair
            {
                Address = Context.Sender,
                ContractName = input.ContractName
            };

            Assert(State.AuthorContractBase[Context.Sender] != null, "error1"+ Context.Sender);
            Assert(State.ContractStateBase[addressName] != null, "error2"+addressName);
            // if target Contract's State not exists
            // create a new Space for the Contract, then return a empty State
            var context = new TransactionContext();
            if (input.ContextFlag)
            {
                context.Self = Context.Self;
                context.Sender = Context.Sender;
                context.ChainID = Context.ChainId;
                context.CurrentBlockHeight = Context.CurrentHeight;
                context.CurrentBlockTime = Context.CurrentBlockTime;
                context.PreviousBlockHash = Context.PreviousBlockHash;
                context.SelfString = Context.Self.ToString();
                context.SenderString = Context.Sender.ToString();
                context.PreviousBlockHashString = Context.PreviousBlockHash.ToString();
            }

            // if target Contract's State exists, but has no record
            // return a empty State
            if (State.ContractStateBase[addressName].State.Count == 0)
            {
                return new GetReturn
                {
                    State = "",
                    Context = context
                };
            }
            
            // fetch the latest version of contract's state, then return it 
            var getReturn = State.ContractStateBase[addressName].State.Last();

            return new GetReturn
            {
                State = getReturn,
                Context = context
            };
        }

        public override StringValue GetContract(StringValue input)
        {
            Assert(State.ContractInfoMap[input]!=null, "No Matched");
            
            return State.ContractInfoMap[input];
        }

        public override BinarySet GetContractInstance(StringValue input)
        {
            return State.ContractInstances[input];
        }
        
        #endregion

        #region Helper

        private void ContractStateInitialize(string contractName, Address author)
        {
            var addressName = new Author_Contract_Pair
            {
                Address = author,
                ContractName = contractName
            };
            
            // if Context.Sender use MultiLang_Contract for the first time (no records in AuthorContractBase)
            // Create a new Space for Context.Sender 
            if (State.AuthorContractBase[Context.Sender] == null)
            {
                State.AuthorContractBase[Context.Sender] = new ContractList();
            }
            // add an access record for Context.Sender, 
            State.AuthorContractBase[Context.Sender].Contract.Add(contractName);
            
            // if target Contract's State not exists
            // create a new Space for the Contract, then return a empty State
            if (State.ContractStateBase[addressName] == null)
            {
                State.ContractStateBase[addressName] = new StateList();
            }
        }

        #endregion
        
    }
}