using System.Net;

namespace B2BCommerce.Backend.Domain.Helpers;

/// <summary>
/// Helper class for IP address validation and CIDR range checking
/// </summary>
public static class IpAddressHelper
{
    /// <summary>
    /// Validates if the input is a valid IP address or CIDR notation
    /// </summary>
    /// <param name="input">IP address or CIDR notation to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidIpOrCidr(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        // Check for CIDR notation
        if (input.Contains('/'))
        {
            var parts = input.Split('/');
            if (parts.Length != 2) return false;

            if (!IPAddress.TryParse(parts[0], out var address)) return false;
            if (!int.TryParse(parts[1], out var prefix)) return false;

            // Valid prefix range for IPv4 (0-32) and IPv6 (0-128)
            var maxPrefix = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
            return prefix >= 0 && prefix <= maxPrefix;
        }

        return IPAddress.TryParse(input, out _);
    }

    /// <summary>
    /// Checks if an IP address is within a CIDR range
    /// </summary>
    /// <param name="ipAddress">IP address to check</param>
    /// <param name="cidr">CIDR range or exact IP address</param>
    /// <returns>True if IP is in range, false otherwise</returns>
    public static bool IsInRange(string ipAddress, string cidr)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(cidr))
            return false;

        if (!cidr.Contains('/'))
        {
            return string.Equals(ipAddress, cidr, StringComparison.OrdinalIgnoreCase);
        }

        var parts = cidr.Split('/');
        if (!IPAddress.TryParse(parts[0], out var network)) return false;
        if (!int.TryParse(parts[1], out var prefixLength)) return false;
        if (!IPAddress.TryParse(ipAddress, out var address)) return false;

        var networkBytes = network.GetAddressBytes();
        var addressBytes = address.GetAddressBytes();

        if (networkBytes.Length != addressBytes.Length) return false;

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        for (int i = 0; i < fullBytes; i++)
        {
            if (networkBytes[i] != addressBytes[i]) return false;
        }

        if (remainingBits > 0 && fullBytes < networkBytes.Length)
        {
            var mask = (byte)(0xFF << (8 - remainingBits));
            if ((networkBytes[fullBytes] & mask) != (addressBytes[fullBytes] & mask))
                return false;
        }

        return true;
    }
}
