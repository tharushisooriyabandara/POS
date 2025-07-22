package database

import (
	"context"
	"database/sql"
	"embed"
	"fmt"
	"time"

	"github.com/Delivergate-Dev/pos-service-golang/config"
	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
	"github.com/Delivergate-Dev/pos-service-golang/logger"
	_ "github.com/go-sql-driver/mysql"
	"github.com/golang-migrate/migrate/v4"
	"github.com/golang-migrate/migrate/v4/database/mysql"
	_ "github.com/golang-migrate/migrate/v4/source/file"
	"github.com/golang-migrate/migrate/v4/source/iofs"
	"go.uber.org/zap"
)

var masterDB *sql.DB

//go:embed migrations/*.sql
var migrationFiles embed.FS

func MustConnectMasterDB(config *config.Config) {

	dsn := fmt.Sprintf("%s:%s@tcp(%s:%s)/%s?parseTime=true&multiStatements=true",
		config.MasterDbUsername,
		config.MasterDbPassword,
		config.MasterDBHost,
		config.MasterDBPort,
		config.MasterDBName,
	)

	var err error
	masterDB, err = sql.Open("mysql", dsn)
	if err != nil {
		panic(fmt.Errorf("failed to connect to master database: %w", err))
	}

	context, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	err = masterDB.PingContext(context)
	if err != nil {
		panic(fmt.Errorf("failed to ping master database: %w", err))
	}

	logger.Info("Successfully connected to master database")
}

func RunTenantMigrations(ctx context.Context, step int) error {
	tenants, err := getTenants(ctx)
	if err != nil {
		return err
	}

	for _, tenant := range tenants {
		tenantDB, err := connectTenantDB(ctx, tenant.XTenantCode)
		if err != nil {
			logger.Error("Failed to connect to tenant database", zap.Error(err))
			return err
		}

		// run
		driver, err := mysql.WithInstance(tenantDB, &mysql.Config{})
		if err != nil {
			logger.Error("Failed to create migrate driver", zap.Error(err))
			return err
		}

		d, err := iofs.New(migrationFiles, "migrations")
		if err != nil {
			logger.Error("Failed to create iofs driver", zap.Error(err))
			return err
		}

		m, err := migrate.NewWithInstance(
			"iofs",
			d,
			"mysql",
			driver,
		)
		if err != nil {
			logger.Error("Failed to create migrate instance", zap.Error(err))
			return err
		}

		if step < 0 {
			if err := m.Steps(step); err != nil && err != migrate.ErrNoChange {
				logger.Error("Failed to run migrations", zap.Error(err))
				return err
			}
		} else {
			if err := m.Up(); err != nil && err != migrate.ErrNoChange {
				logger.Error("Failed to run migrations", zap.Error(err))
				return err
			}
		}

	}

	return nil
}

func connectTenantDB(ctx context.Context, tenantCode string) (*sql.DB, error) {
	tenant, err := getTenantInfo(ctx, tenantCode)
	if err != nil {
		return nil, err
	}

	dsn := fmt.Sprintf("%s:%s@tcp(%s:%s)/%s?parseTime=true",
		tenant.Username,
		tenant.Password,
		tenant.Host,
		tenant.Port,
		tenant.DatabaseName,
	)

	tenantDB, err := sql.Open("mysql", dsn)
	if err != nil {
		logger.Error("Failed to connect to tenant database", zap.Error(err))
		return nil, fmt.Errorf("failed to connect to tenant database: %w", err)
	}

	tenantDB.SetMaxOpenConns(100)
	tenantDB.SetMaxIdleConns(10)
	tenantDB.SetConnMaxLifetime(time.Minute * 5)
	tenantDB.SetConnMaxIdleTime(time.Second * 60)

	logger.Info("Successfully connected to tenant database", zap.String("tenant", tenant.DatabaseName))

	return tenantDB, nil
}

func getTenantInfo(ctx context.Context, tenantCode string) (*db.GetTenantInfoRow, error) {
	tenant, err := db.New(masterDB).GetTenantInfo(ctx, tenantCode)
	if err != nil {
		if err == sql.ErrNoRows {
			return nil, fmt.Errorf("tenant not found for tenant code: %s", tenantCode)
		}
		logger.Error("Failed to get tenant info", zap.Error(err))
		return nil, err
	}

	return tenant, nil

}

func getTenants(ctx context.Context) ([]*db.Tenant, error) {
	tenants, err := db.New(masterDB).GetTenants(ctx)
	if err != nil {
		logger.Error("Failed to get tenants", zap.Error(err))
		return nil, err
	}

	return tenants, nil
}
