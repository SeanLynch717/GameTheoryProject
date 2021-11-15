using System;
using System.Collections.Generic;
using System.Text;

namespace Crates
{
    public class Cell
    {
        Dictionary<ItemType, bool> itemLookup;
        private Direction direction;

        public Cell(List<ItemType> occupies, Direction direction = Direction.None)
        {
            itemLookup = new Dictionary<ItemType, bool>();
            itemLookup.Add(ItemType.Conveyer, occupies.FindIndex(f => f == ItemType.Conveyer) >= 0 ? true : false);
            itemLookup.Add(ItemType.Guard, occupies.FindIndex(f => f == ItemType.Guard) >= 0 ? true : false);
            itemLookup.Add(ItemType.Trap, occupies.FindIndex(f => f == ItemType.Trap) >= 0 ? true : false);
            itemLookup.Add(ItemType.Crate, occupies.FindIndex(f => f == ItemType.Crate) >= 0 ? true : false);
            itemLookup.Add(ItemType.Block, occupies.FindIndex(f => f == ItemType.Block) >= 0 ? true : false);
            itemLookup.Add(ItemType.End, occupies.FindIndex(f => f == ItemType.End) >= 0 ? true : false);
            itemLookup.Add(ItemType.None, occupies.FindIndex(f => f == ItemType.None) >= 0 ? true : false);

            this.direction = direction;
        }
        public static Cell DeepCopy(Cell copy)
        {
            // This cell is off the board, doesn't exist.
            if (copy == null)
            {
                return null;
            }

            Cell c = new Cell(new List<ItemType>());
            if (copy.Crate) { c.Crate = true; }
            if (copy.Empty) { c.Empty = true; }
            if (copy.Block) { c.Block = true; }
            if (copy.Guard) { c.Guard = true; }
            if (copy.End) { c.End = true; }
            if (copy.Trap) { c.Trap = true; }
            if (copy.Conveyer)
            {
                c.Conveyer = true;
                c.Direction = copy.Direction;
            }

            return c;
        }

        public bool Block
        {
            get { return itemLookup[ItemType.Block]; }
            set { itemLookup[ItemType.Block] = value; }
        }

        public bool Trap
        {
            get { return itemLookup[ItemType.Trap]; }
            set { itemLookup[ItemType.Trap] = value; }
        }

        public bool Crate
        {
            get { return itemLookup[ItemType.Crate]; }
            set { itemLookup[ItemType.Crate] = value; }
        }

        public bool Guard
        {
            get { return itemLookup[ItemType.Guard]; }
            set { itemLookup[ItemType.Guard] = value; }
        }

        public bool Conveyer
        {
            get { return itemLookup[ItemType.Conveyer]; }
            set { itemLookup[ItemType.Conveyer] = value; }
        }
        public bool Empty
        {
            get { return itemLookup[ItemType.None]; }
            set { itemLookup[ItemType.None] = value; }
        }
        public bool End
        {
            get { return itemLookup[ItemType.End]; }
            set { itemLookup[ItemType.End] = value; }
        }

        public Direction Direction
        {
            get { return itemLookup[ItemType.Conveyer] ? direction : Direction.None; }
            set { direction = value; }
        }
    }
}
