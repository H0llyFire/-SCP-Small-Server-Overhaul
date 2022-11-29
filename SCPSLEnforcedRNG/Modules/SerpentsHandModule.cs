using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
    public class SerpentsHand : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Overrides
        public override string ModuleName => "SerpentsHand";
        public override void Activate()
        {
        }
        public override void SetUpRound()
        {
        }
        //-------------------------------------------------------------------------------
        //Main
        //-------------------------------------------------------------------------------
        //Events
        //-------------------------------------------------------------------------------
        //Roles
        public class SerpentsHandPerson : Synapse.Api.Roles.Role
        {
            public override int GetRoleID() => 110;
            public override string GetRoleName() => "Serpent's Hand";
            public override void Spawn()
            {

            }
        }
    }
}
