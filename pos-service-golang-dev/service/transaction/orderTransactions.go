package transaction

import (
	"bytes"
	"cmp"
	"context"
	"database/sql"
	"encoding/json"
	"fmt"
	"math/rand"
	"os/exec"
	"strconv"
	"time"

	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
	"github.com/Delivergate-Dev/pos-service-golang/logger"
	"github.com/Delivergate-Dev/pos-service-golang/types"
	"go.uber.org/zap"
)

func Order(orderRequest *types.CreateOrderRequest) queryFunc[int64] {
	return func(ctx context.Context, qtx *db.Queries) (int64, error) {
		if err := verifyItemAvailability(ctx, qtx, orderRequest); err != nil {
			return 0, err
		}

		orderID, err := orderTransaction(ctx, qtx, orderRequest)
		if err != nil {
			return 0, fmt.Errorf("failed to create order: %w", err)
		}

		err = orderPaymentsTransaction(ctx, qtx, orderRequest, orderID)
		if err != nil {
			return 0, fmt.Errorf("failed to create order payments: %w", err)
		}

		orderItemModifiers := make(map[int32][]types.ModifierDetails)
		err = orderItemsTransaction(ctx, qtx, orderRequest.OrderItems, orderID, orderItemModifiers)
		if err != nil {
			return 0, fmt.Errorf("failed to create order items: %w", err)
		}

		err = orderItemModifiersTransaction(ctx, qtx, orderItemModifiers)
		if err != nil {
			return 0, fmt.Errorf("failed to create order item modifiers: %w", err)
		}

		return orderID, nil
	}
}

func verifyItemAvailability(ctx context.Context, qtx *db.Queries, orderRequest *types.CreateOrderRequest) error {
	itemIDs := make([]string, 0, len(orderRequest.OrderItems))
	for _, item := range orderRequest.OrderItems {
		itemIDs = append(itemIDs, strconv.Itoa(item.ItemID))
	}

	availableItems, err := qtx.GetAvailableOrderItems(ctx, &db.GetAvailableOrderItemsParams{
		ItemIds:            itemIDs,
		DeliveryPlatformID: int32(orderRequest.DeliveryPlatformID),
	})
	if err != nil {
		return fmt.Errorf("failed to get available items: %w", err)
	}

	availableItemIDs := make(map[string]bool, len(availableItems))
	for _, item := range availableItems {
		availableItemIDs[item.ExternalItemID] = true
	}

	for _, item := range orderRequest.OrderItems {
		if !availableItemIDs[strconv.Itoa(item.ItemID)] {
			return fmt.Errorf("item %d is not available", item.ItemID)
		}
	}

	return nil
}

func orderTransaction(ctx context.Context, qtx *db.Queries, orderRequest *types.CreateOrderRequest) (int64, error) {
	return qtx.CreateOrder(ctx, &db.CreateOrderParams{
		CampaignCode:        sql.NullString{},
		CancelledByCustomer: false,
		CancelledReason:     sql.NullString{},
		CashDue:             "0.00",
		ContactAccessCode:   sql.NullString{},
		CreatedAt:           sql.NullTime{Time: time.Now(), Valid: true},
		CustomerID:          cmp.Or(int32(orderRequest.CustomerID), 99),
		CustomerName:        cmp.Or(orderRequest.CustomerName, "GUEST CUSTOMER"),
		DeliveryDateTime:    parseDeliveryDateTime(orderRequest.DeliveryDateTime),
		DeliveryLocationID:  sql.NullInt32{},
		DeliveryPlatformID:  int32(orderRequest.DeliveryPlatformID),
		DevicePlatform:      "dg_pos",
		Discount:            calculateAmount(orderRequest.Discount),
		DiscountType:        sql.NullString{String: orderRequest.DiscountType, Valid: orderRequest.DiscountType != ""},
		DisplayOrderID:      orderRequest.DisplayOrderID,
		IsScheduled:         false,
		IsTableOrder:        orderRequest.IsTableOrder,
		Note:                sql.NullString{String: orderRequest.OrderNote, Valid: orderRequest.OrderNote != ""},
		OrderDelayed:        false,
		OrderTypeID:         int32(4),
		PaymentMethod:       orderRequest.PaymentMethod,
		RemoteOrderID:       generateRemoteOrderID(len(orderRequest.OrderItems)),
		ShippingMethod:      orderRequest.ShippingMethod,
		ShippingTax:         sql.NullString{String: fmt.Sprintf("%.2f", orderRequest.ShippingTax), Valid: orderRequest.ShippingTax != 0},
		ShippingTotal:       sql.NullString{String: fmt.Sprintf("%.2f", orderRequest.ShippingTotal), Valid: orderRequest.ShippingTotal != 0},
		ShopID:              int32(orderRequest.ShopID),
		Status:              "QUEUE",
		SubTotal:            calculateAmount(orderRequest.SubTotal),
		Surcharge:           "0.00",
		TableID:             sql.NullString{String: orderRequest.TableID, Valid: orderRequest.TableID != ""},
		TableOrderMethodID:  sql.NullInt32{Int32: int32(orderRequest.TableOrderMethodID), Valid: orderRequest.TableOrderMethodID != 0},
		TestingOrder:        false,
		Tip:                 fmt.Sprintf("%.2f", orderRequest.Tip),
		TipPercentage:       fmt.Sprintf("%.2f", orderRequest.TipPercentage),
		TotalAmount:         calculateAmount(orderRequest.TotalAmount),
		TotalFee:            sql.NullInt32{},
		TotalTax:            sql.NullString{String: fmt.Sprintf("%.2f", orderRequest.TotalTax), Valid: orderRequest.TotalTax != 0},
		UniqueOrderID:       sql.NullString{},
		UpdatedAt:           sql.NullTime{Time: time.Now(), Valid: true},
		UserID:              int32(orderRequest.UserID),
		Vouchers:            sql.NullString{},
	})
}

