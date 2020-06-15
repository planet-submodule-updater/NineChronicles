using System.Collections.Generic;
using System.Collections.Immutable;
using Bencodex.Types;
using Libplanet;
using Libplanet.Crypto;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Xunit;

namespace Lib9c.Tests.Action
{
    public class CreateActivationKeyTest
    {
        [Fact]
        public void Execute()
        {
            var nonce = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var pubKey = new PublicKey(
                ByteUtil.ParseHex("02ed49dbe0f2c34d9dff8335d6dd9097f7a3ef17dfb5f048382eebc7f451a50aa1")
            );
            var key = new ActivationKeyState(nonce, pubKey);
            var action = new CreateActivationKey(key);
            var adminAddress = new Address("399bddF9F7B6d902ea27037B907B2486C9910730");
            var adminState = new AdminState(adminAddress, 100);
            var state = new State(ImmutableDictionary<Address, IValue>.Empty
                .Add(AdminState.Address, adminState.Serialize())
            );
            var actionContext = new ActionContext()
            {
                BlockIndex = 1,
                PreviousStates = state,
                Signer = adminAddress
            };

            var nextState = action.Execute(actionContext);
            Assert.Equal(key.Serialize(), nextState.GetState(key.address));
        }

        [Fact]
        public void CheckPermission()
        {
            var nonce = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var pubKey = new PublicKey(
                ByteUtil.ParseHex("02ed49dbe0f2c34d9dff8335d6dd9097f7a3ef17dfb5f048382eebc7f451a50aa1")
            );
            var key = new ActivationKeyState(nonce, pubKey);
            var action = new CreateActivationKey(key);
            var adminAddress = new Address("399bddF9F7B6d902ea27037B907B2486C9910730");
            var adminState = new AdminState(adminAddress, 100);
            var state = new State(ImmutableDictionary<Address, IValue>.Empty
                .Add(AdminState.Address, adminState.Serialize())
            );

            Assert.Throws<PolicyExpiredException>(
                () => action.Execute(new ActionContext()
                {
                    BlockIndex = 101,
                    PreviousStates = state,
                    Signer = adminAddress
                })
            );

            Assert.Throws<PermissionDeniedException>(
                () => action.Execute(new ActionContext()
                {
                    BlockIndex = 1,
                    PreviousStates = state,
                    Signer = new Address()
                })
            );
        }
    }
}
