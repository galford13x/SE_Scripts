using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace SE_Scripts
{
    class RefineryScript
    {
        public string RefineryTagPrefix = "[Auto:";
        public string RefineryTagSuffix = "]";

        public List<string> GlobalFilterOreOut = new List<string>()
        {
            "Stone",
        };

        void Main()
        {
            
        }

        public class RefineryManager
        {
            public List<IMyRefinery> Refineries = new List<IMyRefinery>(); 
        }
        #region Classes

        public class InventoryMap
        {
            public IMyInventory Inventory;
            public IMyInventoryItem Item;
            public IMyInventoryOwner Owner;
        }

        public class ItemManager
        {
            public Dictionary<string, double> Quotas;
            public IMyTerminalBlock DebugBlock;

            public ItemManager(Dictionary<string, double> quotas, IMyTerminalBlock debugBlock = null)
            {
                Quotas = quotas;
                DebugBlock = debugBlock;
            }

            public Dictionary<string, Dictionary<string, Dictionary<int, InventoryReporting.InventoryMap>>> ItemMap =
                new Dictionary<string, Dictionary<string, Dictionary<int, InventoryReporting.InventoryMap>>>(100);

            private int count;

            public void MapItems(IMyInventoryOwner owner)
            {
                for (int i = 0; i < owner.InventoryCount; i++)
                {
                    var inventory = owner.GetInventory(i);
                    var items = inventory.GetItems();
                    for (int j = 0; j < items.Count; j++)
                    {
                        var item = items[j];
                        var itemType = GetItemType(item);
                        if (ItemMap.ContainsKey(itemType) == false)
                            ItemMap[itemType] =
                                new Dictionary<string, Dictionary<int, InventoryReporting.InventoryMap>>();
                        if (ItemMap[itemType].ContainsKey(item.Content.SubtypeName) == false)
                            ItemMap[itemType][item.Content.SubtypeName] =
                                new Dictionary<int, InventoryReporting.InventoryMap>();
                        ItemMap[itemType][item.Content.SubtypeName][count++] = new InventoryReporting.InventoryMap()
                        {
                            Inventory = inventory,
                            Item = item,
                            Owner = owner,
                        };
                    }
                }
            }

            public InventoryReporting.InventoryMap[] GetMapOfSubtype(string type, string subtype)
            {
                if (ItemMap.ContainsKey(type) && ItemMap[type].ContainsKey(subtype))
                {
                    InventoryReporting.InventoryMap[] map =
                        new InventoryReporting.InventoryMap[ItemMap[type][subtype].Count];
                    ItemMap[type][subtype].Values.CopyTo(map, 0);
                    return map;
                }
                return new InventoryReporting.InventoryMap[0];
            }

            public IEnumerable<InventoryReporting.InventoryMap> GetAllBySubtype(string type, string subtype)
            {
                if (ItemMap.ContainsKey(type) && ItemMap[type].ContainsKey(subtype))
                    return ItemMap[type][subtype].Values;
                return new InventoryReporting.InventoryMap[0];
            }

            public double GetAmountBySubtype(string type, string subtype)
            {
                var enumerator = GetAllBySubtype(type, subtype).GetEnumerator();
                long amount = 0;
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    amount += current.Item.Amount.RawValue;
                }
                return amount/1e6;
            }

            public string GetItemReportByType(string type)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("== " + type + " ==");
                if (ItemMap.ContainsKey(type) == false) return sb.ToString();
                var subtypes = ItemMap[type];
                var subtypesEnum = subtypes.GetEnumerator();
                while (subtypesEnum.MoveNext())
                {
                    var currentSubtype = subtypesEnum.Current;
                    double amount = 0.0;
                    long rawAmount = 0;
                    var itemsEnum = currentSubtype.Value.GetEnumerator();
                    while (itemsEnum.MoveNext())
                    {
                        var currentItem = itemsEnum.Current;
                        rawAmount += currentItem.Value.Item.Amount.RawValue;
                    }
                    amount = rawAmount/1e6;
                    double quota = amount;
                    if (Quotas.ContainsKey(type + ":" + currentSubtype.Key))
                        quota = Quotas[type + ":" + currentSubtype.Key];
                    sb.AppendFormat("{0} - {1}/{2} - {3}%\n", currentSubtype.Key, amount, quota, 100.0*amount/quota);
                }
                return sb.ToString();

            }

            public string GenerateQuotaReport()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("== Quotas Not Met ==");
                var quotaEnum = this.Quotas.GetEnumerator();
                while (quotaEnum.MoveNext())
                {
                    var currentQuota = quotaEnum.Current;
                    string[] typeSubtype = currentQuota.Key.Split(':');
                    double amount = this.GetAmountBySubtype(typeSubtype[0], typeSubtype[1]);
                    if (amount < currentQuota.Value)
                        sb.AppendFormat("{0} - {1}/{2} - {3}%\n", currentQuota.Key, amount, currentQuota.Value,
                            100.0*amount/currentQuota.Value);
                }
                return sb.ToString();
            }


            public string GetItemType(IMyInventoryItem item)
            {
                string type = item.Content.TypeId.ToString();
                var tokens = type.Split('_');
                if (tokens.Length == 2) return tokens[1];
                else return type;
            }

        }

        #endregion

    }
}
