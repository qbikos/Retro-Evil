namespace HorrorEngine
{
    public static class MathUtils
    {
        public static float Map(float x, float minFrom, float maxFrom, float minTo, float maxTo)
        {
            var m = (maxTo - minTo) / (maxFrom - minFrom);
            var c = minTo - m * minFrom; // point of interest: c is also equal to y2 - m * x2, though float math might lead to slightly different results.

            return m * x + c;
        }

        public static float Wrap(float x, float min, float max)
        {
            if (x < min)
                x = max - (min - x) % (max - min);
            else
                x = min + (x - min) % (max - min);

            return x;
        }
    }
}
