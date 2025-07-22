package database

import (
	"context"
	"database/sql"
	"sync"

	"github.com/Delivergate-Dev/pos-service-golang/logger"
	"go.uber.org/zap"
)

// SimpleTenantManager manages tenant database connections
type SimpleTenantManager struct {
	connections map[string]*sql.DB
	mu          sync.RWMutex
}

// NewSimpleTenantManager creates a new tenant manager with the specified TTL
func NewSimpleTenantManager() *SimpleTenantManager {
	return &SimpleTenantManager{
		connections: make(map[string]*sql.DB),
	}
}

// GetConnection returns a cached connection or creates a new one
func (tm *SimpleTenantManager) GetConnection(ctx context.Context, tenantCode string) (*sql.DB, error) {
	// Try to get from cache first
	tm.mu.RLock()
	connection, exists := tm.connections[tenantCode]
	tm.mu.RUnlock()

	// If connection exists and is not expired, return it
	if exists {
		logger.Info("Using cached tenant connection", zap.String("tenant", tenantCode))
		return connection, nil
	}

	// Create a new connection
	tenantDB, err := connectTenantDB(ctx, tenantCode)
	if err != nil {
		return nil, err
	}

	// Cache the new connection
	tm.mu.Lock()
	tm.connections[tenantCode] = tenantDB
	tm.mu.Unlock()

	logger.Info("Created new tenant connection", zap.String("tenant", tenantCode))
	return tenantDB, nil
}
