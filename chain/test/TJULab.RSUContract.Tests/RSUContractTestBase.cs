using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;

namespace TJULab.RSUContract
{
    public class RSUContractTestBase : DAppContractTestBase<RSUContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal RSUContractContainer.RSUContractStub GetRSUContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<RSUContractContainer.RSUContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}