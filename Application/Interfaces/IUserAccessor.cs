namespace Application.Interfaces
{
    public interface IUserAccessor //we'll be able to use this inside our application project to go and get this username
    {
         string GetUsername();//But the functionality for this is going to be contained inside our infrastructure project.
    }
}