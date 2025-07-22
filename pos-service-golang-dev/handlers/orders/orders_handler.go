package handlers

import (
	"context"
	"errors"

	"github.com/Delivergate-Dev/pos-service-golang/api"
	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/Delivergate-Dev/pos-service-golang/types"
	"github.com/Delivergate-Dev/pos-service-golang/validator"
	"github.com/gofiber/fiber/v2"
)

type orderService interface {
	CreateOrder(ctx context.Context, tenantCode string, orderRequest *types.CreateOrderRequest) error
	UpdateOrderStatus(ctx context.Context, tenantCode string, orderID uint64, status string) error
	GetOrders(ctx context.Context, tenantCode string, page, limit int) ([]*db.Order, error)
}

type OrdersHandler struct {
	orderService orderService
}

func NewOrdersHandler(service orderService) *OrdersHandler {
	return &OrdersHandler{
		orderService: service,
	}
}

func (h *OrdersHandler) CreateOrder(c *fiber.Ctx) error {

	var createOrderRequest types.CreateOrderRequest
	if err := c.BodyParser(&createOrderRequest); err != nil {
		return api.BadRequest("Invalid request body", err.Error())
	}

	if err := validator.Validate(&createOrderRequest); err != nil {
		return api.BadRequest("Validation failed", validator.TranslateErrors(err))
	}

	tenantCode := c.Locals("tenant").(string)
	err := h.orderService.CreateOrder(c.Context(), tenantCode, &createOrderRequest)
	if err != nil {
		if errors.Is(err, service.ErrOrderCreationFailed) {
			return api.BadRequest("Failed to create order", err.Error())
		}
		return err
	}

	return api.Ok(c, "Order created successfully", struct{}{})

}

func (h *OrdersHandler) UpdateOrderStatus(c *fiber.Ctx) error {
	orderID, err := c.ParamsInt("id")
	if err != nil || orderID == 0 {
		return api.BadRequest("Invalid order ID", err.Error())
	}

	var updateOrderStatusRequest types.UpdateOrderStatusRequest
	if err := c.BodyParser(&updateOrderStatusRequest); err != nil {
		return api.BadRequest("Invalid request body", err.Error())
	}

	if err := validator.Validate(&updateOrderStatusRequest); err != nil {
		return api.BadRequest("Validation failed", validator.TranslateErrors(err))
	}

	// update the order with,
	// TODO: order_status_timestamp table
	if err := h.orderService.UpdateOrderStatus(
		c.Context(),
		c.Locals("tenant").(string),
		uint64(orderID),
		updateOrderStatusRequest.Status,
	); err != nil {
		return err
	}

	return nil
}

func (h *OrdersHandler) GetOrders(c *fiber.Ctx) error {

	page := c.QueryInt("page")
	if page <= 0 {
		page = 1
	}

	limit := c.QueryInt("limit")
	if limit > 100 || limit <= 0 {
		limit = 100
	}

	orders, err := h.orderService.GetOrders(
		c.Context(),
		c.Locals("tenant").(string),
		page,
		limit,
	)
	if err != nil {
		return err
	}

	return api.Ok(c, "Orders fetched successfully", orders)
}
