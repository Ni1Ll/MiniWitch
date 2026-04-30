public class Order
{
    public int id;
    public string customerName;
    public bool isPending;

    public ItemType itemType;
    public Recipe recipe; // пока простая

    public int price;
}