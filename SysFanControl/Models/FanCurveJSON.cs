using Newtonsoft.Json.Linq;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysFanControl.Models
{
    public static class FanCurveJSON
    {
        public static JObject Serialise(FanCurve fanCurve)
        {
            var points = new List<FanCurvePoint>();
            foreach (var point in fanCurve.Points)
            {
                points.Add(new FanCurvePoint { Value = point.Value, Percent = point.Percent });
            }

            var obj = new JObject
            {
                { "enabled", fanCurve.Enabled },
                { "points", JToken.FromObject(points) },
            };
            if (fanCurve.Source != null)
            {
                obj["sensor"] = fanCurve.Source.Sensor.Identifier.ToString();
            }

            return obj;
        }

        public static void UpdateFromJSON(JObject json, FanCurve fanCurve, List<IHardware> hardware)
        {
             fanCurve.Enabled = json["enabled"].ToObject<bool>();

            fanCurve.Points.Clear();
            foreach (var point in ((JArray)json["points"]).ToObject<List<FanCurvePoint>>())
            {
                fanCurve.AddPoint(point);
            }

            if (json.ContainsKey("sensor"))
            {
                var sensorId = json["sensor"].ToString();

                var hardwareResult = hardware.Where(hw => sensorId.Contains(hw.Identifier.ToString()));
                if (hardwareResult.Count() > 0)
                {
                    var sensorResult = hardwareResult.First().Sensors.Where(s => s.Identifier.ToString() == sensorId);
                    if (sensorResult.Count() > 0)
                    {
                        fanCurve.Source = new FanCurveSource(sensorResult.First());
                    }
                }
            }
        }
    }
}
