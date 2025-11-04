# SubscriptionService

Service quản lý gói thuê bao và hóa đơn cho xe điện.

## Chức năng chính

### 1. Quản lý Subscription Plans
- **GET** `/api/SubscriptionPlan` - Lấy tất cả gói thuê bao
- **GET** `/api/SubscriptionPlan/{id}` - Lấy thông tin gói theo ID

### 2. Quản lý Vehicle Subscriptions
- **POST** `/api/VehicleSubscription/register` - Đăng ký gói thuê bao cho xe
  - `vehicleId`: ID của xe
  - `subscriptionPlanId`: ID của gói thuê bao
  - `autoRenew`: Tự động gia hạn (true/false)
- **GET** `/api/VehicleSubscription/vehicle/{vehicleId}` - Lấy gói thuê bao hiện tại của xe
- **GET** `/api/VehicleSubscription/{id}` - Lấy thông tin gói thuê bao theo ID

### 3. Quản lý Charging Sessions
- **POST** `/api/ChargingSession/start` - Bắt đầu session sạc
  - `vehicleSubscriptionId`: ID gói thuê bao
  - `stationId`: ID trạm sạc
  - `startTime`: Thời gian bắt đầu
- **POST** `/api/ChargingSession/end` - Kết thúc session sạc
  - `sessionId`: ID session
  - `endTime`: Thời gian kết thúc
  - `batteryNeededKwh`: Lượng điện cần để đầy pin
- **GET** `/api/ChargingSession/subscription/{subscriptionId}` - Lấy danh sách session theo subscription
- **GET** `/api/ChargingSession/{id}` - Lấy chi tiết session

### 4. Quản lý Payment & Billing
- **GET** `/api/Payment/summary/{vehicleSubscriptionId}` - Lấy tổng tiền và tổng kWh đã sạc
- **POST** `/api/Payment/generate-invoice/{vehicleSubscriptionId}` - Tạo hóa đơn tháng
- **GET** `/api/Payment/invoices/{vehicleSubscriptionId}` - Lấy danh sách hóa đơn

## Business Logic

### Cách tính tiền sạc:
```
- Khi bắt đầu session: Ghi nhận thời gian bắt đầu
- Khi kết thúc session: 
  1. Tính thời gian sạc (minutes)
  2. Tính kWh đã sử dụng = (thời gian / 60) * công suất trạm (kWh)
  3. Tính tiền:
     - Nếu kWh đã sạc < kWh cần để đầy pin: 
       Tiền = kWh đã sạc * giá kWh
     - Nếu kWh đã sạc >= kWh cần để đầy pin:
       Tiền = kWh cần để đầy pin * giá kWh
  4. Lưu session với các thông số: kWh, thời gian, tiền
  5. Cập nhật VehicleSubscriptionUsage
```

### Hóa đơn tháng:
- Mỗi 30 ngày từ ngày đăng ký, hệ thống sẽ tính hóa đơn
- Bao gồm:
  - Phí cơ bản (nếu auto_renew = true)
  - Tổng tiền sạc trong tháng (tổng kWh đã sạc * giá kWh)
- Mỗi session sẽ có 1 hóa đơn riêng

## Environment Variables
```
MONGO_URI=<MongoDB connection string>
MONGO_DB_NAME=ev_subscription
JWT_SECRET=<JWT secret key>
JWT_ISSUER=<JWT issuer>
JWT_AUDIENCE=<JWT audience>
```

## Run
```bash
dotnet run --project Services/SubscriptionService/SubscriptionService.csproj
```

Service chạy trên port: **http://localhost:5003**

