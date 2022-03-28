using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;

namespace AElf.Contracts.CreditTransferContract
{
    public class CreditTransferContractTestBase : DAppContractTestBase<CreditTransferContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal CreditTransferContractContainer.CreditTransferContractStub GetCreditTransferContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<CreditTransferContractContainer.CreditTransferContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}