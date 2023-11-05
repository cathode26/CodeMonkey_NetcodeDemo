using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The PlatesCounterVisual class handles the visual aspect of the plates on the countertop.
public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;
    [SerializeField] private float plateOffset = 0.1f;

    // A list of game objects that represent the plates on the countertop.
    private List<GameObject> plateVisualGameObjectList = new List<GameObject>();

    // Subscribe to the plate-related events.
    private void OnEnable()
    {
        platesCounter.OnPlateVisualSpawned += PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateVisualConsumed += PlatesCounter_OnPlateConsumed;
    }

    // Unsubscribe from the plate-related events.
    private void OnDisable()
    {
        platesCounter.OnPlateVisualSpawned -= PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateVisualConsumed -= PlatesCounter_OnPlateConsumed;
    }

    // This method is invoked when a plate is spawned.
    // It creates a visual representation of a plate at a specific position.
    private void PlatesCounter_OnPlateSpawned(object sender, System.EventArgs e)
    {
        Transform plate = Instantiate(plateVisualPrefab, counterTopPoint);
        plate.transform.localPosition = Vector3.zero;
        plate.transform.localPosition = new Vector3(0, plateOffset * plateVisualGameObjectList.Count, 0);
        plateVisualGameObjectList.Add(plate.gameObject);
    }

    // This method is invoked when a plate is consumed.
    // It destroys the visual representation of the last spawned plate.
    private void PlatesCounter_OnPlateConsumed(object sender, System.EventArgs e)
    {
        GameObject plate = plateVisualGameObjectList.Last();
        plateVisualGameObjectList.Remove(plate);
        Destroy(plate);
    }
}
