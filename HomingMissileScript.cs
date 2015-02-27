using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using IMyTerminalBlock = Sandbox.ModAPI.Ingame.IMyTerminalBlock;

namespace SE_Scripts
{
    class HomingMissileScript
    {
        private IMyGridTerminalSystem GridTerminalSystem;

        List<IMyTerminalBlock> turrets = new List<IMyTerminalBlock>();
        Dictionary<IMyTerminalBlock, long> CurrentAmmo = new Dictionary<IMyTerminalBlock, long>();
        Dictionary<IMyTerminalBlock, long> PreviousAmmo = new Dictionary<IMyTerminalBlock, long>();

        void Main()
        {
            GridTerminalSystem.GetBlocksOfType<IMyLargeGatlingTurret>(turrets);

            for (int i = 0; i < turrets.Count; i++)
            {
                CurrentAmmo.Add(turrets[i], GetAmmoCount(turrets[i]));
                var value = GetPreviousAmmoCount1(turrets[i]);
            }

        }

        private long GetPreviousAmmoCount1(IMyTerminalBlock myTerminalBlock)
        {
            // format is; TurretName:AmmoQty  (e.g. West:132)
            string[] tokens = myTerminalBlock.CustomName.Split(':');
            if(tokens.Length != 2) myTerminalBlock.SetCustomName(myTerminalBlock.CustomName + ":" + GetAmmoCount(myTerminalBlock));
            return long.Parse(tokens[1]);
        }

        public Dictionary<IMyTerminalBlock, long> GetPreviousAmmoCount()
        {
            List<IMyTerminalBlock> ammoBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName("Ammo", ammoBlocks);
            if (ammoBlocks.Count == 0) return new Dictionary<IMyTerminalBlock, long>();
            var ammoBlock = ammoBlocks[0];
            // Previous Ammo counts stored on a block in CustomName
            // Format is;  Ammo-West:#;East
            string[] tokens = ammoBlock.CustomName.Split('-');
            return null;
        }

        public long GetAmmoCount(string blockName)
        {
            return 0;
        }

        public long GetAmmoCount(IMyTerminalBlock block)
        {
            var inventory = block.GetInventory(0);
            var items = inventory.GetItems();
            long amount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Content.SubtypeName == "25x184mm NATO ammo container") ;
                amount += item.Amount.RawValue;
            }
            return amount / 1000000;
        }
    }
}