func orderItemsTransaction(ctx context.Context, qtx *db.Queries, orderItems []types.OrderItem, orderID int64, orderItemModifiers map[int32][]types.ModifierDetails) error {
	for _, item := range orderItems {

		phpSerializedModifiers, err := generateModifierString(item.ModifierDetails)
		if err != nil {
			return err
		}

		// create order item
		orderItemID, err := qtx.CreateOrderItem(ctx, &db.CreateOrderItemParams{
			CategoryName:   sql.NullString{},
			DiscountAmount: calculateAmount(item.DiscountAmount),
			IsSale:         item.IsSale,
			ItemID:         strconv.Itoa(item.ItemID),
			ItemName:       item.ItemName,
			Modifiers:      sql.NullString{String: phpSerializedModifiers, Valid: true},
			Note:           sql.NullString{String: item.Note, Valid: item.Note != ""},
			OrderID:        int32(orderID),
			OriginalPrice:  cmp.Or(calculateAmount(item.OriginalPrice), calculateAmount(item.PricePerItem)),
			PricePerItem:   calculateAmount(item.PricePerItem),
			Quantity:       float64(item.Quantity),
			Status:         "QUEUE",
			Tax:            calculateAmount(item.Tax),
			Total:          calculateAmount(item.Total),
			CreatedAt:      sql.NullTime{Time: time.Now(), Valid: true},
			UpdatedAt:      sql.NullTime{Time: time.Now(), Valid: true},
		})
		if err != nil {
			return err
		}

		orderItemModifiers[int32(orderItemID)] = item.ModifierDetails
	}
	return nil
}

func orderItemModifiersTransaction(ctx context.Context, qtx *db.Queries, orderItemModifiers map[int32][]types.ModifierDetails) error {
	for orderItemID, modifierDetails := range orderItemModifiers {
		for _, modifier := range modifierDetails {
			err := qtx.CreateOrderItemModifier(ctx, &db.CreateOrderItemModifierParams{
				Amount:              strconv.Itoa(int(calculateAmount(modifier.ModifierItem.Price))),
				ModifierGroupName:   modifier.ModifierGroupName,
				ModifierID:          strconv.Itoa(modifier.ModifierMainItem),
				ModifierOptionID:    strconv.Itoa(modifier.ModifierItem.ExternalItemID),
				ModifierOptionName:  modifier.ModifierItem.ItemName,
				OrderItemID:         orderItemID,
				ParentModifierID:    sql.NullString{},
				Quantity:            strconv.Itoa(modifier.Quantity),
				SubParentModifierID: sql.NullString{},
				CreatedAt:           sql.NullTime{Time: time.Now(), Valid: true},
				UpdatedAt:           sql.NullTime{Time: time.Now(), Valid: true},
			})
			if err != nil {
				return err
			}
		}
	}
	return nil
}

