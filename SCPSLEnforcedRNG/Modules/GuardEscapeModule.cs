using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synapse;
using Synapse.Api;
using Synapse.Api.Items;

namespace SCPSLEnforcedRNG.Modules
{
    public class GuardEscapeModule : BaseModule
    {
        public override string ModuleName { get { return "GuardEscape"; } }

        public override void Activate()
        {
            SynapseController.Server.Events.Player.PlayerEscapesEvent += OnEscape;
            Server.Get.RoleManager.RegisterCustomRole<MTFcommander>();
        }

        public void OnEscape(Synapse.Api.Events.SynapseEventArguments.PlayerEscapeEventArgs args)
        {
           if (args.Player.RoleType == RoleType.FacilityGuard && args.Player.IsCuffed == false)
            {
                args.Allow = true;
                args.Player.RoleID = 103;
            }
        }

    }
    public class MTFcommander : Synapse.Api.Roles.Role
    {
        public override int GetRoleID() => 103;
        public override string GetRoleName() => "NTF Commander";
        public override int GetTeamID() => (int)Team.MTF;
        public override List<int> GetFriendsID() => new List<int> { (int)Team.RSC, (int)Team.RIP }; 
        public override List<int> GetEnemiesID() => new List<int> { (int)Team.CHI, (int)Team.SCP, (int)Team.CDP };
        public override void Spawn()
        {
            Player.RoleType = RoleType.NtfPrivate;

            foreach(var item in Player.Inventory.Items)
                if (item.ItemType == ItemType.KeycardNTFOfficer) item.Destroy();

            Player.Inventory.AddItem(ItemType.KeycardNTFLieutenant);
            Player.Inventory.AddItem(ItemType.Adrenaline);
            Player.Inventory.AddItem(ItemType.Medkit);
        }

    }
}
