using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace PointCloudCapture.PointCloudSources.Dummies
{
    class Dummy : IPointCloudSource
    {
        private const int WIDTH = 640;
        private const int HEIGHT = 480;

        Vector3[] myVector = new Vector3[WIDTH * HEIGHT];


        public Dummy() {
            for (int y = 0; y < HEIGHT; y++) {
                for (int x = 0; x < WIDTH; x++) {
                    int index = x + y * WIDTH;
                    myVector[index] = new Vector3(x, y, 0);
                }
            }

        }

        Vector3[] IPointCloudSource.getVectors()
        {
            return myVector;
        }
    }
}
