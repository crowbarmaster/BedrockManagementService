using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters {
    public class ConsoleGarbageFilter {

        public class CannedEntry {
            public int ID;
            public string Entry;
            public int TimesRepeated;

            public CannedEntry(int id, string content) {
                ID = id;
                Entry = content;
            }

            public override bool Equals(object? obj) {
                return obj is CannedEntry entry &&
                       ID == entry.ID;
            }

            public override int GetHashCode() {
                return HashCode.Combine(ID);
            }

            public override string ToString() {
                return Entry;
            }
        }

        List<CannedEntry> cannedEntries = new();
        List<string> filters = new();

        public ConsoleGarbageFilter() {
            if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.ServerErrorFilter))) {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Running AutoCompaction...");
                sb.AppendLine("}");
                sb.AppendLine("* Line");
                sb.AppendLine("]");
                sb.AppendLine("\" : [");
                sb.AppendLine("   ");
                sb.AppendLine(" doesn't have any recieves");
                sb.AppendLine("  Syntax error:");
                sb.AppendLine(" doesn't contain any offers");
                sb.AppendLine("[Animation] ");
                sb.AppendLine("[Molang] ");
                sb.AppendLine("[AI] ");
                sb.AppendLine("[Commands] ");
                sb.AppendLine("[Entity] ");
                sb.AppendLine("[Actor] ");
                sb.AppendLine("[Item] ");
                sb.AppendLine("[Json] ");
                sb.AppendLine("[Blocks] ");
                sb.AppendLine("[Components] ");
                sb.AppendLine("[Recipes] ");
                sb.AppendLine("[FeatureRegistry] ");
                sb.AppendLine("[Scripting] ");
                File.WriteAllText(GetServiceFilePath(MmsFileNameKeys.ServerErrorFilter), sb.ToString());
            }
            filters = new(File.ReadAllLines(GetServiceFilePath(MmsFileNameKeys.ServerErrorFilter)));
        }

        public string Filter(string input) {
            if (input.Contains("PrintCannedLineStats")) {
                int totalLinesIgnored = 0;
                cannedEntries.ForEach(canned => {
                    if(canned != null) { 
                        totalLinesIgnored += canned.TimesRepeated;
                    }
                });
                return $"{cannedEntries.Count} canned entries, {totalLinesIgnored} total lines ignored.";
            }
            CannedEntry result = cannedEntries.FirstOrDefault(x => x != null && string.IsNullOrEmpty(input) && x.Entry.Equals(input));
            if (result != null) {
                result.TimesRepeated++;
                return null;
            } else {
                foreach (string entry in filters) {
                    if (input.StartsWith(entry)) {
                        cannedEntries.Add(result);
                        return null;
                    }
                }
            }     
            return input;
        }
    }
}