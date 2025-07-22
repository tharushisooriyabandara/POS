package database

import (
	"context"
	"database/sql"

	"github.com/Delivergate-Dev/pos-service-golang/logger"
	"go.uber.org/zap"
	"golang.org/x/sync/singleflight"
)

type DefaultTenantManager struct {
	pool        map[string]*sql.DB
	flightGroup *singleflight.Group
}

func NewDefaultTenantManager() *DefaultTenantManager {

	connectionPool := make(map[string]*sql.DB)

	// populate the connection pool
	// Note: intentionally ignoring errors
	tenants, _ := getTenants(context.Background())
	for _, tenant := range tenants {
		connectionPool[tenant.XTenantCode], _ = connectTenantDB(context.Background(), tenant.XTenantCode)
	}

	return &DefaultTenantManager{
		pool:        connectionPool,
		flightGroup: &singleflight.Group{},
	}
}

func (t *DefaultTenantManager) GetConnection(ctx context.Context, tenantCode string) (*sql.DB, error) {

	val, err, _ := t.flightGroup.Do(tenantCode, func() (interface{}, error) {
		// Check if connection already exists
		if queries, ok := t.pool[tenantCode]; ok {
			logger.Info("Using cached tenant connection", zap.String("tenant", tenantCode))
			return queries, nil
		}

		// Create a new connection
		logger.Info("Creating new tenant connection", zap.String("tenant", tenantCode))
		queries, err := connectTenantDB(ctx, tenantCode)
		if err != nil {
			return nil, err
		}

		t.pool[tenantCode] = queries
		return queries, nil
	})

	if err != nil {
		return nil, err
	}

	tenantDB := val.(*sql.DB)
	return tenantDB, nil
}
