// Code generated by sqlc. DO NOT EDIT.
// versions:
//   sqlc v1.29.0

package db

import (
	"context"
)

type Querier interface {
	CreateOrder(ctx context.Context, arg *CreateOrderParams) (int64, error)
	CreateOrderItem(ctx context.Context, arg *CreateOrderItemParams) (int64, error)
	CreateOrderItemModifier(ctx context.Context, arg *CreateOrderItemModifierParams) error
	CreateOrderPayments(ctx context.Context, arg *CreateOrderPaymentsParams) error
	CreateRefreshToken(ctx context.Context, arg *CreateRefreshTokenParams) error
	DeleteRefreshToken(ctx context.Context, userID uint64) error
	GetAvailableOrderItems(ctx context.Context, arg *GetAvailableOrderItemsParams) ([]*EntityDeliveryPlatform, error)
	GetCategoryItemsByMenuIdAndShopId(ctx context.Context, arg *GetCategoryItemsByMenuIdAndShopIdParams) (*WebshopMenu, error)
	GetDeliveryPlatform(ctx context.Context, arg *GetDeliveryPlatformParams) (*DeliveryPlatform, error)
	GetOrders(ctx context.Context, arg *GetOrdersParams) ([]*Order, error)
	GetRefreshToken(ctx context.Context, userID uint64) (*PosRefreshToken, error)
	GetShopByID(ctx context.Context, id uint64) (*Shop, error)
	GetTenantInfo(ctx context.Context, xTenantCode string) (*GetTenantInfoRow, error)
	GetTenants(ctx context.Context) ([]*Tenant, error)
	GetUserByEmail(ctx context.Context, email string) (*User, error)
	GetUserByID(ctx context.Context, id uint64) (*GetUserByIDRow, error)
	GetUsers(ctx context.Context) ([]*GetUsersRow, error)
	UpdateOrderStatus(ctx context.Context, arg *UpdateOrderStatusParams) error
}

var _ Querier = (*Queries)(nil)
