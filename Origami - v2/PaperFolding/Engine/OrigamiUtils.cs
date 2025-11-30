using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Origami
{

    // root object
    public class WorkflowRoot
    {
        [JsonProperty("start")]
        public StartObject Start { get; set; }

        [JsonProperty("folds")]
        public List<FoldObject> Folds { get; set; }
    }

    // Start object（id or points）
    public class StartObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }          // 整数ID（可选，与Points互斥）

        [JsonProperty("points")]
        public List<PointF> Points { get; set; } // 点列表（可选，与Id互斥）

        [JsonProperty("otherParam")]
        public string OtherParam { get; set; }
    }

    // Fold object
    public class FoldObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("foldingParams")]
        public List<FoldingParam2> FoldingParams { get; set; }

        [JsonProperty("foldParam")]
        public string FoldParam { get; set; }
    }

    // Folding parameters (polymorphism: point mode or angle mode)
    public abstract class FoldingParam2
    {
        [JsonProperty("lastId")]
        public int LastId { get; set; }        // 公共参数：lastId
    }

    // Point mode parameters
    public class PointFoldingParam : FoldingParam2
    {
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("p1")]
        public PointF P1 { get; set; }

        [JsonProperty("p2")]
        public PointF P2 { get; set; }
    }

    // Angle mode parameters
    public class AngleFoldingParam : FoldingParam2
    {
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("p1")]
        public PointF P1 { get; set; }

        [JsonProperty("alpha")]
        public double Alpha { get; set; }      // 角度（弧度或度数，需根据业务定义）
    }

    public class FoldingParamConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(FoldingParam).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            string mode = jObject["mode"].Value<string>();

            switch (mode)
            {
                case "point":
                    return jObject.ToObject<PointFoldingParam>();
                case "angle":
                    return jObject.ToObject<AngleFoldingParam>();
                default:
                    throw new JsonException("Unknown folding parameter mode: {mode}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public static class WorkflowParser
    {
        // Parse JSON
        public static WorkflowRoot ParseJson(string json)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new FoldingParamConverter() }
            };
            return JsonConvert.DeserializeObject<WorkflowRoot>(json, settings);
        }

        // Serialize to JSON
        public static string SerializeJson(WorkflowRoot root)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new FoldingParamConverter() },
                Formatting = Formatting.Indented
            };
            return JsonConvert.SerializeObject(root, settings);
        }

    
    }
}
