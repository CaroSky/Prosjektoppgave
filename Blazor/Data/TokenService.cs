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
            if (_jwtToken != value)
            {
                _jwtToken = value;
                _logger.LogInformation("Token has been set in TokenService.");
                NotifyStateChanged(); // Inform subscribers that the token has changed.
            }
        }
    }
    public event Action OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}
