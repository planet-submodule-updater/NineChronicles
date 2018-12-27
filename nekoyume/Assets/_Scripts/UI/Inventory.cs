using System;
using Nekoyume.Move;
using System.Collections.Generic;
using Nekoyume.Game.Item;
using Newtonsoft.Json;
using UnityEngine;


namespace Nekoyume.UI
{
    public class Inventory : Widget
    {
        [SerializeField]
        private Transform _grid;
        [SerializeField]
        private GameObject _slotBase;

        private List<InventorySlot> _slots;

        private void Awake()
        {
            _slots = new List<InventorySlot>();
            _slotBase.SetActive(true);
            for (int i = 0; i < 40; ++i)
            {
                GameObject newSlot = Instantiate(_slotBase, _grid);
                InventorySlot slot = newSlot.GetComponent<InventorySlot>();
                _slots.Add(slot);
            }
            _slotBase.SetActive(false);
            Game.Event.OnUpdateEquipment.AddListener(UpdateEquipment);
        }

        private void UpdateEquipment(Equipment equipment)
        {
            foreach (var slot in _slots)
            {
                var item = slot.Item;
                if (item != null && item.Cls == "Weapon")
                {
                    slot.LabelEquip.text = "";
                    if (item.Id == equipment.Data.Id)
                    {
                        slot.LabelEquip.text = equipment.IsEquipped ? "E" : "";
                    }
                }
            }
        }

        public override void Show()
        {
            // FIX ME : get item list
            string itemsstr = MoveManager.Instance.Avatar.Items;
            List<Game.Item.Inventory.InventoryItem> items;
            try
            {
                items = JsonConvert.DeserializeObject<List<Game.Item.Inventory.InventoryItem>>(itemsstr);
            }
            catch (ArgumentNullException)
            {
                items = null;
            }
            for (int i = 0; i < 40; ++i)
            {
                InventorySlot slot = _slots[i];
                if (items != null && items.Count > i)
                {
                    var inventoryItem = items[i];
                    slot.Set(inventoryItem.Item.Data, inventoryItem.Count);
                }
                else
                {
                    slot.Clear();
                }
            }

            base.Show();
        }

    }
}
