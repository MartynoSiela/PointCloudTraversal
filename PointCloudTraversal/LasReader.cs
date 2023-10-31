using laszip.net;

namespace PointCloudTraversal
{
    internal class LasReader
    {
        internal static (float, float, float)?[] ReadPointCloud(string path)
        {
            var lazReader = new laszip_dll();
            var compressed = true;
            lazReader.laszip_open_reader(path, ref compressed);

            var pointsCount = lazReader.header.number_of_point_records;
            var coordinates = new double[3];

            (float, float, float)?[] points = new (float, float, float)?[pointsCount];

            for (int i = 0; i < pointsCount; i += 1)
            {
                lazReader.laszip_read_point();
                lazReader.laszip_get_coordinates(coordinates);
                points[i] = ((float)coordinates[0], (float)coordinates[1], (float)coordinates[2]);
            }

            lazReader.laszip_close_reader();

            return points;
        }

        internal static float[] ReadPointCloudBounds(string path)
        {
            var lazReader = new laszip_dll();
            var compressed = true;
            lazReader.laszip_open_reader(path, ref compressed);

            float[] bounds = new float[6];

            bounds[0] = (float)lazReader.header.min_x;
            bounds[1] = (float)lazReader.header.max_x;
            bounds[2] = (float)lazReader.header.min_y;
            bounds[3] = (float)lazReader.header.max_y;
            bounds[4] = (float)lazReader.header.min_z;
            bounds[5] = (float)lazReader.header.max_z;

            lazReader.laszip_close_reader();

            return bounds;
        }
    }
}
