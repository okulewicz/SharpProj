using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
namespace SharpProj.Tests
{
    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(GeoJson.Geometry).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }

    public class GeometryConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GeoJson.Geometry);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            switch (jo["type"].Value<string>())
            {
                case "Point":
                    return JsonConvert.DeserializeObject<GeoJson.Point>(jo.ToString(), SpecifiedSubclassConversion);
                case "LineString":
                    return JsonConvert.DeserializeObject<GeoJson.LineString>(jo.ToString(), SpecifiedSubclassConversion);
                case "Polygon":
                    return JsonConvert.DeserializeObject<GeoJson.Polygon>(jo.ToString(), SpecifiedSubclassConversion);
                case "MultiPolygon":
                    return JsonConvert.DeserializeObject<GeoJson.MultiPolygon>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new Exception();
            }
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }
    public class GeoJson
    {
        public const string FeatureCollectionType = "FeatureCollection";
        public const string FeatureType = "Feature";

        public class Feature
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("properties")]
            public Dictionary<string, object> Properties { get; set; }
            [JsonProperty("geometry")]
            public Geometry Geometry { get; set; }
        }

        [JsonConverter(typeof(GeometryConverter))]
        public abstract class Geometry
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            public abstract Point GetPoint();
        }

        public class MultiPolygon : Geometry
        {
            public const string TypeName = "MultiPolygon";
            [JsonProperty("coordinates")]
            public double[][][][] Coordinates { get; set; }

            public override Point GetPoint()
            {
                return new Point()
                {
                    Coordinates = new double[] {
                        Coordinates.Average(c =>  Math.Round(c.Average(c2 => c2.Average(c3 => c3[0])))),
                        Coordinates.Average(c =>  Math.Round(c.Average(c2 => c2.Average(c3 => c3[1])))),
                    },
                    Type = "Point"
                };
            }
        }
        public class Polygon : Geometry
        {
            public const string TypeName = "Polygon";
            [JsonProperty("coordinates")]
            public double[][][] Coordinates { get; set; }
            public override Point GetPoint()
            {
                return new Point()
                {
                    Coordinates = new double[] {
                        Coordinates.Average(c => Math.Round(c.Average(c2 => c2[0]), 6)),
                        Coordinates.Average(c => Math.Round(c.Average(c2 => c2[1]), 6)),
                    },
                    Type = "Point"
                };
            }
        }
        public class LineString : Geometry
        {
            public const string TypeName = "LineString";

            [JsonProperty("coordinates")]
            public double[][] Coordinates { get; set; }
            public override Point GetPoint()
            {
                return new Point()
                {
                    Coordinates = new double[] {
                         Math.Round(Coordinates.Average(c => c[0])),
                         Math.Round(Coordinates.Average(c => c[1])),
                    },
                    Type = "Point"
                };
            }
        }
        public class Point : Geometry
        {
            public const string TypeName = "Point";
            [JsonProperty("coordinates")]
            public double[] Coordinates { get; set; }
            public override Point GetPoint()
            {
                return new Point()
                {
                    Coordinates = new double[] {
                         Math.Round(Coordinates[0], 6),
                         Math.Round(Coordinates[1], 6),
                    },
                    Type = "Point"
                };
            }
        }

        [JsonProperty("features")]
        public Feature[] Features { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }

}
