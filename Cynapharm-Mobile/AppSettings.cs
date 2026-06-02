namespace Cynapharm_Mobile;

public class AppSettings
{
    /// <summary>Development / emulator URL (used in Debug builds and Android emulator).</summary>
    public string ApiGatewayBaseUrl { get; set; } = "http://cynapharmgateway.runasp.net/";

    /// <summary>
    /// Production URL — used automatically in Release builds via MauiProgram.
    /// Falls back to ApiGatewayBaseUrl when null or empty.
    /// </summary>
    public string? ApiGatewayBaseUrlProd { get; set; }
}
