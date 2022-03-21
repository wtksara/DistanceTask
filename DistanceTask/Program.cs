using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DistanceTask
{
    public class Distance
    {
        // Coordinates of the place
        public struct Coordinates
        {
            public double Longitude;
            public double Latitude;
        }

        // Calculate the distance between two postcodes
        public static double? DistanceBetweenTwoPostCodes(string postcodeA, string postcodeB)
        { 
            // Change postcode to the coordinates
            var coordinatesA = ChangeFromPostCodeToCoordinates(postcodeA);
            if (!coordinatesA.HasValue) return null;
            var coordinatesB = ChangeFromPostCodeToCoordinates(postcodeB);
            if (!coordinatesB.HasValue) return null;

            // Calculate the distance
            return DistanceInMiles(coordinatesA.Value, coordinatesB.Value);
        }

        public static Coordinates? ChangeFromPostCodeToCoordinates(string postcode)
        {
            var encodedPostCode = String.Concat(postcode.Where(c => !Char.IsWhiteSpace(c)));
            // Download the JSON file with details about that postcode
            var url = string.Format("http://api.postcodes.io/postcodes/{0}", encodedPostCode);
            using (WebClient wc = new WebClient())
            {
                try
                {
                    var json = wc.DownloadString(url);
                    var data = (JObject)JsonConvert.DeserializeObject(json);

                    return new Coordinates
                        {
                            // Get the data from JSON
                            Longitude = data["result"]["longitude"].Value<double>(),
                            Latitude = data["result"]["latitude"].Value<double>()
                        };
                   
                }
                catch {
                    return null;
                }
            }    
        }
        
        // Calculate the distance in miles
        public static double DistanceInMiles(Coordinates from, Coordinates to)
        {
            // Using of the Haversine Formula
            var dLat1InRad = from.Latitude * (Math.PI / 180.0);
            var dLong1InRad = from.Longitude * (Math.PI / 180.0);
            var dLat2InRad = to.Latitude * (Math.PI / 180.0);
            var dLong2InRad = to.Longitude * (Math.PI / 180.0);

            var dLongitude = dLong2InRad - dLong1InRad;
            var dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a 
            var a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                    Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                    Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c
            var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            // Measurement in miles
            var radius = 3959;

            return radius * c;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var distance = Distance.DistanceBetweenTwoPostCodes("BS1 6Q", "B1 2HL");
            if (distance.HasValue)
            {
                Console.WriteLine(distance.Value + " miles"); 
                System.IO.File.AppendAllText(@"c:\log.txt", "Success of  the calculation of the distance. Distance: " + distance.Value + "\n");
            }
            else
            {
                Console.WriteLine("Incorrect postcode");
                System.IO.File.AppendAllText(@"c:\log.txt", "Calculation of the distance failed \n");
            }
        }
    }
}