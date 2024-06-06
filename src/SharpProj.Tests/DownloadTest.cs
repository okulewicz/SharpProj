using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GML2;
using Newtonsoft.Json;

namespace SharpProj.Tests
{
    [TestClass]
    public class DownloadTest
    {
        [XmlRoot(ElementName = "Envelope", Namespace = "http://www.opengis.net/gml/3.2")]
        public class Envelope
        {

            [XmlElement(ElementName = "lowerCorner", Namespace = "http://www.opengis.net/gml/3.2")]
            public string LowerCorner { get; set; }

            [XmlElement(ElementName = "upperCorner", Namespace = "http://www.opengis.net/gml/3.2")]
            public string UpperCorner { get; set; }

            [XmlAttribute(AttributeName = "srsName", Namespace = "")]
            public string SrsName { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "boundedBy", Namespace = "http://www.opengis.net/wfs/2.0")]
        public class BoundedBy
        {

            [XmlElement(ElementName = "Envelope", Namespace = "http://www.opengis.net/gml/3.2")]
            public Envelope Envelope { get; set; }
        }

        [XmlRoot(ElementName = "Point", Namespace = "http://www.opengis.net/gml/3.2")]
        public class Point
        {

            [XmlElement(ElementName = "pos", Namespace = "http://www.opengis.net/gml/3.2")]
            public string Pos { get; set; }

            [XmlAttribute(AttributeName = "id", Namespace = "http://www.opengis.net/gml/3.2")]
            public double Id { get; set; }

