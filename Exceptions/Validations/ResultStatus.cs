namespace DotNet8Starter.Exceptions.Validations
{
	public enum ResultStatus
	{
		None, // Undefined
		Ok, // 200,
		Created, // 201
		Empty, // 204
		BadRequestException, // 400
		UnauthorizedException, // 401
		ForbiddenException, // 403
		NotFoundException, // 404
		ConflictException, // 409
		UnprocessableEntityException, // 422
		TooManyRequestsException, // 429
		InternalServerException, // 500
		BadGatewayException, // 502
		ServiceUnavailableException, // 503
		GatewayTimeoutException, // 504
	}
}
