namespace Common {
    public class InventoryItem {
        public InventoryItem(int rId, int count) {
            this.rId = rId;
            this.count = count;
        }

        public InventoryItem() {}
        public int rId { get; }
        public int count { get; set; }
    }
}