            [XmlAttribute(AttributeName = "srsName", Namespace = "")]
            public string SrsName { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "msGeometry", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
        public class MsGeometry
        {

            [XmlElement(ElementName = "Point", Namespace = "http://www.opengis.net/gml/3.2")]
            public Point Point { get; set; }
        }

        [XmlRoot(ElementName = "prg-adresy", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
        public class Prgadresy
        {

            [XmlElement(ElementName = "boundedBy", Namespace = "http://www.opengis.net/gml/3.2")]
            public BoundedBy BoundedBy { get; set; }

            [XmlElement(ElementName = "msGeometry", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public MsGeometry MsGeometry { get; set; }

            [XmlElement(ElementName = "miejscowosc", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public string Miejscowosc { get; set; }

            [XmlElement(ElementName = "ulica", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public string Ulica { get; set; }

            [XmlElement(ElementName = "numer", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public string Numer { get; set; }

            [XmlElement(ElementName = "teryt", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public string Teryt { get; set; }

            [XmlElement(ElementName = "simc", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public string Simc { get; set; }

            [XmlElement(ElementName = "ulic", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public string Ulic { get; set; }

            [XmlElement(ElementName = "kod", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public string Kod { get; set; }
        }

        [XmlRoot(ElementName = "member", Namespace = "http://www.opengis.net/wfs/2.0")]
        public class Member
        {

            [XmlElement(ElementName = "prg-adresy", Namespace = "http://mapserver.gis.umn.edu/mapserver")]
            public Prgadresy Prgadresy { get; set; }
        }

        [XmlRoot(ElementName = "FeatureCollection", Namespace = "http://www.opengis.net/wfs/2.0")]
        public class FeatureCollection
        {

            [XmlElement(ElementName = "boundedBy", Namespace = "http://www.opengis.net/wfs/2.0")]
            public BoundedBy BoundedBy { get; set; }

            [XmlElement(ElementName = "member", Namespace = "http://www.opengis.net/wfs/2.0")]
            public List<Member> Member { get; set; }

            [XmlAttribute(AttributeName = "ms", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Ms { get; set; }

            [XmlAttribute(AttributeName = "gml", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Gml { get; set; }

            [XmlAttribute(AttributeName = "wfs", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Wfs { get; set; }

            [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Xsi { get; set; }

            [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
            public string SchemaLocation { get; set; }

            [XmlAttribute(AttributeName = "timeStamp", Namespace = "")]
            public DateTime TimeStamp { get; set; }

            [XmlAttribute(AttributeName = "numberMatched", Namespace = "")]
            public int NumberMatched { get; set; }

            [XmlAttribute(AttributeName = "numberReturned", Namespace = "")]
            public int NumberReturned { get; set; }

            [XmlText]
            public string Text { get; set; }
        }
        [TestMethod]
        public void Download()
        {
            var pl1992 = CoordinateReferenceSystem.CreateFromEpsg(2180);
            var wgs84 = CoordinateReferenceSystem.CreateFromEpsg(4326);

            var area = pl1992.UsageArea;
            Console.WriteLine(area.Name);

            using (var t = CoordinateTransform.Create(pl1992, wgs84))
            {
                var r = t.Apply(new PPoint(621000, 305000));
                Console.WriteLine($"{Math.Round(r.X,6)} {Math.Round(r.Y, 6)}");
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://mapy.geoportal.gov.pl/wss/ext/KrajowaIntegracjaNumeracjiAdresowej");
            client.Timeout = new TimeSpan(1, 0, 0);
            //var getCapabilitiesTask = client.GetAsync("?service=wfs&request=GetCapabilities&version=2.0.0");
            //getCapabilitiesTask.Wait();
            //Console.WriteLine(getCapabilitiesTask.Result.ToString());
            int step = 80000;
            for (int x = 781000; x > 115613; x -= step)
            {
                for (int y = 145000; y < 898943; y += step)
                {
                    GetData(pl1992, wgs84, client, step, x, y);
                }
            }
            

            //780623 145011
            //115613 898943

        }

        private static void GetData(CoordinateReferenceSystem pl1992, CoordinateReferenceSystem wgs84, HttpClient client, int step, int x, int y)
        {
            try
            {
                if (File.Exists($"adresy-{x}-{y}.json"))
                {
                    return;
                }
                var getFeaturesTask = client.GetAsync(string.Format(@"?service=wfs&request=GetFeature&version=2.0.0&srsname=urn:ogc:def:crs:EPSG::2180&typenames=ms:prg-adresy&filter=<fes:Filter>
                <fes:Intersects>
                    <fes:ValueReference>ms:msGeometry</fes:ValueReference>
                    <gml:Polygon srsName=""urn:ogc:def:crs:EPSG::2180"" gml:id=""P1"">
                        <gml:exterior>
                            <gml:LinearRing>
                                <gml:posList>{0} {1} {2} {1} {2} {3} {0} {3} {0} {1}</gml:posList>
                            </gml:LinearRing>
                        </gml:exterior>
                        </gml:Polygon>
                        </fes:Intersects>
                    </fes:Filter>&startindex=0", x, y, x - step, y + step));
                getFeaturesTask.Wait();
                GeometryFactory geometryFactory = new GeometryFactory();

                var contentStringTask = getFeaturesTask.Result.Content.ReadAsStringAsync();
                contentStringTask.Wait();
                var gmlReader = new GMLReader(geometryFactory);
                var xmlReader = new System.Xml.XmlTextReader(new StringReader(contentStringTask.Result));

                var transformerPlToWgs = CoordinateTransform.Create(pl1992, wgs84);

                XmlSerializer serializer = new XmlSerializer(typeof(FeatureCollection));
                using (StringReader reader = new StringReader(contentStringTask.Result))
                {
                    var geoJson = new GeoJson();
                    geoJson.Type = GeoJson.FeatureCollectionType;
                    var test = (FeatureCollection)serializer.Deserialize(reader);
                    Console.WriteLine(test.Member.Count);
                    if (test.Member.Count > 0)
                    {
                        Console.WriteLine(test.Member[0].Prgadresy.Ulica);
                        Console.WriteLine(test.Member[0].Prgadresy.MsGeometry.Point.Pos);
                        geoJson.Features = test.Member.Select(m =>
                        {
                            var transformedPoint = transformerPlToWgs.Apply(
                                m.Prgadresy.MsGeometry.Point.Pos.Split(' ')
                                .Select(p => double.Parse(p, CultureInfo.InvariantCulture)).ToArray());

                            return new GeoJson.Feature()
                            {
                                Type = GeoJson.FeatureType,
                                Geometry = new GeoJson.Point()
                                {
                                    Coordinates = new double[] { Math.Round(transformedPoint[1], 6), Math.Round(transformedPoint[0], 6) },
                                    Type = GeoJson.Point.TypeName
                                },
                                Properties = new Dictionary<string, object>()
                                    {
                                    { "kod", m.Prgadresy.Kod },
                                    { "miejscowosc", m.Prgadresy.Miejscowosc },
                                    { "numer", m.Prgadresy.Numer },
                                    { "simc", m.Prgadresy.Simc },
                                    { "teryt", m.Prgadresy.Teryt },
                                    { "ulic", m.Prgadresy.Ulic },
                                    { "ulica", m.Prgadresy.Ulica }
                                    }
                            };
                        }).ToArray();
                        File.WriteAllText($"adresy-{x}-{y}.json", JsonConvert.SerializeObject(geoJson));
                    }

                }
            }
            catch (Exception ex)
            {
                GetData(pl1992, wgs84, client, step / 2, x, y);
                GetData(pl1992, wgs84, client, step / 2, x - (step / 2), y);
                GetData(pl1992, wgs84, client, step / 2, x, y + (step / 2));
                GetData(pl1992, wgs84, client, step / 2, x - (step / 2), y + (step / 2));
            }
        }
    }
}
