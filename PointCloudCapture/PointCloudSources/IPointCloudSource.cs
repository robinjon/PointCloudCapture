using OpenTK;


namespace PointCloudCapture.PointCloudSources
{
    /// <summary>
    /// Gives a interface for all point cloud objects
    /// </summary>
    interface IPointCloudSource
    {
        Vector3[] getVectors();
    }
}
