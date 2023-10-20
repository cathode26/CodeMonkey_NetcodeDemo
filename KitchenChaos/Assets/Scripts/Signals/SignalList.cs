using UnityEngine;

namespace CheatSignalList
{
    public class EnableMovementSpeedCheatSignal : Signal<float> { }
    public class DisableMovementSpeedCheatSignal : Signal { }
}

namespace GameSignalList
{
    public class OnPlayerSpawnedSignal : Signal<Player> { }
    public class OnPlayerDespawnedSignal : Signal<Player> { }
    public class OnSelectedBaseCounterChangedSignal : Signal<BaseCounter> { }
    public class SetAudioListenerSignal : Signal<Transform> { }
    public class ReturnAudioListenerSignal : Signal { }
    public class OnRecipeSuccessSignal : Signal { }
    public class OnRecipeFailedSignal : Signal { }
}

namespace SoundSignalList
{
    public class OnChangeSoundEffectVolumeSignal : Signal { }
    public class OnSoundEffectsVolumeChangedSignal : Signal<float> { }
    public class OnChangeMusicVolumeSignal : Signal { }
    public class OnMusicVolumeChangedSignal : Signal<float> { }
    public class OnRecipeSuccessSignal : Signal<Vector3> { }
    public class OnRecipeFailedSignal : Signal<Vector3> { }
    public class OnChoppedSignal : Signal<Vector3> { }
    public class OnObjectPickupSignal : Signal<Vector3> { }
    public class OnObjectDropSignal : Signal<Vector3> { }
    public class OnAnyObjectTrashedSignal : Signal<Vector3> { }
    public class OnFootStepsSignal : Signal<Vector3> { }
    public class OnWarningSignal : Signal<Vector3> { }
    public class OnCountdownSignal : Signal { }
}
