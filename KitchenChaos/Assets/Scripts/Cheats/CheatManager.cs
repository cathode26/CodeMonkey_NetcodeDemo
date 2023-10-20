using QFSW.QC;
using UnityEngine;
using CheatSignalList;

public class CheatManager : MonoBehaviour
{
    [Command("EnableMovementSpeedCheat", MonoTargetType.Single)]
    public void EnableMovementSpeedCheat(float speedMultiplier)
    {
        Signals.Get<EnableMovementSpeedCheatSignal>().Dispatch(speedMultiplier);
    }
    [Command("DisableMovementSpeedCheat")]
    public void DisableMovementSpeedCheat()
    {
        Signals.Get<DisableMovementSpeedCheatSignal>().Dispatch();
    }
}
