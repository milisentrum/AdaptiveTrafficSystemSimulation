using System.Linq;
using AdaptiveTrafficSystem.Net.Websocket.Messages;
using AdaptiveTrafficSystem.Tracking.Parameters;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

namespace AdaptiveTrafficSystem.Net.Websocket.Behaviors
{
    public class TrafficParametersApiBehavior : ExtendedWebSocketBehavior
    {
        public ParametersHolder[] ParametersHolder;

        private const string TRAFFIC_INFO_TYPE = "TrafficInfo";

        protected override void OnMessage(MessageEventArgs e)
        {
            var type = JsonConvert.DeserializeObject<Message>(e.Data).Type;
            Debug.Log($"Msg ({type}): {e.Data}");
            
            switch (type)
            {
                case TRAFFIC_INFO_TYPE:
                    SendParams();
                    break;
            }
        }

        private void SendParams()
        {
            var data = ParametersHolder
                .Select(holder => new DirectionInfo
                {
                    Direction = new Direction
                    {
                        ID = holder.PathDirection.Id,
                        PathId = holder.PathDirection.Path.Id
                    },
                    Parameters = new DirectionParameters
                    {
                        Phase = holder.PathDirection.Path.TrafficGroup.OpenPathTime.GetValue(),
                        Intensity = holder.IntensityParameter.GetValue(),
                        AvgSpeed = holder.AvgSpeedParameter.GetValue(),
                        MinSpeed = holder.MinSpeedParameter.GetValue(),
                        MaxSpeed = holder.MaxSpeedParameter.GetValue(),
                        AvgWaitingTime = holder.AvgWaitingTimeParameter.GetValue(),
                        MinWaitingTime = holder.MinWaitingTimeParameter.GetValue(),
                        MaxWaitingTime = holder.MaxWaitingTimeParameter.GetValue(),
                        WaitingVehicles = holder.VehicleCountParameter.GetValue(),
                        ServiceIntensity = holder.ServiceIntensityParameter.GetDirectionValue(holder.PathDirection),
                        WaitingPedestriansCount = holder.WaitingPedestriansCountParameter.GetValue(),
                        CrossingPedestriansCount = holder.CrossingPedestriansCountParameter.GetValue()
                    }
                })
                .ToArray();
            var msg = new TrafficParametersDataPack
            {
                Type = TRAFFIC_INFO_TYPE,
                Data = data
            };
            var json = JsonConvert.SerializeObject(msg);
            SendAsync(json);
        }
    }
}