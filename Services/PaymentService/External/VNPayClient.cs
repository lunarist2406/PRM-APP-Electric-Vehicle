using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Models.DTOs;

namespace PaymentService.External
{
    public class VNPayClient
    {
        private readonly string _baseUrl;
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly ILogger<VNPayClient> _logger;

        public VNPayClient(IConfiguration config, ILogger<VNPayClient> logger)
        {
            _baseUrl = config["VNPay:BaseUrl"] ?? "";
            _tmnCode = config["VNPay:TmnCode"] ?? "";
            _hashSecret = config["VNPay:HashSecret"] ?? "";
            _logger = logger;
        }

        public string CreatePaymentUrl(VNPayRequestDto dto)
        {
            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _tmnCode },
                { "vnp_Amount", ((int)(dto.Amount * 100)).ToString() }, // VNPay amount = VND * 100
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", dto.OrderId },
                { "vnp_OrderInfo", dto.OrderInfo },
                { "vnp_OrderType", dto.OrderType ?? "billpayment" },
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", dto.ReturnUrl },
                { "vnp_IpAddr", dto.IpAddress },
                { "vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss") }
            };

            string queryString = string.Join("&", vnpParams.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
            string hashData = string.Join("&", vnpParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            string secureHash = CreateHmacSha512(_hashSecret, hashData);

            return $"{_baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        private string CreateHmacSha512(string key, string data)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}
