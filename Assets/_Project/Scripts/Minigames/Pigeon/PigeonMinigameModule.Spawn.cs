using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        bool TryArmPour()
        {
            if (noodlePrefab == null || cupNoodle == null || noodlePosition == null || cupAnimator == null)
            {
                Debug.LogError("[PigeonMinigameModule] noodlePrefab / cupNoodle / cupAnimator / noodlePosition 을 Inspector에 연결하세요.");
                return false;
            }

            cupNoodle.SetActive(false);
            return true;
        }

        void TickPour(float dt)
        {
            if (!_pourArmed)
                return;

            _pourElapsed += dt;

            switch (_pourPhase)
            {
                case PourPhase.Forward:
                    if (_pourElapsed >= _pourClipDuration)
                        FinishForwardAndBeginPileWave();
                    break;
                case PourPhase.Spawning:
                    TickPileWave();
                    break;
                case PourPhase.Reverse:
                    if (_pourElapsed >= _pourClipDuration)
                        FinishReverseAndCooldown();
                    break;
                case PourPhase.Cooldown:
                    if (_pourElapsed >= Mathf.Max(0f, pourCooldown))
                        StartPourForward();
                    break;
            }
        }

        void StartPourForward()
        {
            _pourPhase = PourPhase.Forward;
            _pourElapsed = 0f;
            _pourClipDuration = ResolveCupPourDuration();

            PlaceNoodlePositionOnScreen();
            cupNoodle.SetActive(true);
            cupAnimator.Play(CupPourForwardState, 0, 0f);
            cupAnimator.Update(0f);
        }

        void FinishForwardAndBeginPileWave()
        {
            PreparePileWave();
            _pourPhase = PourPhase.Spawning;
            _pourElapsed = 0f;
            SpawnNextPile();
            if (_pourJitterIndex >= _pourJitterCount)
                StartPourReverse();
        }

        void TickPileWave()
        {
            if (_pourJitterIndex >= _pourJitterCount)
            {
                StartPourReverse();
                return;
            }

            float stagger = Mathf.Max(0f, pileSpawnStagger);
            if (stagger <= 0f)
            {
                while (_pourJitterIndex < _pourJitterCount)
                    SpawnNextPile();
                StartPourReverse();
                return;
            }

            if (_pourElapsed < stagger)
                return;

            _pourElapsed = 0f;
            SpawnNextPile();
            if (_pourJitterIndex >= _pourJitterCount)
                StartPourReverse();
        }

        void StartPourReverse()
        {
            _pourPhase = PourPhase.Reverse;
            _pourElapsed = 0f;
            cupAnimator.Play(CupPourReverseState, 0, 0f);
            cupAnimator.Update(0f);
        }

        void FinishReverseAndCooldown()
        {
            cupNoodle.SetActive(false);
            _pourPhase = PourPhase.Cooldown;
            _pourElapsed = 0f;
        }

        float ResolveCupPourDuration()
        {
            if (cupPourDuration > 0f)
                return cupPourDuration;

            RuntimeAnimatorController ctrl = cupAnimator.runtimeAnimatorController;
            if (ctrl != null)
            {
                AnimationClip[] clips = ctrl.animationClips;
                if (clips != null && clips.Length > 0 && clips[0] != null && clips[0].length > 0f)
                    return clips[0].length;
            }

            return DefaultCupPourDuration;
        }

        void PlaceNoodlePositionOnScreen()
        {
            if (playCamera == null || !playCamera.orthographic)
                return;

            float h = playCamera.orthographicSize;
            float w = h * playCamera.aspect;
            Vector3 cam = playCamera.transform.position;
            Vector3 p = noodlePosition.position;
            p.x = Random.Range(cam.x - w, cam.x + w);
            p.y = Random.Range(cam.y - h, cam.y + h);
            noodlePosition.position = p;
        }

        void PreparePileWave()
        {
            int count = Mathf.Max(0, pilesPerPour);
            float radius = Mathf.Max(0f, spawnJitterRadius);

            if (_pourJitters == null || _pourJitters.Length < count)
                _pourJitters = new Vector2[Mathf.Max(count, 1)];

            _pourJitterCount = count;
            _pourJitterIndex = 0;

            for (var i = 0; i < count; i++)
                _pourJitters[i] = radius > 0f ? Random.insideUnitCircle * radius : Vector2.zero;

            for (var i = 1; i < count; i++)
            {
                Vector2 key = _pourJitters[i];
                int j = i - 1;
                while (j >= 0 && _pourJitters[j].sqrMagnitude > key.sqrMagnitude)
                {
                    _pourJitters[j + 1] = _pourJitters[j];
                    j--;
                }

                _pourJitters[j + 1] = key;
            }
        }

        void SpawnNextPile()
        {
            if (_pourJitterIndex >= _pourJitterCount || noodlePrefab == null || noodlePosition == null)
                return;

            Vector2 jitter = _pourJitters[_pourJitterIndex];
            _pourJitterIndex++;

            Transform parent = pileParent != null ? pileParent : noodlePosition.parent;
            Vector3 world = noodlePosition.TransformPoint(new Vector3(jitter.x, jitter.y, 0f));

            GameObject pile = Instantiate(noodlePrefab, world, Quaternion.identity, parent);

            SpriteRenderer sr = pile.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = ++_nextSortingOrder;
        }
    }
}
