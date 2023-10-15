using UnityEngine;

/*
 * LookAt:
Character Names or Health Bars in MMOs: In multiplayer games with many active characters, character names or health bars above each player or NPC should always 
face the player for readability, regardless of the character or camera orientation.
World Space UI: When you have a UI element that exists within your 3D world (like a hovering text bubble), you'll want it to always face the camera so that the 
text is easily readable.

LookAtInverted:
Simulating Headlights in Racing Games: If you want to simulate the effect of headlights on a vehicle that always illuminate the road ahead, you could use this mode.
Creating Shadows or Reflections: If you are creating a simplified system for shadows or reflections that need to move with the camera, this could be a good use-case.

CameraForward:
Point Of Interest (POI) Indicators: In open world games, the objective or POI markers that appear at the edge of the screen could use this mode to point towards the objective.
Enemy Indicators: In a stealth game, indicators that point towards off-screen enemies can use this mode to point in the direction of the enemies.

CameraForwardInverted:
Retreating Indicators: In a game where the player must retreat from threats, an indicator could use this mode to show the fastest escape direction, 
essentially pointing away from the camera's forward vector.
Slingshot or Swing Mechanic: If you have a game mechanic where a character or object is launched in the opposite direction (like a slingshot or swing), 
this mode could be useful to show the potential trajectory or launch direction.

 Billboarding:
Interactive UI Elements: In a 3D menu or interactive scene, the UI elements could use this mode to always face the player but maintain their upright orientation, 
improving readability.
Enemy or NPC Health Bars: Similar to the MMO example above, but the billboarding ensures that the health bars are always upright and readable, 
even when the camera is above or below the horizon line.
*/

public class LookAtCamera : MonoBehaviour
{
    // The Mode enumeration defines the different ways an object can look towards the camera.
    private enum Mode
    {
        Billboarding,  //don't look up and down, just rotate around the y axis
        LockAt, // Looks directly at the camera.
        LookAtInverted, // Faces the same direction the camera is facing.
        CameraForward, // Points in the same direction the camera is pointing, regardless of its location.
        CameraForwardInverted // Faces the opposite direction of where the camera is facing.
    }
    [SerializeField] private Mode mode; // Stores the chosen mode from the Unity editor.

    // LateUpdate is called after all Update functions have been called.
    // This is useful to ensure that the camera has already moved this frame before updating the object's rotation.
    private void LateUpdate()
    {
        switch (mode)
        {
            case Mode.Billboarding:
                //The direction vector from the object to the camera, which is the normal look direction 
                Vector3 direction = (Camera.main.transform.position - transform.position).normalized;
                // Remove the y from the vector to prevent the UI element from tilting
                direction.y = 0;
                // Make the object face towards the camera while maintaining its upright orientation.
                transform.rotation = Quaternion.LookRotation(-direction);
                break;
            case Mode.LockAt:
                // Make the object face towards the camera.
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.LookAtInverted:
                // Make the object face the same direction the camera is facing.
                Vector3 vectorFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + vectorFromCamera);
                break;
            case Mode.CameraForward:
                // Make the object point in the same direction the camera is pointing.
                transform.forward = Camera.main.transform.forward;
                break;
            case Mode.CameraForwardInverted:
                // Make the object always face the opposite direction of where the camera is facing.
                transform.forward = -Camera.main.transform.forward;
                break;
        }
    }
}

