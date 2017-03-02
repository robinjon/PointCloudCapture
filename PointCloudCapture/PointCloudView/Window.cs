using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using PointCloudCapture.PointCloudSources;
using System.Linq;

namespace PointCloudCapture.PointCloudView
{
    partial class Window : GameWindow
    {

        private IPointCloudSource pointCloudSource;

        private float modelScalingFactor = 1f / 100;

        private Vector3 modelScaleVector;
        private Vector3 modelTranselationVector;

        private float cameraViewAngleY = 0; // angle in radians around the Y-axis
        private float cameraViewAngleX = 0; // angle in radians around the Y-axis
        private float cameraViewRadius = 40; // radius from origin to camera
        public float CameraViewRadius
        {
            get { return cameraViewRadius; }
            private set
            {
                if (value > MIN_RADIUS && value < MAX_RADIUS)
                {
                    cameraViewRadius = value;
                }
            }
        }

        private Vector3 cameraUp;
        private Vector3 cameraPossition;

        private const float minCameraViewRadius = 20;
        private const float maxCameraViewRadius = 60;

        ////// Public Methods //////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="windowTitle">Title of the window</param>
        public Window(string windowTitle)
                : base(800, 600, GraphicsMode.Default, windowTitle)
        {
            VSync = VSyncMode.Off;
        }

        /// <summary>
        /// Sets point cloud source
        /// </summary>
        /// <param name="pointCloudSource">Point cloud source</param>
        public void SetPointCloudSource(IPointCloudSource pointCloudSource) {

            this.pointCloudSource = pointCloudSource;
        }


        ////// Protected Methods //////

        /// <summary>
        /// Initializes class
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initializing scale and translation vectors
            modelScaleVector = new Vector3(modelScalingFactor, modelScalingFactor, modelScalingFactor);
            modelTranselationVector = new Vector3(0, 0, -30);

            // Set background to black
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);

            // Register input events from Mouse and Keyboard
            RegisterInputEvents();

        }

       
        /// <summary>
        /// Event handler called when this is closing
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Register input events from Mouse and Keyboard
            UnregisterInputEvents();
        }

        /// <summary>
        /// Event handler called when this is resized
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Recalculate view port from new window size
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            // Recalculate projection and applies them
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 128.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

        }

        /// <summary>
        /// Creates a new frame and draws it
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // 1. Clear frame
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // 2. Setup Frame
            Matrix4 modelview = getCameraTransformationMatrix();
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            // 3. Receive vectors from point cloud source
            Vector3[] vectors = pointCloudSource.getVectors();


            // 4. Drawing point cloud data
            if (vectors != null)
            {

                // Setting vertex color
                GL.Color3(Color.Gray);

                // Start drawing model
                GL.Begin(PrimitiveType.Points);
                
                // Drawing all vectors
                foreach(var vector in vectors)
                {
                    GL.Vertex3(modelTranselationVector + modelScaleVector * vector);
                }

                GL.End();

            }

            SwapBuffers();
        }

        /// <summary>
        /// Creates a 4x4 Matrix for transformation of camera position
        /// </summary>
        /// <returns>4x4 Matrix</returns>
        private Matrix4 getCameraTransformationMatrix()
        {

            // Calculate camera up vector
            cameraUp = Matrix3.CreateRotationY(cameraViewAngleY) * Matrix3.CreateRotationX(cameraViewAngleX) * Vector3.UnitY;

            // Calculate camera position in space
            cameraPossition = Matrix3.CreateRotationY(cameraViewAngleY) * Matrix3.CreateRotationX(cameraViewAngleX) * new Vector3(0, 0, -cameraViewRadius);

            return Matrix4.LookAt(cameraPossition, Vector3.Zero, cameraUp);

        }
    }
}
