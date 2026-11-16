namespace ECommerce.Api.Shared.Exceptions;

public sealed class DomainConflictException() : Exception("The request conflicts with existing state.");
