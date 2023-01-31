using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;

namespace TJULab.RSUServerContract
{
    public class RSUServerContractTestBase : DAppContractTestBase<RSUServerContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal RSUServerContractContainer.RSUServerContractStub GetRSUServerContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<RSUServerContractContainer.RSUServerContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}