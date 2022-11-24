﻿using System.Collections.Generic;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Rune;
using Nekoyume.Model.State;
using UnityEngine;

namespace Nekoyume.UI.Module
{
    public class RuneSlots : MonoBehaviour
    {
        [SerializeField]
        private List<RuneSlotView> slots;

        public void Set(
            List<RuneSlot> runeSlotStates,
            System.Action<RuneSlotView> onClick,
            System.Action<RuneSlotView> onDoubleClick)
        {
            foreach (var state in runeSlotStates)
            {
                slots[state.Index].Set(state, onClick, onDoubleClick);
            }
        }

        public void ActiveWearable(List<int> slotIndexes)
        {
            foreach (var index in slotIndexes)
            {
                slots[index].IsWearableImage = true;
            }
        }

        public void UpdateNotification(RuneState runeState, BattleType battleType)
        {
            foreach (var slot in slots)
            {
                slot.UpdateNotification(runeState, battleType);
            }
        }
    }
}
