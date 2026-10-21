namespace HospitalAppointmentSystem.Models; // Same namespace as the other models

// Post model: matches one JSON object returned by https://jsonplaceholder.typicode.com/posts
public class Post
{
    public int UserId { get; set; } // Id of the user who wrote the post (comes from JSON "userId")

    public int Id { get; set; } // Post id (comes from JSON "id") - displayed in the table

    public string Title { get; set; } = string.Empty; // Post title (comes from JSON "title") - displayed in the table

    public string Body { get; set; } = string.Empty; // Post body text (comes from JSON "body") - not displayed, only mapped
}
