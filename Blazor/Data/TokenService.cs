public class TokenService
{
    private string _jwtToken;
    private readonly ILogger<TokenService> _logger;

    public TokenService(ILogger<TokenService> logger)
    {
        _logger = logger;
    }

    public string JwtToken
    {
        get => _jwtToken;
        set
        {
            _jwtToken = value;
            _logger.LogInformation("Token has been set in TokenService.");
        }
    }
}
