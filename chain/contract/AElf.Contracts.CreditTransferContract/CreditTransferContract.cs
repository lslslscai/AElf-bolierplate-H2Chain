using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CreditTransferContract
{
    /// <summary>
    /// The C# implementation of the contract defined in credit_transfer_contract.proto that is located in the "protobuf"
    /// folder.
    /// Notice that it inherits from the protobuf generated code. 
    /// </summary>
    public class CreditTransferContract : CreditTransferContractContainer.CreditTransferContractBase
    {
        /// <summary>
        /// The implementation of the Hello method. It takes no parameters and returns on of the custom data types
        /// defined in the protobuf definition file.
        /// </summary>
        /// <param name="input">Empty message (from Protobuf)</param>
        /// <returns>a HelloReturn</returns>
        public override Empty SRT_Create(UInt64Value input)
        {
            return new Empty();
        }
        
        public override Empty SRT_Adjust(SRT input)
        {
            return new Empty();
        }
        
        public override Empty Course_Create(CourseInfo input)
        {
            return new Empty();
        }
        
        public override Empty Course_Adjust(CourseInfo input)
        {
            return new Empty();
        }

        private void Return_Solve(int input)
        {
            
        }

        private int SRT_Validate(SRT input)
        {
            return 0;
        }
        
        private int Course_Validate(CourseInfo input)
        {
            return 0;
        }
    }
}