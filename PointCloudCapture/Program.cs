using System;
using PointCloudCapture.PointCloudView;
using PointCloudCapture.PointCloudSources.Kinect;
using System.Threading;
using PointCloudCapture.PointCloudSources.Dummies;

namespace PointCloudCapture
{
    class Program
    {
        private const string programTitle = "Point Cloud Capture";

        static void Main(string[] args)
        {
            // Startup
            
            // 1. Initialize and start Kinect
            Kinect kinect = new Kinect();

            kinect.Connect();

            //Dummy dummy = new Dummy();

            //2.Start openTK window
            using (Window game = new Window(programTitle))
            {
                game.SetPointCloudSource(kinect);
                game.Run(60);
            }


            // Cleanup

            kinect.Disconnect();

        }
    }
}
