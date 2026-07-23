namespace BaSL.Users;

public readonly record struct UserContext
{

    public static implicit operator User(UserContext context) => context.User;

    // TODO
    public UserContext(User user) => User = user;

    public User User { get; }

    public string Name => User.Username;

}
