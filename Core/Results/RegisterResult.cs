namespace Core.Results;

public enum RegisterError
{
    None,
    UsernameAlreadyExists,
    DefaultRoleNotFound,
    PersistenceError
}

public record RegisterResult(
    bool Succeeded,
    RegisterError Error
);