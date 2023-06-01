namespace Domain
{
    public class Activity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public bool IsCancelled { get; set; } //if the user is the host of an event, then we don't want them to be able to remove themselves from the event, but rather we're going to allow them to cancel the activity itself.
        public ICollection<ActivityAttendee> Attendees { get; set; } = new List<ActivityAttendee>(); //new list so that we don't get null reference when accessing Attendees
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}