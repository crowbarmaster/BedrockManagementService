using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Json;
using System.Dynamic;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;

namespace BedrockService.Shared.LiteLoaderFileModels.JsonModels {
    public class LiteLoaderConfigNodeModel {
        public string Name { get; set; }
        public LiteLoaderConfigNodeModel ParentNode { get; set; }
        public Dictionary<string, dynamic> Properties { get; set; } = new();
        public List<LiteLoaderConfigNodeModel> ChildNodes { get; set; } = new();

        public LiteLoaderConfigNodeModel(string name, string input, LiteLoaderConfigNodeModel parentNode = null) {
            Name = name;
            ParentNode = parentNode;
            dynamic json = JsonSerializer.Deserialize<ExpandoObject>(input);
            Dictionary<string, string> children = new();
            foreach (var obj in json) {
                if (obj.Value.ValueKind == JsonValueKind.Object) {
                    string obString = obj.Value.ToString();
                    children.Add(obj.Key.ToString(), obString);
                } else {
                    string key = obj.Key.ToString();
                    dynamic value = obj.Value;
                    Properties.Add(key, value);
                }
            }
            if (children.Count > 0) {
                foreach (var child in children) {
                    ChildNodes.Add(new LiteLoaderConfigNodeModel(child.Key, child.Value, this));
                }
            }
        }

        public LiteLoaderConfigNodeModel GetChildByName(string name) {
            return ChildNodes[ChildNodes.IndexOf(ChildNodes.Where(x => x.Name == name).First())];
        }

        public void SaveToFile(string path) {
            if (ParentNode != null) {
                return;
            }
            File.WriteAllText(path, ToJsonString());
        }

        public string ToJsonString(int tabDepth = 1) {
            StringBuilder builder = new StringBuilder();
            List<string> keyCollection = new();
            keyCollection.AddRange(Properties.Keys);
            keyCollection.AddRange(ChildNodes.Select(x => x.Name).ToArray());
            keyCollection.Sort();
            builder.AppendLine("{");
            int count = 1;
            foreach (string key in keyCollection) {
                if (Properties.ContainsKey(key)) {
                    AppendProperty(builder, key, Properties[key], tabDepth);
                } else {
                    AppendChildNode(builder, ChildNodes.Where(x => x.Name == key).First(), tabDepth);
                }
                if (count < keyCollection.Count) {
                    builder.AppendLine(",");
                } else {
                    builder.AppendLine();
                }
                count++;
            }
            builder.Append(PrintTabDepth(tabDepth - 1));
            builder.Append("}");
            return builder.ToString();
        }

        private void AppendProperty(StringBuilder builder, string key, dynamic value, int tabDepth) {
            builder.Append(PrintTabDepth(tabDepth));
            string val = string.Empty;
            if (value.GetType() == typeof(JsonElement)) {
                val = value.ValueKind == JsonValueKind.String ? ToLiteral(value.ToString()) : value.ToString().ToLower();
            } else {
                val = value.GetType() == typeof(string) ? ToLiteral(value.ToString()) : value.ToString().ToLower();
            }
            builder.Append($"\"{key}\": {val}");
        }

        private void AppendChildNode(StringBuilder builder, LiteLoaderConfigNodeModel child, int tabDepth) {
            builder.Append(PrintTabDepth(tabDepth));
            builder.Append($"\"{child.Name}\": {child.ToJsonString(tabDepth + 1)}");
        }

        private static string PrintTabDepth(int tabDepth) {
            StringBuilder builder = new();
            for (int i = 0; i < tabDepth; i++) {
                builder.Append("    ");
            }
            return builder.ToString();
        }

        private static string ToLiteral(string input) {
            using var writer = new StringWriter();
            using var provider = CodeDomProvider.CreateProvider("CSharp");
            provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
            return writer.ToString();
        }
    }
}
