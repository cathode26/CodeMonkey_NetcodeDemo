using System.Collections.Generic;
using UnityEngine;

public class LatencyAverage : MonoBehaviour
{
    public static LatencyAverage Instance { get; private set; }
    private LinkedList<float> values = new LinkedList<float>();
    private int maxSize = 10;
    private float sum = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of LatencyAverage detected! Destroying...");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    private void OnDestroy()
    {
        Instance = null;
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
