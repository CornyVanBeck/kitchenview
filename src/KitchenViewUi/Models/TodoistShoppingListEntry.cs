namespace kitchenview.Models
{
    public class TodoistShoppingListEntry : IShoppingListEntry
    {
        public string Name
        {
            get; set;
        }

        public string Collaborator
        {
            get; set;
        }
    }
}