namespace BaSL.Users;

public sealed class UserContext
{

    public static implicit operator User(UserContext context) => context.User;

    internal UserContext(User user) => User = user;

    public User User { get; }

}
