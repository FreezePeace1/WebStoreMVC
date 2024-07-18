namespace WebStoreMVC.Domain.Enum;

public enum ErrorCode
{
    //GeneralErrors 200 - ?
    AccessError = 200,
    InternalServerError = 201,

    //For models 100 - ?
    CreatingModelIsFailed = 100,

    //For AuthService 1-10
    RolesAlreadyExists = 1,
    UserAlreadyExists = 2,
    IncorrectCredentials = 3,
    IncorrectToken = 4,
    UserDoesNotExist = 5,
    UserAlreadyIsAdmin = 6,
    TokenIsNullOrNotFound = 7,
    EmailNotFound = 8,

    //For SearchingProducts Service 11 - 20
    ProductsAreNotFound = 11,
    ProductAlreadyExists = 12,
    
    //OrderService 21 - 30
    CartIsEmpty = 21,
    GettingOrderDataIsFailed = 22,
    OrderDeletingIsFailed = 23,
    TransactionIsFailed = 24,
    
    //AccountService 31 - 40
    FailureToShowOrderInfo = 31,
    ProductsNotFound = 32,
    FailureToGetUserDeliveryInfo = 33,
    
    //Cart service 41 - 50
    FailureToAddProductToCart = 41,
    
    //EmailFailure
    EmailFailure = 300,
   
}