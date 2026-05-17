using Common.Exceptions;

namespace PaymentService.Configs;

public static class PayOSConfigsLoader
{
    public static PayOSConfigs LoadAndValidate(IConfiguration configuration)
    {
        var payOSConfigs = configuration.GetSection("PayOS").Get<PayOSConfigs>()
            ?? throw new InternalServerException("PayOS configuration section is required.");

        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(payOSConfigs.ApiKey))
            missingFields.Add("PayOS:ApiKey");
        if (string.IsNullOrWhiteSpace(payOSConfigs.ClientId))
            missingFields.Add("Keycloak:ClientId");
        if (string.IsNullOrWhiteSpace(payOSConfigs.ChecksumKey))
            missingFields.Add("Keycloak:ChecksumKey");
        if (string.IsNullOrWhiteSpace(payOSConfigs.ReturnUrl))
            missingFields.Add("Keycloak:ReturnUrl");
        if (string.IsNullOrWhiteSpace(payOSConfigs.CancelUrl))
            missingFields.Add("Keycloak:CancelUrl");

        if (missingFields.Count > 0)
            throw new InternalServerException(
                $"Missing required PayOS configuration fields: {string.Join(", ", missingFields)}");

        return payOSConfigs;
    }
}