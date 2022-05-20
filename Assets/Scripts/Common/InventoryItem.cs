namespace Common {
    public class InventoryItem {
        public int rId { get; set; }
        public int count { get; set; }

        public InventoryItem(int rId, int count) {
            this.rId = rId;
            this.count = count;
        }

        public InventoryItem() {}
    }
}