func orderPaymentsTransaction(ctx context.Context, qtx *db.Queries, orderRequest *types.CreateOrderRequest, orderID int64) error {
	timeNow := time.Now()
	return qtx.CreateOrderPayments(ctx, &db.CreateOrderPaymentsParams{
		Amount:          fmt.Sprintf("%.2f", float64(calculateAmount(orderRequest.TotalAmount))),
		Cash:            fmt.Sprintf("%.2f", float64(calculateAmount(orderRequest.TotalCash))),
		Balance:         fmt.Sprintf("%.2f", float64(calculateAmount(orderRequest.TotalBalance))),
		DateTime:        sql.NullTime{Time: timeNow, Valid: true},
		OrderID:         int32(orderID),
		PaymentMethodID: mapPaymentMethod(orderRequest.PaymentMethod),
		Status:          sql.NullString{String: "CREATED", Valid: true},
		TransactionID: sql.NullString{
			String: orderRequest.TransactionID,
			Valid:  orderRequest.PaymentMethod == "CARD" && orderRequest.TransactionID != "",
		},
		RefundID:  sql.NullString{},
		IsRefund:  false,
		CreatedAt: sql.NullTime{Time: timeNow, Valid: true},
		UpdatedAt: sql.NullTime{Time: timeNow, Valid: true},
	})
}

func generateRemoteOrderID(itemsCount int) string {
	const charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
	date := time.Now().Format("020106") // DDMMYY
	randomStr := make([]byte, 12)
	for i := range randomStr {
		randomStr[i] = charset[rand.Intn(len(charset))]
	}
	return fmt.Sprintf("%s-%02d-%s-%s-%s", randomStr[:2], itemsCount, date, randomStr[2:8], randomStr[8:])
}

func parseDeliveryDateTime(timestamp string) time.Time {
	// date format is already validated therefore it is safe to ignore the error
	deliveryDateTime, _ := time.Parse(time.DateTime, timestamp)
	return deliveryDateTime
}

func calculateAmount(amount float64) int32 {
	return int32(amount * 100)
}

func mapPaymentMethod(paymentMethod string) string {
	if paymentMethod == "CARD" {
		return "2"
	}
	return "3"
}

type modifier struct {
	ItemID        int    `json:"item_id"`
	Title         string `json:"title"`
	SelectedItems []item `json:"selected_items"`
	RemovedItems  []item `json:"removed_items"`
}

type item struct {
	ItemID       int     `json:"item_id"`
	Title        string  `json:"title"`
	Quantity     float64 `json:"quantity"`
	PricePerItem string  `json:"price_per_item"`
	DisplayPrice string  `json:"display_price"`
}

func generateModifierString(modifierDetails []types.ModifierDetails) (string, error) {

	// map modifier details to modifier struct
	modifiers := []modifier{}
	for _, md := range modifierDetails {
		modifiers = append(modifiers, modifier{
			ItemID:       md.ModifierMainItem,
			Title:        md.ModifierGroupName,
			RemovedItems: []item{},
			SelectedItems: []item{
				{
					ItemID:       md.ModifierItem.ExternalItemID,
					Title:        md.ModifierItem.ItemName,
					Quantity:     float64(md.Quantity),
					PricePerItem: fmt.Sprintf("%.2f", float64(md.ModifierItem.Price)*100),
					DisplayPrice: fmt.Sprintf("%.2f", md.ModifierItem.Price),
				},
			},
		})
	}

	// fold modifiers by item id and combine selected and removed items
	acc := []modifier{}
	for _, m := range modifiers {
		exists := false
		for i, accM := range acc {
			if accM.ItemID == m.ItemID {
				exists = true
				acc[i].SelectedItems = append(acc[i].SelectedItems, m.SelectedItems...)
				acc[i].RemovedItems = append(acc[i].RemovedItems, m.RemovedItems...)
			}
		}
		if !exists {
			acc = append(acc, m)
		}
	}

	// marshal modifiers to json
	modifiersJSON, err := json.Marshal(acc)
	if err != nil {
		return "", fmt.Errorf("failed to marshal modifiers: %w", err)
	}

	serialized, err := phpSerialize(modifiersJSON)
	if err != nil {
		logger.Error("Failed to serialize modifiers", zap.Error(err))
		return "", err
	}

	return serialized, nil
}

func phpSerialize(jsonBytes json.RawMessage) (string, error) {

	cmd := exec.Command("php", "-r", `
		$input = file_get_contents("php://stdin");
		$array = json_decode($input, true);
		echo serialize($array);
	`)
	cmd.Stdin = bytes.NewReader(jsonBytes)

	output, err := cmd.Output()
	if err != nil {
		return "", fmt.Errorf("failed to execute php command: %w", err)
	}

	return string(output), nil

}
