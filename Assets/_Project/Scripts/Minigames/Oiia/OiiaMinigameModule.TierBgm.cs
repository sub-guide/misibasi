using UnityEngine;

namespace MiniParty.Minigames.Oiia
{
    public sealed partial class OiiaMinigameModule
    {
        void EnsureTierBgmAudioSource()
        {
            if (_tierBgmRuntime != null)
                return;

            if (tierBgmSource != null)
            {
                _tierBgmRuntime = tierBgmSource;
            }
            else
            {
                AudioSource[] arr = GetComponents<AudioSource>();
                if (arr.Length >= 2)
                    _tierBgmRuntime = arr[1];
                else
                    _tierBgmRuntime = gameObject.AddComponent<AudioSource>();
            }

            _tierBgmRuntime.playOnAwake = false;
            _tierBgmRuntime.loop = true;
            _tierBgmRuntime.spatialBlend = 0f;
            _tierBgmRuntime.volume = tierBgmVolume;
            _tierBgmRuntime.pitch = 1f;
        }

        void StopTierBgm()
        {
            AudioSource s = tierBgmSource != null ? tierBgmSource : _tierBgmRuntime;
            if (s == null)
                return;

            s.Stop();
            s.clip = null;
            s.pitch = 1f;
        }

        bool TryGetMaintainingMaxConsecutiveLoops(out int maxLoops)
        {
            maxLoops = 0;

            if (_ctx.IsPractice)
                return false;

            bool found = false;
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_aliveMask[i])
                    continue;

                ref SlotRuntime sr = ref _slots[i];
                if (!MaintainingGameplayGauge(ref sr))
                    continue;

                found = true;
                if (sr.ConsecutiveLoopSuccesses > maxLoops)
                    maxLoops = sr.ConsecutiveLoopSuccesses;
            }

            return found;
        }

        AudioClip ResolveTierBgmClip()
        {
            if (!TryGetMaintainingMaxConsecutiveLoops(out int maxLoops))
                return null;

            if (maxLoops >= 3 && tier3BeatLoop != null)
                return tier3BeatLoop;

            if (tier2DrumLoop != null)
                return tier2DrumLoop;

            return null;
        }

        float ResolveTierBgmPitch(int maxLoops, AudioClip clip)
        {
            if (clip == tier2DrumLoop && maxLoops >= 2 && maxLoops < 3)
                return tier2BgmPitchScale;

            return 1f;
        }

        void UpdateTierBgm()
        {
            if (!_running || _ctx.IsPractice)
            {
                StopTierBgm();
                return;
            }

            if (!TryGetMaintainingMaxConsecutiveLoops(out int maxLoops))
            {
                StopTierBgm();
                return;
            }

            AudioClip want = ResolveTierBgmClip();
            if (want == null)
            {
                StopTierBgm();
                return;
            }

            EnsureTierBgmAudioSource();
            if (_tierBgmRuntime == null)
                return;

            _tierBgmRuntime.volume = tierBgmVolume;

            float wantPitch = ResolveTierBgmPitch(maxLoops, want);

            if (_tierBgmRuntime.clip == want && _tierBgmRuntime.isPlaying)
            {
                if (!Mathf.Approximately(_tierBgmRuntime.pitch, wantPitch))
                    _tierBgmRuntime.pitch = wantPitch;

                return;
            }

            _tierBgmRuntime.Stop();
            _tierBgmRuntime.clip = want;
            _tierBgmRuntime.loop = true;
            _tierBgmRuntime.pitch = wantPitch;
            _tierBgmRuntime.Play();
        }
    }
}
