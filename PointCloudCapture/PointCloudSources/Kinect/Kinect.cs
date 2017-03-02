using Microsoft.Kinect;
using OpenTK;
using PointCloudCapture.PointCloudSources.Kinect.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PointCloudCapture.PointCloudSources.Kinect
{
    public class Kinect : IPointCloudSource
    {
        // Constants
        private const int WIDTH = CameraParameters.WIDTH;
        private const int HEIGHT = CameraParameters.HEIGHT;

        private const int MIN_KINECT_VALUE = CameraParameters.MIN_KINECT_VALUE;
        private const int MAX_KINECT_VALUE = CameraParameters.MAX_KINECT_VALUE;

        // Kinect
        private KinectSensor kinectSensor;
        private Thread kinectCallbackThread;

        // Buffers
        private short[] rawDepthFrameBuffer = new short[WIDTH * HEIGHT];
        private Vector3[] realWorldVectorsBuffer;

        // Lock objects
        private object rawDepthFrameBuffer_lock = new object();
        private object realWorldVectorsBuffer_lock = new object();

        // Members
        private List<Vector3> realWorldVectors = new List<Vector3>();
        private int frameDroped = 0;
        private Task postProsessingTask = new Task(() => { });




        /////// Public methods ///////

        /// <summary>
        /// Connects to kinect and creates a callback thread
        /// </summary>
        public void Connect()
        {
            // 1. Collecting available sensors connected to system
            List<KinectSensor> availableSensors = KinectSensor.KinectSensors.Where(sensor => sensor.Status == KinectStatus.Connected).ToList();

            if (availableSensors.Count > 0)
            {
                // 2. Selecting the first available sensor
                this.kinectSensor = availableSensors[0];

                // Configuring depth stream
                this.kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                try
                {
                    // 3. Starting sensor
                    this.kinectSensor.Start();
                }
                catch (IOException e)
                {
                    Console.WriteLine("Unable to start sensor: " + e);
                    this.kinectSensor = null;
                }
            }

            // 4. Creating dedicated thread for callbacks
            if (this.kinectSensor.Status == KinectStatus.Connected)
            {
                this.kinectCallbackThread = new Thread(() => { this.kinectSensor.DepthFrameReady += this.OnSensorDepthFrameReady; });
                this.kinectCallbackThread.IsBackground = true;
                this.kinectCallbackThread.Start();

                Console.WriteLine("Connected to kinect!");
            }
            else
            {
                Console.WriteLine("Failed to connect to kinect");
            }

        }



        /// <summary>
        /// Disconnects from the kinect sensor
        /// </summary>
        public void Disconnect()
        {

            if (this.kinectSensor != null)
            {
                // Unregister events
                this.kinectSensor.DepthFrameReady -= this.OnSensorDepthFrameReady;

                // Stop the kinect sensor
                this.kinectSensor.Stop();
            }

            Console.WriteLine("Kinect stopped!");
        }


        /// <summary>
        /// Returns vectors in real world vector buffer.
        /// </summary>
        /// <returns>Vectors referenced to real world coordinates</returns>
        Vector3[] IPointCloudSource.getVectors()
        {
            lock (realWorldVectorsBuffer_lock)
            {
                return realWorldVectorsBuffer;
            }
        }


        /////// Private methods ///////


        /// <summary>
        /// Post-processing frame, converting raw depth values to vectors
        /// </summary>
        private void PostProcessFrame()
        {
            // 1. Getting the newest raw depth frame and converting to formated depthFrame (0 - 4096)

            short[] rawDepthFrame = getRawDepthFrameBuffer();


            // 2. Looping trough all pixels in frame to convert to real world vectors

            int index;
            int depthValue;
            int max = 0;
            for (int y = 0; y < HEIGHT; y++)
                for (int x = 0; x < WIDTH; x++)
                {
                    // Converting x and y to index in depth frame array
                    index = x + y * WIDTH;
                    depthValue = rawDepthFrame[index] >> 3;

                    // If depth frame pixel is in the valid value range (1 - 4094)
                    if (depthValue > MIN_KINECT_VALUE &&
                        depthValue < MAX_KINECT_VALUE)
                    {
                        Vector3 newVector = new Vector3();

                        // Converting to real world points
                        newVector.X = (x - CameraParameters.CX) * CameraParameters.A * depthValue;
                        newVector.Y = -(y - CameraParameters.CY) * CameraParameters.A * depthValue;
                        newVector.Z = depthValue;

                        realWorldVectors.Add(newVector);
                    }
                };


            // 3. Setting real world vectors buffer and cleaning up

            SetRealWorldVectorsBuffer(realWorldVectors.ToArray());
            realWorldVectors.Clear();

            if(max > 0) Console.WriteLine(max);
        }
        

        /// <summary>
        /// Event handler for every kinect depth frame, 
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnSensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            /// Note: this method is running inside the kinectCallbackThread
            /// And are called every time the kinect API have a new depth frame ready

            // 1. Get depth image frame
            DepthImageFrame newDepthFrame = e.OpenDepthImageFrame();

            if (newDepthFrame != null)
            {
                lock (rawDepthFrameBuffer_lock)
                {
                    // 2. Copy to raw depth frame buffer
                    newDepthFrame.CopyPixelDataTo(rawDepthFrameBuffer);
                }

                if (postProsessingTask.Status != TaskStatus.Running)
                {
                    // 3. Start new post-processing task 
                    postProsessingTask = Task.Factory.StartNew(() => PostProcessFrame());
                }
                else
                {
                    // If post-processing is already running increment frames dropped and ignore current frame
                    frameDroped++;
                }

            }

            newDepthFrame.Dispose();

        }

        /// <summary>
        /// Gets the raw depth data frame in buffer
        /// </summary>
        /// <returns>Raw depth frame</returns>
        private short[] getRawDepthFrameBuffer()
        {
            lock (rawDepthFrameBuffer_lock)
            {
                return this.rawDepthFrameBuffer;
            }
        }

        /// <summary>
        /// Sets the real world vectors buffer
        /// </summary>
        /// <param name="newVectors">Vectors to replace in buffer</param>
        private void SetRealWorldVectorsBuffer(Vector3[] newVectors)
        {
            lock (realWorldVectorsBuffer_lock)
            {
                realWorldVectorsBuffer = newVectors;
            }
        }



    }
}
