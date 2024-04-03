namespace WebStoreMVC.Domain.Enum;

public enum ErrorCode
{
    //GeneralErrors 200 - ?
    AccessError = 200,

    //For models 100 - ?
    CreatingModelIsFailed = 100,

    //For AuthService 1-10
    RolesAlreadyExists = 1,
    UserAlreadyExists = 2,
    IncorrectCredentials = 3,
    IncorrectToken = 4,
    UserDoesNotExist = 5,
    UserAlreadyIsAdmin = 6,

    //For SearchingProducts Service 11 - 20
    ProductsAreNotFound = 11,
}