using System.Collections.Generic;

public class LatencyAverage
{
    private LinkedList<float> values;
    private int maxSize;
    private float sum;

    public LatencyAverage(int size)
    {
        values = new LinkedList<float>();
        maxSize = size;
        sum = 0;
    }

    public void AddValue(float value)
    {
        // Add the new value to the end (back) of the LinkedList
        values.AddLast(value);
        sum += value;

        // If the LinkedList size exceeds the maximum size, remove the first (front) value
        if (values.Count > maxSize)
        {
            sum -= values.First.Value;
            values.RemoveFirst();
        }
    }

    public float GetAverage()
    {
        if (values.Count == 0) return 0; // Avoid division by zero
        return sum / values.Count;
    }
}
