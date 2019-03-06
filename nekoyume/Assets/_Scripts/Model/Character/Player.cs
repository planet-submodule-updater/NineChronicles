using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Action;
using Nekoyume.Data.Table;
using Nekoyume.Game.Item;

namespace Nekoyume.Model
{
    [Serializable]
    public class Player : CharacterBase
    {
        public long exp;
        public long expMax;
        public int level;
        public int stage;
        public Weapon weapon;
        public Armor armor;
        public Belt belt;
        public Necklace necklace;
        public Ring ring;
        public Helm helm;

        public readonly Inventory inventory;
        public List<Inventory.InventoryItem> Items => inventory.items;

        public Player(Avatar avatar, Simulator simulator = null)
        {
            exp = avatar.EXP;
            level = avatar.Level;
            stage = avatar.WorldStage;
            this.simulator = simulator;
            inventory = new Inventory();
            var inventoryItems = avatar.Items;
            if (inventoryItems != null)
            {
                Equip(inventoryItems);
                inventory.Set(inventoryItems);
            }

            CalcStats(level);
        }

        public void GetExp(Monster monster)
        {
            exp += monster.rewardExp;
            while (expMax <= exp)
            {
                LevelUp();
            }
            targets.Remove(monster);
        }

        protected override void OnDead()
        {
            base.OnDead();
            simulator.Lose = true;
        }

        public void CalcStats(int lv)
        {
            var stats = ActionManager.Instance.tables.Stats;
            Stats data;
            stats.TryGetValue(lv, out data);
            if (data == null)
            {
                throw new InvalidActionException();
            }
            hp = data.Health;
            atk = data.Attack;
            def = data.Defense;
            hpMax = data.Health;
            expMax = data.Exp;
            if (weapon?.equipped == true)
            {
                atk += weapon.Data.Param_0;
            }
            if (armor?.equipped == true)
            {
                def += armor.Data.Param_0;
            }
        }
        private void LevelUp()
        {
            if (exp < expMax)
                return;

            exp -= expMax;
            level++;

            CalcStats(level);

            var levelUp = new LevelUp
            {
                character = Copy(this),
            };
            simulator.Log.Add(levelUp);
        }

        public void GetItem(ItemBase item)
        {
            inventory.Add(item);
        }

        public void Equip(List<Inventory.InventoryItem> items)
        {
            var equipments = items.Select(i => i.Item).OfType<Equipment>().Where(e => e.equipped);
            foreach (var equipment in equipments)
            {
                switch ((ItemBase.ItemType) Enum.Parse(typeof(ItemBase.ItemType), equipment.Data.Cls))
                {
                    case ItemBase.ItemType.Weapon:
                        weapon = equipment as Weapon;
                        break;
                    case ItemBase.ItemType.RangedWeapon:
                        weapon = equipment as RangedWeapon;
                        break;
                    case ItemBase.ItemType.Armor:
                        armor = equipment as Armor;
                        break;
                    case ItemBase.ItemType.Belt:
                        belt = equipment as Belt;
                        break;
                    case ItemBase.ItemType.Necklace:
                        necklace = equipment as Necklace;
                        break;
                    case ItemBase.ItemType.Ring:
                        ring = equipment as Ring;
                        break;
                    case ItemBase.ItemType.Helm:
                        helm = equipment as Helm;
                        break;
                    default:
                        throw new InvalidEquipmentException();
                }
            }
        }
    }

    public class InvalidEquipmentException : Exception
    {
    }
}
