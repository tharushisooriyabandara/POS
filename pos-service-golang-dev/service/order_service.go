package service

import (
	"context"
	"errors"
	"fmt"

	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
	"github.com/Delivergate-Dev/pos-service-golang/logger"
	"github.com/Delivergate-Dev/pos-service-golang/service/transaction"
	"github.com/Delivergate-Dev/pos-service-golang/types"
	"go.uber.org/zap"
)

var (
	ErrOrderCreationFailed = errors.New("failed to create order")
)

type OrderService struct {
	tenantManager TenantManager
}

func NewOrderService(tenantManager TenantManager) *OrderService {
	return &OrderService{
		tenantManager: tenantManager,
	}
}

func (s *OrderService) GetOrders(ctx context.Context, tenantCode string, page, limit int) ([]*db.Order, error) {
	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		logger.Error("Failed to get tenant connection", zap.Error(err), zap.String("tenant", tenantCode))
		return nil, err
	}
	queries := db.New(tenant)

	return queries.GetOrders(ctx, &db.GetOrdersParams{
		Limit:  int32(limit),
		Offset: int32((page - 1) * limit),
	})
}

func (s *OrderService) UpdateOrderStatus(ctx context.Context, tenantCode string, orderID uint64, status string) error {
	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		logger.Error("Failed to get tenant connection", zap.Error(err), zap.String("tenant", tenantCode))
		return err
	}
	queries := db.New(tenant)

	err = queries.UpdateOrderStatus(ctx, &db.UpdateOrderStatusParams{
		Status: status,
		ID:     orderID,
	})
	if err != nil {
		logger.Error("Failed to update order status", zap.Error(err), zap.String("tenant", tenantCode))
		return err
	}

	return nil
}

func (s *OrderService) CreateOrder(ctx context.Context, tenantCode string, orderRequest *types.CreateOrderRequest) error {

	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		logger.Error("Failed to get tenant connection", zap.Error(err), zap.String("tenant", tenantCode))
		return err
	}

	do, err := transaction.New[int64](ctx, tenant)
	if err != nil {
		return err
	}

	orderTransaction := transaction.Order(orderRequest)

	if _, err := do(orderTransaction); err != nil {
		return fmt.Errorf("%w: %v", ErrOrderCreationFailed, err)
	}

	return nil
}
