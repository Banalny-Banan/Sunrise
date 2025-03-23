using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sunrise.Features.ServersideBacktrack;

namespace Sunrise.Utility
{
    public static class QuaternionExtensions
    {
        public static RelativeRotation GetClientRotation(this Player player)
        {
            if (ServersideBacktrackModule.LastClientsRotation.TryGetValue(player, out var relativeRot))
                return relativeRot;

            return null;
        }

        public static Vector3 GetDeltaRotationTo(this Quaternion from, Quaternion to)
        {
            var deltaRot = (Quaternion.Inverse(to) * from).normalized;
            var euler = deltaRot.eulerAngles;

            var x = Mathf.DeltaAngle(0, euler.x);
            var y = Mathf.DeltaAngle(0, euler.y);
            var z = Mathf.DeltaAngle(0, euler.z);

            return new Vector3(x, y, z);
        }

        public static Vector2 AsMouseMove(this Vector3 deltaRotation)
        {
            return new Vector2(-deltaRotation.y, deltaRotation.x);
        }
    }
}
