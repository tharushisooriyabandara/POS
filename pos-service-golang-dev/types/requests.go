package types

/*
	types for authentication
*/

type LoginRequest struct {
	Email string `json:"email" validate:"required,email"`
	Pin   string `json:"pin" validate:"required,numeric,min=4,max=6"`
}

type RefreshRequest struct {
	RefreshToken string `json:"refreshToken" validate:"required,jwt"`
}

/**************************/

/*
types for orders
*/

type UpdateOrderStatusRequest struct {
	Status string `json:"status" validate:"required,oneof=QUEUE COMPLETED"`
}

type CreateOrderRequest struct {
	CustomerID         int         `json:"customer_id" validate:"required_with=CustomerName"`
	CustomerName       string      `json:"customer_name" validate:"required_with=CustomerID"`
	DeliveryDateTime   string      `json:"delivery_date_time" validate:"required,datetime=2006-01-02 15:04:05"`
	DeliveryPlatformID int         `json:"platform_id" validate:"required"`
	Discount           float64     `json:"discount" validate:"gte=0"`
	DiscountType       string      `json:"discount_type"`
	DisplayOrderID     string      `json:"display_order_id" validate:"required"`
	IsTableOrder       bool        `json:"is_table_order"`
	OrderItems         []OrderItem `json:"order_items" validate:"required,gte=1,dive"`
	OrderNote          string      `json:"order_note"`
	PaymentMethod      string      `json:"payment_method" validate:"required,oneof=CASH CARD"`
	ServiceCharge      string      `json:"service_charge"`
	ShippingMethod     string      `json:"shipping_method" validate:"required,oneof=TAKEAWAY DELIVERY DINE-IN"`
	ShippingTax        float64     `json:"shipping_tax" validate:"gte=0"`
	ShippingTotal      float64     `json:"shipping_total" validate:"gte=0"`
	ShopID             int         `json:"shop_id" validate:"required"`
	SubTotal           float64     `json:"sub_total" validate:"required"`
	TableID            string      `json:"table_id"`
	TableOrderMethodID int         `json:"table_order_method_id"`
	Tip                float64     `json:"tip" validate:"gte=0"`
	TipPercentage      float64     `json:"tip_percentage" validate:"gte=0"`
	TotalAmount        float64     `json:"total_amount" validate:"required"`
	TotalCash          float64     `json:"total_cash" validate:"gtefield=TotalAmount"`
	TotalBalance       float64     `json:"total_balance" validate:"gte=0"`
	TotalTax           float64     `json:"total_tax" validate:"gte=0"`
	TransactionID      string      `json:"transaction_id" validate:"required_unless=PaymentMethod CASH"`
	UserID             int         `json:"user_id" validate:"required"`
}

type OrderItem struct {
	DiscountAmount  float64           `json:"discount_amount" validate:"gte=0"`
	IsSale          bool              `json:"is_sale"`
	ItemID          int               `json:"item_id" validate:"required"`
	ItemName        string            `json:"item_name" validate:"required"`
	ModifierDetails []ModifierDetails `json:"modifierDetails" validate:"dive"`
	Note            string            `json:"note"`
	OriginalPrice   float64           `json:"original_price" validate:"gte=0"`
	PricePerItem    float64           `json:"price_per_item" validate:"required"`
	Quantity        int               `json:"quantity" validate:"required"`
	Tax             float64           `json:"tax" validate:"gte=0"`
	Total           float64           `json:"total" validate:"required"`
}

type ModifierDetails struct {
	ModifierGroupName string `json:"modifier_main" validate:"required"`
	ModifierMainItem  int    `json:"modifier_main_item" validate:"required"`
	Quantity          int    `json:"quantity" validate:"required"`
	ModifierItem      struct {
		ExternalItemID int     `json:"external_item_id" validate:"required"`
		ItemName       string  `json:"item_name" validate:"required"`
		Price          float64 `json:"price" validate:"required"`
	} `json:"modifierItem" validate:"required"`
}

/**************************/
