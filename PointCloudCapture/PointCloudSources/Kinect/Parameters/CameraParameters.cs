using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudCapture.PointCloudSources.Kinect.Parameters
{
    /// <summary>
    /// Camera parameter unique for the Kinect V1
    /// </summary>
    struct CameraParameters
    {
        public const int WIDTH = 640;
        public const int HEIGHT = 480;

        public const float CX = WIDTH / 2;
        public const float CY = HEIGHT / 2;
        public const float A = 0.00173667f;

        public const int MIN_KINECT_VALUE = 0;
        public const int MAX_KINECT_VALUE = 4095;
    }
}
