using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelativePositioning;

namespace Sunrise.Features.ServersideBacktrack
{
    public class RelativeRotation
    {
        public Quaternion Rotation { get; }
        public byte WaypointId { get; }

        public RelativeRotation(Quaternion rotation, byte waypointId)
        {
            Rotation = rotation;
            WaypointId = waypointId;
        }

        public Quaternion ToWorldRotation()
        {
            return WaypointBase.GetWorldRotation(WaypointId, Rotation).normalized;
        }
    }
}
