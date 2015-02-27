using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace SE_Scripts
{


    public class InventorySorting
    {
        private IMyGridTerminalSystem GridTerminalSystem;
       // Fix Array Index in MultiDicitonary GetKeyPairs() functions
 
        #region Settings 
        public const int ReportColumnWidth_ItemName = 15; 
        public const int ReportColumnWidth_CurrentAmount = 8; 
        public const int ReportColumnWidth_GoalAmount = 8; 
        public const int ReportColumnWidth_Percent = 4; 
        private IMyTextPanel DebugPanel; 
 
        public static MultiDictionary<string, string, double> Quotas = new MultiDictionary<string, string, double>(); 
        void LoadSettings() 
        { 
            // Add new reports to generate and print  
            // param: TargetBlockName - Where to print the info  
            // param: TargetBlockTitle - Reserved  
            // param: Delegate to generate the report.  Allows someone to easily generate their own report  
            //ItemReport.Add("Debug1", "DEBUG TITLE", GenerateOreReport); 
            ItemReport.Add("LCD1", "DEBUG TITLE1", GenerateQuotaReport1); 
            //ItemReport.Add("Debug2", "DEBUG TITLE1", GenerateIngotReport); 
            ItemReport.Add("LCD2", "DEBUG TITLE1", GenerateComponentReport); 
            //ItemReport.Add("Debug2", "DEBUG TITLE1", GenerateAmmoMagazineReport); 
 
            // Load Quotas      
            // param: Type - Type/Category of the item  
            // param: Subtype - The specific name of the Item  
            // param: Amount - the quota impolsed on that type of item  
            Quotas["Ingot", "Iron"] = 1000; 
            Quotas["Ingot", "Uranium"] = 1000; 
            Quotas["Ingot", "Cobalt"] = 1000; 
            Quotas["Ingot", "Magnesium"] = 1000; 
            Quotas["Ingot", "Nickel"] = 1000; 
            Quotas["Ingot", "Silver"] = 1000; 
            Quotas["Ingot", "Silicon"] = 1000; 
            Quotas["Ingot", "Platinum"] = 1000; 
            Quotas["Ingot", "Gold"] = 1000; 
            Quotas["Component", "SteelPlate"] = 1000; 
 
         //Quotas.DebugBlock = Tmp.DebugBlock;
        } 
        #endregion Settings 
 
        #region Test Functions 
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
        #endregion Test Functions 
 public class MyClass
{
        public int i = 1;
        public int j = 2;
}
        public void Main() 
        { 
            this.Debug("", true); 
            IMyTerminalBlock myclass = GridTerminalSystem.GetBlockWithName("LCD1");
if(myclass == null) Debug("is null");
            object o = myclass;
            IMyTerminalBlock myclass2 = o as IMyTerminalBlock;
            if(o is IMyTerminalBlock) Debug("o is MyClass"); else Debug("o is not MyClass");
            
            Reset(); //DEBUG
return;
            LoadSettings(); 
    
            var blocks = new List<IMyTerminalBlock>(); 
            GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(blocks); 
             
            PerformInventoryReporting(blocks); 
        } 
 
        // Inventory Reporting Execution 
        void PerformInventoryReporting(List<IMyTerminalBlock> blocks) 
        { 
            var itemList = new MyList<InventoryMap>(); 
            GetFullInventoryList(blocks, itemList); 
            InventoryReporter iManager = new InventoryReporter(); 
 
            for (int i = 0; i < itemList.Count; i++) 
            { 
                for (int j = 0; j < itemList[i].Items.Count; j++) 
                { 
                    iManager.AddItemToInventory(itemList[i].Items[j]); 
                } 
            } 
 
            PrintReports(iManager, ItemReport.Reports); 
        } 
 
        // Prints reports to displays as specified in the Report settings 
        private void PrintReports(InventoryReporter reporter, MyList<ItemReport> reports) 
        { 
            for (int i = 0; i < reports.Count; i++) 
            { 
                var report = reports[i]; 
                var displayBlock = GridTerminalSystem.GetBlockWithName(report.TargetBlockName) as IMyTextPanel; 
                if (displayBlock == null)
                {
                    Debug("displayBlock not found " + report.TargetBlockName);
                    continue; 
                }
                string reportData = report.ReportGenerator(reporter); 
                displayBlock.WritePublicText(reportData);
                displayBlock.ShowTextureOnScreen();
                displayBlock.ShowPublicTextOnScreen();
                //displayBlock.SetCustomName(report.TargetBlockName + "\n" + reportData); 
            } 
        } 
 
        #region InventoryReport Functions 
 
        private void GetFullInventoryList(List<IMyTerminalBlock> blocks, MyList<InventoryMap> itemList) 
        { 
            for (int i = 0; i < blocks.Count; i++) 
            { 
                var owner = (blocks[i] as IMyInventoryOwner); 
                AddInveotry(owner, itemList); 
            } 
        } 
 
        public void AddInveotry(IMyInventoryOwner owner, MyList<InventoryMap> itemList) 
        { 
 
            for (int i = 0; i < owner.InventoryCount; i++) 
            { 
                var inventory = owner.GetInventory(i); 
                itemList.Add(new InventoryMap { Inventory = inventory, Items = inventory.GetItems() }); 
            } 
        } 
 
        public class InventoryReporter 
        { 
            /// <summary>  
            /// Contains a list of items by SubType and Amount (string,float)  
            /// </summary>  
            //public Dictionary<string, long> Inventory = new Dictionary<string, long>();  
            //public Dictionary<string, InventoryItemBySubtype> InventoryBySubtype = new Dictionary<string, InventoryItemBySubtype>();  
            public Dictionary<string, Dictionary<string, InventoryItemBySubtype>> InventoryByType = new Dictionary<string, Dictionary<string, InventoryItemBySubtype>>(); 
 
            public void AddItemToInventory(IMyInventoryItem item) 
            { 
                var name = item.Content.SubtypeName; 
                var type = GetItemType(item); 
                if (InventoryByType.ContainsKey(type) == false) 
                    InventoryByType[type] = new Dictionary<string, InventoryItemBySubtype>(); 
                var bySubtype = InventoryByType[type]; 
                if (bySubtype.ContainsKey(name) == false) 
                    bySubtype[name] = new InventoryItemBySubtype(type, name); 
                bySubtype[name].Items.Add(item); 
                bySubtype[name].RawAmount += item.Amount.RawValue; 
            } 
 
            public string GetItemType(IMyInventoryItem item) 
            { 
                string type = item.Content.TypeId.ToString(); 
                var tokens = type.Split('_'); 
                if (tokens.Length == 2) return tokens[1]; 
                else return type; 
            } 
 
        } 
        #region Report Generators 
        /*  
        public string GenerateReport(InventoryReporter inventorReporter)  
        {  
            StringBuilder sb = new StringBuilder();  
            var typeKeys = new string[inventorReporter.InventoryByType.Keys.Count];  
            inventorReporter.InventoryByType.Keys.CopyTo(typeKeys, 0);  
  
  
            for (int i = 0; i < inventorReporter.InventoryByType.Count; i++)  
            {  
                var inventoryBySubtype = inventorReporter.InventoryByType[typeKeys[i]];  
                var subtypeKeys = new string[inventoryBySubtype.Keys.Count];  
                inventoryBySubtype.Keys.CopyTo(subtypeKeys, 0);  
                sb.AppendLine("== " + typeKeys[i] + " ==");  
                for (int j = 0; j < inventoryBySubtype.Count; j++)  
                {  
                    var subtypeKey = subtypeKeys[j];  
                    sb.AppendLine(inventoryBySubtype[subtypeKey].ToString());  
                }  
            }  
  
            return sb.ToString();  
        }  
*/ 
 
        public static string GenerateOreReport(InventoryReporter reporter) 
        { 
            return GenerateReportFor("Ore", reporter.InventoryByType["Ore"]); 
        } 
 
        public static string GenerateIngotReport(InventoryReporter reporter) 
        { 
            return GenerateReportFor("Ingot", reporter.InventoryByType["Ingot"]); 
        } 
 
        public static string GenerateComponentReport(InventoryReporter reporter) 
        { 
            return GenerateReportFor("Component", reporter.InventoryByType["Component"]); 
        } 
        public static string GenerateAmmoMagazineReport(InventoryReporter reporter) 
        { 
            return GenerateReportFor("AmmoMagazine", reporter.InventoryByType["AmmoMagazine"]); 
        } 
 
        public string GenerateQuotaReport(InventoryReporter reporter) 
        { 
            StringBuilder sb = new StringBuilder(); 
            var typeKeys = new string[reporter.InventoryByType.Keys.Count]; 
            reporter.InventoryByType.Keys.CopyTo(typeKeys, 0); 
            int count = 0; 
            sb.AppendLine("== Quota Not Met =="); 
            for (int i = 0; i < reporter.InventoryByType.Count; i++) 
            { 
                var inventoryBySubtype = reporter.InventoryByType[typeKeys[i]]; 
                var subtypeKeys = new string[inventoryBySubtype.Keys.Count]; 
                inventoryBySubtype.Keys.CopyTo(subtypeKeys, 0); 
 
                for (int j = 0; j < inventoryBySubtype.Count; j++) 
                { 
                    var subtypeKey = subtypeKeys[j]; 
 
                    if (Quotas.ContainsKey(typeKeys[i], subtypeKey) && 
                        inventoryBySubtype[subtypeKey].Amount < Quotas[typeKeys[i], subtypeKey]) 
                    { 
                        sb.AppendLine(inventoryBySubtype[subtypeKey].ToString()); 
                        count++; 
                    } 
                } 
            } 
            return sb.ToString(); 
        } 
 
        public string GenerateQuotaReport1(InventoryReporter reporter) 
        {             
            StringBuilder sb = new StringBuilder(); 
            sb.AppendLine("== Quotas Not Met =="); 
            var quotaKeys = Quotas.GetKeyPairs();             
            
            for (int i = 0; i < quotaKeys.Count; i++) 
            { 
                var kvp = quotaKeys[i];
                if (reporter.InventoryByType.ContainsKey(quotaKeys[i].Key) && 
                    reporter.InventoryByType[quotaKeys[i].Key].ContainsKey(quotaKeys[i].Value)) 
                { 
                
                    sb.AppendLine(reporter.InventoryByType[quotaKeys[i].Key][quotaKeys[i].Value].ToString()); 
                } 
                else 
                { 
                    
                    double quotaVal = Quotas[quotaKeys[i].Key, quotaKeys[i].Value]; 
                    sb.AppendFormat("{0} {1} {2}\n", quotaKeys[i].Value.PadLeft(ReportColumnWidth_ItemName), 
                        0 + "/" + quotaVal, 0); 
                } 
            } 
            return sb.ToString(); 
        } 
 
        public static string GenerateReportFor(string type, Dictionary<string, InventoryItemBySubtype> itemsBySubtype) 
        { 
            StringBuilder sb = new StringBuilder(); 
            var subtypeKeys = new string[itemsBySubtype.Keys.Count]; 
            itemsBySubtype.Keys.CopyTo(subtypeKeys, 0); 
            sb.AppendLine("== " + type + " =="); 
            for (int j = 0; j < itemsBySubtype.Count; j++) 
            { 
                var subtypeKey = subtypeKeys[j]; 
                sb.AppendLine(itemsBySubtype[subtypeKey].ToString()); 
            } 
            return sb.ToString(); 
        } 
 
        #endregion Report Generators 
 
        public class ItemReport 
        { 
            public ItemReport(string targetBlockName, string targetPublicTitle, 
                Func<InventoryReporter, string> reportGenerator) 
            { 
                TargetBlockName = targetBlockName; 
                TargetPublicTitle = targetPublicTitle; 
                ReportGenerator = reportGenerator; 
            } 
            public string TargetBlockName; 
            public string TargetPublicTitle; 
            public Func<InventoryReporter, string> ReportGenerator; 
            public static MyList<ItemReport> Reports = new MyList<ItemReport>(); 
 
 
            public static void Add(string targetBlockName, string targetPublicTitle, 
                Func<InventoryReporter, string> reportGenerator) 
            { 
                Reports.Add(new ItemReport(targetBlockName, targetPublicTitle, reportGenerator)); 
            } 
        } 
 
        public class MultiDictionary<Key1, Key2, Tout> 
        { 
             public IMyTerminalBlock DebugBlock;
            public Tout this[Key1 key1, Key2 key2] 
            { 
                get { return _dict[key1][key2]; } 
                set 
                { 
                    if (!_dict.ContainsKey(key1)) _dict[key1] = new Dictionary<Key2, Tout>(); 
                    _dict[key1][key2] = value; 
                } 
            } 
            private readonly Dictionary<Key1, Dictionary<Key2, Tout>> _dict = 
                new Dictionary<Key1, Dictionary<Key2, Tout>>(); 
 
            public bool ContainsKey(Key1 key1, Key2 key2) 
            { 
                return _dict.ContainsKey(key1) && _dict[key1].ContainsKey(key2); 
            } 
 
            public List<KeyValuePair<Key1, Key2>> GetKeyPairs() 
            { 
                List<KeyValuePair<Key1, Key2> > keyPairs = new List<KeyValuePair<Key1, Key2>>(); 

                Key1[] key1 = new Key1[_dict.Count]; 
                _dict.Keys.CopyTo(key1,0); 
                
                for (int i = 0; i < _dict.Count; i++) 
                { 
                    Key2[] key2 = new Key2[_dict[key1[i]].Count]; 
                    
                    _dict[key1[i]].Keys.CopyTo(key2,0);
                    for (int j = 0; j < _dict[key1[i]].Count; j++) 
                    {                       
                        keyPairs.Add(new KeyValuePair<Key1, Key2>(key1[i], key2[j])); 
                    } 
                } 
                 
                return keyPairs; 
            } 
        } 
 
        public class InventoryItemBySubtype 
        { 
            public string Type; 
            public string Subtype; 
            public List<IMyInventoryItem> Items; 
 
            public long RawAmount; 
 
            public double Amount { get { return RawAmount / 10e6d; } } 
            public InventoryItemBySubtype(string type, string subtype, List<IMyInventoryItem> items = null) 
            { 
                Type = type; 
                Subtype = subtype; 
                Items = items ?? new List<IMyInventoryItem>(); 
            } 
 
            public long GetTotalAmount() 
            { 
                long amount = 0; 
                for (int i = 0; i < Items.Count; i++) 
                { 
                    amount += Items[i].Amount.RawValue; 
                } 
                return amount; 
            } 
 
            public override string ToString() 
            { 
                double goal = Quotas.ContainsKey(Type, Subtype) ? Quotas[Type, Subtype] : Amount; 
 
                return Subtype.PadRight(ReportColumnWidth_ItemName) + 
                       (Amount + "/" + goal).PadRight(ReportColumnWidth_CurrentAmount + ReportColumnWidth_GoalAmount) + 
                       (100.0 * Amount / goal).ToString("0.0"); 
            } 
        } 
        #endregion InventoryReport Functions 
 
        public class InventoryMap 
        { 
            public IMyInventory Inventory; 
            public List<IMyInventoryItem> Items; 
        } 
 
        #region Utilities 
        public class MyList<T> 
        { 
            private Dictionary<int, T> _dictionary; 
            private List<int> _keys; 
            private int _currentIndex = 0; 
            public MyList(int size = 20) 
            { 
                _dictionary = new Dictionary<int, T>(size); 
                _keys = new List<int>(size); 
            } 
 
            public void RemoveAt(int index) 
            { 
                _dictionary.Remove(_keys[index]); 
                _keys.RemoveAt(index); 
            } 
 
            public T this[int index] 
            { 
                get { return _dictionary[_keys[index]]; } 
                set { _dictionary[_keys[index]] = value; } 
            } 
 
            public void Add(T item) 
            { 
 
                int index = _keys.Count == 0 ? 0 : _keys[_keys.Count - 1] + 1; 
                _dictionary.Add(index, item); 
                _keys.Add(index); 
            } 
 
            public void Clear() 
            { 
                _dictionary.Clear(); 
                _keys.Clear(); 
            } 
 
            public bool Contains(T item) 
            { 
                return _dictionary.ContainsValue(item); 
            } 
 
            public int Count { get { return _dictionary.Count; } } 
 
            public bool Remove(T item) 
            { 
                for (int i = 0; i < _keys.Count; i++) 
                { 
                    if (_dictionary[_keys[i]].Equals(item)) 
                    { 
                        _dictionary.Remove(_keys[i]); 
                        _keys.RemoveAt(i); 
                        return true; 
                    } 
                } 
                return false; 
            } 
        } 
 
        public int[] GetKeys<T>(Dictionary<int, T> list) 
        { 
            int[] keys = new int[list.Count]; 
            list.Keys.CopyTo(keys, 0); 
            return keys; 
        } 
        #endregion Utilities 
 
        #region Debug Stuff 
        public static IMyTerminalBlock DebugBlock;
        public void Debug(string text, bool clear = false) 
        { 
            List<IMyTerminalBlock> debugs = new List<IMyTerminalBlock>(); 
            GridTerminalSystem.SearchBlocksOfName("DEBUG", debugs); 
            if (debugs.Count <= 0) return; 

            for (int d = 0; d < debugs.Count; d++) 
            { 
                
                if (debugs[d].CustomName.StartsWith("DEBUG") == false) continue; 
                IMyTerminalBlock debug = debugs[d]; 
                if (clear) debug.SetCustomName("DEBUG"); 
                debug.SetCustomName(debug.CustomName + "\n" + text); 
            } 
        } 

     public static void DebugStatic(IMyTerminalBlock block, string text, bool clear = false) 
        { 
            if (block == null) return;   
                 
                if (clear) block.SetCustomName("DEBUG");   
                block.SetCustomName(block.CustomName + "\n" + text);   
        }
        #endregion Debug Stuff
    }
}
