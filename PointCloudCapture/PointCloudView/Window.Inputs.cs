using OpenTK;
using OpenTK.Input;
using System;


namespace PointCloudCapture.PointCloudView
{
    /// <summary>
    /// Controls user-inputs collected from the openGL window
    /// </summary>
    partial class Window : GameWindow
    {
        // Mouse sensitivity
        private const float MOUSE_SENSITIVITY = 0.01f;
        private const float MOUSE_WHEEL_SENSITIVITY = 1f;

        private const float MIN_RADIUS = 20;
        private const float MAX_RADIUS = 60;


        /// <summary>
        /// Registers input events for window
        /// </summary>
        private void RegisterInputEvents()
        {
            this.Keyboard.KeyDown += OnKeyDown;
            this.Mouse.WheelChanged += OnMouseWheelChanged;
            this.Mouse.Move += OnMouseMove;
        }


        /// <summary>
        /// Unregisters input events for window
        /// </summary>
        private void UnregisterInputEvents()
        {
            this.Keyboard.KeyDown -= OnKeyDown;
            this.Mouse.WheelChanged -= OnMouseWheelChanged;
            this.Mouse.Move -= OnMouseMove;
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (e.Mouse.LeftButton == ButtonState.Pressed)
            {
                cameraViewAngleY += e.XDelta * MOUSE_SENSITIVITY;
                cameraViewAngleX -= e.YDelta * MOUSE_SENSITIVITY;
            }
        }



        private void OnMouseWheelChanged(object sender, MouseWheelEventArgs e)
        {
            // Changing distance from origin
            CameraViewRadius -= e.DeltaPrecise * MOUSE_WHEEL_SENSITIVITY;

        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    cameraViewAngleX -= 0.1f;
                    break;
                case Key.Down:
                    cameraViewAngleX += 0.1f;
                    break;
                case Key.Left:
                    cameraViewAngleY += 0.1f;
                    break;
                case Key.Right:
                    cameraViewAngleY -= 0.1f;
                    break;
                case Key.R:
                    cameraViewAngleY = 0;
                    cameraViewAngleX = 0;
                    cameraViewRadius = 40;
                    break;
                case Key.T:
                    cameraViewAngleY = 0;
                    cameraViewAngleX = (float)-Math.PI / 2;
                    cameraViewRadius = 40;
                    break;
            }
        }
    }
}
