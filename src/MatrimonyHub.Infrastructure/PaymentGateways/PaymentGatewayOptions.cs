namespace MatrimonyHub.Infrastructure.PaymentGateways;

public class PaymentGatewayOptions
{
    public const string SectionName = "PaymentGateways";

    public decimal ContactUnlockFee { get; set; } = 500.00m; // Default BDT 500
    public string ActiveGateway { get; set; } = "Sandbox";

    public BkashOptions Bkash { get; set; } = new();
    public SslCommerzOptions SSLCommerz { get; set; } = new();
    public NagadOptions Nagad { get; set; } = new();
}

public class BkashOptions
{
    public string AppKey { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://tokenized.sandbox.bka.sh/v1.2.0-beta";
}

public class SslCommerzOptions
{
    public string StoreId { get; set; } = string.Empty;
    public string StorePassword { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
    public string BaseUrl => IsSandbox ? "https://sandbox.sslcommerz.com" : "https://securepay.sslcommerz.com";
}

public class NagadOptions
{
    public string MerchantId { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox.mynagad.com:10080/remote-payment-gateway-1.0";
}
