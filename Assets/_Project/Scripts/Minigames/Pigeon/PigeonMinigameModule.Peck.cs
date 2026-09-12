using MiniParty.Input;
using UnityEngine;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        void TickPeck(int slotIndex, float dt)
        {
            switch (_peckPhase[slotIndex])
            {
                case PeckPhase.Idle:
                    if (BoothUsbSlotInput.WasPathPressed(slotIndex, SlotGamepad.Get(slotIndex), BoothUsbGamepadLayout.FaceA))
                        StartPeckForward(slotIndex);
                    break;
                case PeckPhase.Forward:
                    _peckElapsed[slotIndex] += dt;
                    if (_peckElapsed[slotIndex] >= _peckClipDuration[slotIndex])
                    {
                        ResolvePeckHit(slotIndex);
                        StartPeckReverse(slotIndex);
                    }

                    break;
                case PeckPhase.Reverse:
                    _peckElapsed[slotIndex] += dt;
                    if (_peckElapsed[slotIndex] >= _peckClipDuration[slotIndex])
                        EndPeck(slotIndex);
                    break;
            }
        }

        void StartPeckForward(int slotIndex)
        {
            Animator anim = GetPeckAnimator(slotIndex);
            if (anim == null)
                return;

            _peckPhase[slotIndex] = PeckPhase.Forward;
            _peckElapsed[slotIndex] = 0f;
            _peckClipDuration[slotIndex] = ResolvePeckDuration(anim);
            _peckFromRight[slotIndex] = IsCursorRightOfScreenCenter(slotIndex);

            SetPeckVisuals(slotIndex, pigeonOn: true, mouthOn: false);
            anim.Play(_peckFromRight[slotIndex] ? PeckRightForwardState : PeckForwardState, 0, 0f);
            anim.Update(0f);

            if (peckSfxSource != null && peckSfxClip != null)
                peckSfxSource.PlayOneShot(peckSfxClip);
        }

        void StartPeckReverse(int slotIndex)
        {
            Animator anim = GetPeckAnimator(slotIndex);
            if (anim == null)
            {
                EndPeck(slotIndex);
                return;
            }

            _peckPhase[slotIndex] = PeckPhase.Reverse;
            _peckElapsed[slotIndex] = 0f;
            anim.Play(_peckFromRight[slotIndex] ? PeckRightReverseState : PeckReverseState, 0, 0f);
            anim.Update(0f);
        }

        void EndPeck(int slotIndex)
        {
            _peckPhase[slotIndex] = PeckPhase.Idle;
            SetPeckVisuals(slotIndex, pigeonOn: false, mouthOn: false);
        }

        void ResolvePeckHit(int slotIndex)
        {
            CircleCollider2D cursorCol = GetPeckCursorCollider(slotIndex);
            if (cursorCol == null)
            {
                SetMouth(slotIndex, false);
                return;
            }

            var filter = new ContactFilter2D();
            filter.NoFilter();
            filter.useTriggers = true;
            int hits = cursorCol.OverlapCollider(filter, _peckOverlap);

            int bestIndex = -1;
            int bestOrder = int.MinValue;

            for (var h = 0; h < hits; h++)
            {
                Collider2D hit = _peckOverlap[h];
                if (hit == null)
                    continue;

                int pileIndex = FindFloorPileIndex(hit);
                if (pileIndex < 0)
                    continue;

                SpriteRenderer sr = _floorPiles[pileIndex].Sr;
                int order = sr != null ? sr.sortingOrder : 0;
                if (order >= bestOrder)
                {
                    bestOrder = order;
                    bestIndex = pileIndex;
                }
            }

            if (bestIndex < 0)
            {
                SetMouth(slotIndex, false);
                return;
            }

            FloorPile pile = _floorPiles[bestIndex];
            _floorPiles.RemoveAt(bestIndex);
            if (pile.Go != null)
                Destroy(pile.Go);

            _score[slotIndex] += Mathf.Max(0, scorePerPile);
            SetMouth(slotIndex, true);
        }

        int FindFloorPileIndex(Collider2D col)
        {
            for (var i = 0; i < _floorPiles.Count; i++)
            {
                if (_floorPiles[i].Col == col)
                    return i;
            }

            return -1;
        }

        void SetPeckVisuals(int slotIndex, bool pigeonOn, bool mouthOn)
        {
            GameObject pigeon = GetAt(peckPigeons, slotIndex);
            if (pigeon != null)
                pigeon.SetActive(pigeonOn);

            SetMouth(slotIndex, mouthOn);
        }

        void SetMouth(int slotIndex, bool on)
        {
            GameObject mouth = GetAt(mouthNoodles, slotIndex);
            if (mouth != null)
                mouth.SetActive(on);
        }

        static GameObject GetAt(GameObject[] arr, int i)
        {
            if (arr == null || i < 0 || i >= arr.Length)
                return null;

            return arr[i];
        }

        Animator GetPeckAnimator(int slotIndex)
        {
            if (peckAnimators == null || slotIndex < 0 || slotIndex >= peckAnimators.Length)
                return null;

            return peckAnimators[slotIndex];
        }

        CircleCollider2D GetPeckCursorCollider(int slotIndex)
        {
            if (peckCursorColliders == null || slotIndex < 0 || slotIndex >= peckCursorColliders.Length)
                return null;

            return peckCursorColliders[slotIndex];
        }


        bool IsCursorRightOfScreenCenter(int slotIndex)
        {
            Transform cursor = GetCursor(slotIndex);
            if (cursor == null)
                return false;

            float mid = playCamera != null ? playCamera.transform.position.x : 0f;
            return cursor.position.x > mid;
        }

        float ResolvePeckDuration(Animator anim)
        {
            if (peckDuration > 0f)
                return peckDuration;

            RuntimeAnimatorController ctrl = anim.runtimeAnimatorController;
            if (ctrl != null)
            {
                AnimationClip[] clips = ctrl.animationClips;
                if (clips != null && clips.Length > 0 && clips[0] != null && clips[0].length > 0f)
                    return clips[0].length;
            }

            return DefaultPeckDuration;
        }
    }
}
