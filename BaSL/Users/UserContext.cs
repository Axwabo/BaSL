namespace BaSL.Users;

public sealed record UserContext
{

    public static implicit operator User(UserContext context) => context.User;

    internal UserContext(User user) => User = user;

    public User User { get; }

    public string Name => User.Username;

}
