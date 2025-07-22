package service

import (
	"context"
	"database/sql"

	"github.com/Delivergate-Dev/pos-service-golang/config"
	"github.com/Delivergate-Dev/pos-service-golang/database"
)

type TenantManager interface {
	GetConnection(ctx context.Context, tenantCode string) (*sql.DB, error)
}

// ServiceRegistry holds all application services
type ServiceRegistry struct {
	User       IUserService
	Auth       IAuthService
	Migrations IMigrationsService
	Menu       *ItemCategoryService
	Order      *OrderService
}

// NewServiceRegistry creates and returns a new instance of ServiceRegistry
func NewServiceRegistry(tenantManager TenantManager, cfg *config.Config, cacheStore database.Cache) *ServiceRegistry {
	return &ServiceRegistry{
		User:       NewUserService(tenantManager, cacheStore),
		Auth:       NewAuthService(tenantManager, cfg),
		Migrations: NewMigrationsService(),
		Menu:       NewItemCategoryService(tenantManager, cfg),
		Order:      NewOrderService(tenantManager),
	}
}
