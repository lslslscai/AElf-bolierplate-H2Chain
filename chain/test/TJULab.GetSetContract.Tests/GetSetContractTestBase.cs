using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;

namespace TJULab.GetSetContract
{
    public class GetSetContractTestBase : DAppContractTestBase<GetSetContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal GetSetContractContainer.GetSetContractStub GetGetSetContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<GetSetContractContainer.GetSetContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}