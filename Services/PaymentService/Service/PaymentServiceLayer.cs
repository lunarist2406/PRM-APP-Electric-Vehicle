using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentService.Models;
using PaymentService.Repositories;
using PaymentService.External;
using PaymentService.Models.DTOs;

namespace PaymentService.Services
{
	public class PaymentServiceLayer
	{
		private readonly IPaymentRepository _repo;
		private readonly VehicleClient _vehicleClient;
		private readonly UserClient _userClient;
		private readonly BillingClient _billingClient;
		private readonly VNPayClient _vnpayClient;
		private readonly ILogger<PaymentServiceLayer> _logger;

		public PaymentServiceLayer(
			IPaymentRepository repo,
			VehicleClient vehicleClient,
			UserClient userClient,
			BillingClient billingClient,
			VNPayClient vnpayClient,
			ILogger<PaymentServiceLayer> logger)
		{
			_repo = repo;
			_vehicleClient = vehicleClient;
			_userClient = userClient;
			_billingClient = billingClient;
			_vnpayClient = vnpayClient;
			_logger = logger;
		}

		// Tạo payment
		public async Task<PaymentResultDto> CreatePaymentsFromBillingAsync(string token)
		{
			_logger.LogInformation("🔄 Fetching monthly bills from BillingService...");

			var response = await _billingClient.GenerateMonthlyBillsAsync(token);
			var bills = response?.Results ?? new List<BillingItemDto>();

			if (bills.Count == 0)
			{
				_logger.LogWarning("⚠️ No bills returned from BillingService.");
				return new PaymentResultDto
				{
					Success = false,
					Message = "Hiện tại bạn không có hóa đơn cần thanh toán."
				};
			}

			// 👉 Giả sử chỉ xử lý bill đầu tiên (hoặc bạn có thể loop tạo nhiều nếu cần)
			var bill = bills.First();
			try
			{
				var vehicle = await _vehicleClient.GetVehicleByIdAsync(bill.VehicleId, token);
				if (vehicle == null)
					return new PaymentResultDto { Success = false, Message = "Không tìm thấy phương tiện tương ứng." };

				var user = await _userClient.GetUserByIdAsync(vehicle.UserId, token);
				if (user == null)
					return new PaymentResultDto { Success = false, Message = "Không tìm thấy người dùng tương ứng." };

				var orderId = Guid.NewGuid().ToString();

				var payment = new Payment
				{
					UserId = user.Id,
					VehicleId = bill.VehicleId,
					Amount = (decimal)bill.TotalAmount,
					OrderId = orderId,
					Status = "Pending",
					CreatedAt = DateTime.UtcNow
				};

				// 👉 Tạo URL VNPay
				var returnUrl = Environment.GetEnvironmentVariable("VNPAY_RETURN_URL") ??
								"https://yourapp.onrender.com/api/payment/return-vnpay";

				var vnPayUrl = _vnpayClient.CreatePaymentUrl(new VNPayRequestDto
				{
					Amount = payment.Amount,
					OrderId = payment.OrderId,
					OrderInfo = $"Thanh toán cho xe {bill.VehicleId}",
					ReturnUrl = returnUrl,
					IpAddress = "127.0.0.1" // có thể thay bằng IP thật
				});

				payment.PaymentUrl = vnPayUrl;
				await _repo.CreateAsync(payment);

				_logger.LogInformation("✅ Created payment {OrderId} for user {UserId}, vehicle {VehicleId}",
					orderId, user.Id, bill.VehicleId);

				return new PaymentResultDto
				{
					Success = true,
					Message = "Tạo thanh toán thành công!",
					PaymentUrl = vnPayUrl
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Failed to create payment for bill {BillId}", bill.VehicleId);
				return new PaymentResultDto { Success = false, Message = "Đã xảy ra lỗi trong quá trình tạo thanh toán." };
			}
		}




		// Lấy payments của user
		public async Task<List<Payment>> GetUserPaymentsAsync(string userId)
		{
			return await _repo.GetByUserIdAsync(userId);
		}

		// Lấy tất cả payments (admin)
		public async Task<List<Payment>> GetAllPaymentsAsync()
		{
			return await _repo.GetAllAsync();
		}

		// Duyệt payment
		public async Task ApprovePaymentAsync(string paymentId)
		{
			var payment = await _repo.GetByIdAsync(paymentId);
			if (payment == null)
			{
				_logger.LogWarning("Payment {PaymentId} not found for approval", paymentId);
				throw new Exception("Payment not found");
			}

			// TODO: Call VNPay hoặc Billing API để xử lý thanh toán nếu cần
			_logger.LogInformation("Approving payment {PaymentId}", paymentId);
			await _repo.UpdateStatusAsync(paymentId, "Approved");
		}

		// Hủy payment
		public async Task CancelPaymentAsync(string paymentId)
		{
			var payment = await _repo.GetByIdAsync(paymentId);
			if (payment == null)
			{
				_logger.LogWarning("Payment {PaymentId} not found for cancellation", paymentId);
				throw new Exception("Payment not found");
			}

			_logger.LogInformation("Cancelling payment {PaymentId}", paymentId);
			await _repo.UpdateStatusAsync(paymentId, "Canceled");
		}

		// Tạo URL thanh toán VNPay
		public string GenerateVNPayUrl(Payment payment, string returnUrl, string ipAddress)
		{
			var requestDto = new VNPayRequestDto
			{
				Amount = payment.Amount,
				OrderId = payment.OrderId,
				OrderInfo = $"Payment for vehicle {payment.VehicleId}",
				ReturnUrl = returnUrl,
				IpAddress = ipAddress
			};

			return _vnpayClient.CreatePaymentUrl(requestDto);
		}
	}
}
