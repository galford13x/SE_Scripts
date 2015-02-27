using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace SE_Scripts
{
    class InventoryReporting
    {
        public static IMyGridTerminalSystem GridTerminalSystem;

        #region Settings
        public const int ReportColumnWidth_ItemName = 15;
        public const int ReportColumnWidth_CurrentAmount = 8;
        public const int ReportColumnWidth_GoalAmount = 8;
        public const int ReportColumnWidth_Percent = 4;


        void LoadSettings()
        {
            // Add new reports to generate and print     
            // param: TargetBlockName - Where to print the info (Must be a text panel or LCD   
            // param: TargetBlockTitle - Reserved     
            // param: Delegate to generate the report.  Allows someone to easily generate their own report     
            //ItemReport.Add("Debug1", "DEBUG TITLE", GenerateOreReport);    
            ItemReport.Add(new List<string>() { "Components1", "Components2" }, 16, GenerateComponentReport);
            //ItemReport.Add("Debug2", "DEBUG TITLE1", GenerateIngotReport);    
            //ItemReport.Add(new List<string>() { "LCD2" }, 1.1, GenerateComponentReport);  
            //ItemReport.Add("Debug2", "DEBUG TITLE1", GenerateAmmoMagazineReport);    

            // Load Quotas         
            // param: Type:Subtype - Type/Category of the item : The Specific name of the Item;     
            // param: Amount - the quota impolsed on that type of item  
            Quota.Add("Ingot", "Iron", 1000);
            Quota.Add("Ingot", "Uranium", 1000);
            Quota.Add("Ingot", "Cobalt", 1000);
            Quota.Add("Ingot", "Magnesium", 1000);
            Quota.Add("Ingot", "Nickel", 1000);
            Quota.Add("Ingot", "Silver", 1000);
            Quota.Add("Ingot", "Silicon", 1000);
            Quota.Add("Ingot", "Platinum", 1000);
            Quota.Add("Ingot", "Gold", 1000);
            Quota.Add("Component", "SteelPlate", 1000);

            GridSystem = GridTerminalSystem;
        }

        public static IMyGridTerminalSystem GridSystem;
        // Contains custom item reports to print to textpanels and LCDs   
        // Add more reports to be displayed in the LoadSettings function below   
        public static Dictionary<int, ItemReport> Reports = new Dictionary<int, ItemReport>();

        #endregion Settings
        public static IMyTerminalBlock DebugBlock;

        public void Main()
        {
            this.Debug("", true);
            //Reset(); //DEBUG   

            LoadSettings();

            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(blocks);
            ItemManager itemManager = GetItems(blocks);
            PrintReports(itemManager, Reports.Values);
        }


        /*  
                // Prints reports to displays as specified in the Report settings    
                private void PrintReports(ItemManager itemManager, IEnumerable<ItemReport> reportsEnum)  
                {  
                    var reports = reportsEnum.GetEnumerator();  
                    while (reports.MoveNext())  
                    {  
                        var report = reports.Current;  
                        List<IMyTerminalBlock> displayBlocks = new List<IMyTerminalBlock>();  
                        GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displayBlocks, report.IsValid);  
                        if (displayBlocks.Count == 0)  
                        {  
                            Debug("displayBlock not found or is not a valid TextPanel/LCD: " + report.TargetPanels);  
                            continue;  
                        }  
                        string reportData = report.ReportGenerator(itemManager);  
                        WritePublicToLCD(displayBlock, reportData);  
                    }  
                }  
        */
        // Prints reports to displays as specified in the Report settings    
        private void PrintReports(ItemManager itemManager, IEnumerable<ItemReport> reportsEnum)
        {
            var reports = reportsEnum.GetEnumerator();
            while (reports.MoveNext())
            {
                var report = reports.Current;
                report.PrintToPanels(itemManager);

            }
        }

        #region InventoryReport Functions




        public static List<string> GenerateOreReport(ItemManager itemManager)
        {
            return itemManager.GetItemReportByType("Ore");
        }

        public static List<string> GenerateIngotReport(ItemManager itemManager)
        {
            return itemManager.GetItemReportByType("Ingot");
        }

        public static List<string> GenerateComponentReport(ItemManager itemManager)
        {
            return itemManager.GetItemReportByType("Component");
        }
        public static List<string> GenerateAmmoMagazineReport(ItemManager itemManager)
        {
            return itemManager.GetItemReportByType("AmmoMagazine");
        }

        private List<string> GenerateQuotaReport(ItemManager itemManager)
        {
            return itemManager.GenerateQuotaReport();
        }


        #endregion InventoryReport Functions


        #region Utilities
        // Maps all items in the given blocks to inventorys and blocks mapped via item Type and Subtype   
        private ItemManager GetItems(List<IMyTerminalBlock> blocks)
        {
            ItemManager manager = new ItemManager(Quota.Quotas, DebugBlock);
            for (int i = 0; i < blocks.Count; i++)
            {
                IMyInventoryOwner owner = blocks[i] as IMyInventoryOwner;
                if (owner != null)
                {
                    manager.MapItems(owner);
                }
            }
            return manager;
        }
        // Write a message to an IMyTextPanel (TextPanel or LCD)   
        public static void WritePublicToLCD(IMyTextPanel panel, string message, bool append = false)
        {
            panel.WritePublicText(message, append);
            panel.ShowTextureOnScreen();
            panel.ShowPublicTextOnScreen();
        }


        #endregion Utilities

        #region Custom Classes

        public class Quota
        {
            public string Type;
            public string Subtype;
            public double RequiredAmount;
            public double CurrentAmount = -1;
            // Contains a custom quota list per item   
            // Add Quotas in the LoadSettings() function below   
            public static Dictionary<string, Quota> Quotas = new Dictionary<string, Quota>(10);

            public static void Add(string type, string subtype, double requiredAmount)
            {
                Quotas[type + ":" + subtype] = new Quota() { Type = type, Subtype = subtype, RequiredAmount = requiredAmount };
            }
        }

        public class InventoryMap
        {
            public IMyInventory Inventory;
            public IMyInventoryItem Item;
            public IMyInventoryOwner Owner;
        }

        public class ItemManager
        {
            public Dictionary<string, Quota> Quotas;
            public IMyTerminalBlock DebugBlock;
            public ItemManager(Dictionary<string, Quota> quotas, IMyTerminalBlock debugBlock = null)
            {
                Quotas = quotas;
                DebugBlock = debugBlock;
            }
            public Dictionary<string, Dictionary<string, Dictionary<int, InventoryMap>>> ItemMap =
                new Dictionary<string, Dictionary<string, Dictionary<int, InventoryMap>>>(100);

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
                            ItemMap[itemType] = new Dictionary<string, Dictionary<int, InventoryMap>>();
                        if (ItemMap[itemType].ContainsKey(item.Content.SubtypeName) == false)
                            ItemMap[itemType][item.Content.SubtypeName] = new Dictionary<int, InventoryMap>();
                        ItemMap[itemType][item.Content.SubtypeName][count++] = new InventoryMap()
                        {
                            Inventory = inventory,
                            Item = item,
                            Owner = owner,
                        };
                    }
                }
            }

            public InventoryMap[] GetMapOfSubtype(string type, string subtype)
            {
                if (ItemMap.ContainsKey(type) && ItemMap[type].ContainsKey(subtype))
                {
                    InventoryMap[] map = new InventoryMap[ItemMap[type][subtype].Count];
                    ItemMap[type][subtype].Values.CopyTo(map, 0);
                    return map;
                }
                return new InventoryMap[0];
            }

            public IEnumerable<InventoryMap> GetAllBySubtype(string type, string subtype)
            {
                if (ItemMap.ContainsKey(type) && ItemMap[type].ContainsKey(subtype))
                    return ItemMap[type][subtype].Values;
                return new InventoryMap[0];
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
                return amount / 1e6;
            }

            public List<string> GetItemReportByType(string type)
            {
                //StringBuilder sb = new StringBuilder();  
                List<string> lines = new List<string>(30);
                lines.Add("== " + type + " ==");
                //sb.AppendLine("== " + type + " ==");  
                if (ItemMap.ContainsKey(type) == false) return lines;
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
                    amount = rawAmount / 1e6;
                    Quota quota = Quotas.ContainsKey(type + ":" + currentSubtype.Key)
                        ? Quotas[type + ":" + currentSubtype.Key]
                        : new Quota() { RequiredAmount = amount };
                    quota.CurrentAmount = amount;
                    //sb.AppendFormat("{4}:{0} - {1}/{2} - {3}%\n", currentSubtype.Key, amount, quota.RequiredAmount, 100.0 * amount / quota.RequiredAmount, count++);  
                    lines.Add(string.Format("{0} - {1}/{2} - {3}%", currentSubtype.Key, amount, quota.RequiredAmount, 100.0 * amount / quota.RequiredAmount));
                }
                return lines;

            }
            public List<string> GenerateQuotaReport()
            {
                StringBuilder sb = new StringBuilder();
                List<string> lines = new List<string>(30);
                lines.Add("== Quotas Not Met ==");
                sb.AppendLine("== Quotas Not Met ==");
                var quotaEnum = this.Quotas.GetEnumerator();
                while (quotaEnum.MoveNext())
                {
                    var currentQuota = quotaEnum.Current;
                    string[] typeSubtype = currentQuota.Key.Split(':');
                    double amount = this.GetAmountBySubtype(typeSubtype[0], typeSubtype[1]);
                    if (amount < currentQuota.Value.RequiredAmount)
                        //sb.AppendFormat("{0} - {1}/{2} - {3}%\n", currentQuota.Key, amount, currentQuota.Value.RequiredAmount, 100.0 * amount / currentQuota.Value.RequiredAmount);  
                        lines.Add(string.Format("{0} - {1}/{2} - {3}%", currentQuota.Key, amount, currentQuota.Value.RequiredAmount, 100.0 * amount / currentQuota.Value.RequiredAmount));
                    currentQuota.Value.CurrentAmount = amount;
                }
                return lines;
            }


            public string GetItemType(IMyInventoryItem item)
            {
                string type = item.Content.TypeId.ToString();
                var tokens = type.Split('_');
                if (tokens.Length == 2) return tokens[1];
                else return type;
            }

        }

        public class ItemReport
        {
            public ItemReport(List<string> targetPanels, string targetPublicTitle,
                Func<ItemManager, List<string>> reportGenerator)
            {
                TargetPanels = targetPanels;
                //TargetPublicTitle = targetPublicTitle;  
                ReportGenerator = reportGenerator;
            }

            private static int count;
            //public List<string> TargetPanels;   
            public List<string> TargetPanels;
            public int LinesPerLCD;
            public Func<ItemManager, List<string>> ReportGenerator;

            public static void Add(string targetPanelName, int linesPerLCD,
                Func<ItemManager, List<string>> reportGenerator)
            {
                Reports.Add(count++, new ItemReport(new List<string> { targetPanelName }, "", reportGenerator) { LinesPerLCD = linesPerLCD });
            }
            public static void Add(List<string> panels, int linesPerLCD,
                Func<ItemManager, List<string>> reportGenerator)
            {
                Reports.Add(count++, new ItemReport(panels, "", reportGenerator) { LinesPerLCD = linesPerLCD });
            }

            public bool IsValid(IMyTerminalBlock block)
            {
                return TargetPanels.Contains(block.CustomName);
            }

            public void PrintToPanels(ItemManager itemManager)
            {
                var reportData = ReportGenerator(itemManager);
                DebugStatic(DebugBlock, "NumReports: " + reportData.Count);
                var lcdPanels = GetLCDPanels();

                StringBuilder sb = new StringBuilder();
                for (int k = 0; k < TargetPanels.Count; k++)
                {
                    IMyTextPanel panel = lcdPanels[k] as IMyTextPanel;
                    int end = (k + 1) * LinesPerLCD;
                    end = end < reportData.Count ? end : reportData.Count;
                    //DebugStatic(DebugBlock, "End = " + end);  
                    for (int i = k * LinesPerLCD; i < end; i++)
                    {
                        //DebugStatic(DebugBlock, "Printing " + i);  
                        sb.AppendLine(i + ": " + reportData[i]);
                    }
                    WritePublicToLCD(panel, sb.ToString());
                    sb.Clear();
                }
            }

            public List<IMyTerminalBlock> GetLCDPanels()
            {
                List<IMyTerminalBlock> lcdPanels = new List<IMyTerminalBlock>();
                GridSystem.GetBlocksOfType<IMyTextPanel>(lcdPanels, IsValid);
                IMyTerminalBlock[] sorted = new IMyTerminalBlock[lcdPanels.Count];
                for (int i = 0; i < TargetPanels.Count; i++)
                {
                    for (int k = 0; k < lcdPanels.Count; k++)
                    {
                        if (lcdPanels[k].CustomName.Equals(TargetPanels[i]))
                        {
                            sorted[i] = lcdPanels[k];
                            break;
                        }
                    }
                }
                return new List<IMyTerminalBlock>(sorted);
            }
        }
        #endregion Custom Classes

        #region Debug Stuff
        public void Debug(string text, bool clear = false)
        {
            List<IMyTerminalBlock> debugs = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName("DEBUG", debugs);
            if (debugs.Count <= 0) return;
            DebugBlock = debugs[0];

            for (int d = 0; d < debugs.Count; d++)
            {

                if (debugs[d].CustomName.StartsWith("DEBUG") == false) continue;
                IMyTerminalBlock debug = debugs[d];
                if (clear) debug.SetCustomName("DEBUG");
                debug.SetCustomName(debug.CustomName + "\n" + text);
            }
        }
        //public static class Tmp   
        //{   
        //public static IMyTerminalBlock DebugBlock;   
        //}   

        public static void DebugStatic(IMyTerminalBlock block, string text, bool clear = false)
        {
            if (block == null) return;

            //if (block.CustomName.StartsWith("DEBUG") == false) return;      

            if (clear) block.SetCustomName("DEBUG");
            block.SetCustomName(block.CustomName + "\n" + text);
        }
        void Reset()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName("LCD", blocks, GetOnlyDebugs);
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].SetCustomName("LCD" + (i + 1));
            }
        }
        // DEBUG   
        private bool GetOnlyDebugs(IMyTerminalBlock arg)
        {
            if (arg.CustomName.StartsWith("LCD1")) return true;
            else if (arg.CustomName.StartsWith("LCD2")) return true;
            else return false;
        }

        #endregion Debug Stuff
    }
}
