package main

import (
	"net"

	"github.com/Delivergate-Dev/pos-service-golang/api"
	"github.com/Delivergate-Dev/pos-service-golang/config"
	"github.com/Delivergate-Dev/pos-service-golang/database"
	"github.com/Delivergate-Dev/pos-service-golang/logger"
	"github.com/Delivergate-Dev/pos-service-golang/routes"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/Delivergate-Dev/pos-service-golang/validator"
	"github.com/gofiber/fiber/v2"
	"github.com/gofiber/storage/memory/v2"
	"go.uber.org/zap"
)

func main() {
	// configs, loggers and validators
	cfg := config.MustLoad()
	logger.Init(cfg)
	validator.Init()

	// must connect db
	database.MustConnectMasterDB(cfg)

	// cache storage
	cacheStore := memory.New()

	// initialize services
	tenantManager := database.NewDefaultTenantManager()
	services := service.NewServiceRegistry(tenantManager, cfg, cacheStore)

	// Create Fiber app
	app := fiber.New(fiber.Config{
		ErrorHandler: api.ErrorHandler,
	})

	// Setup routes and middleware
	routes.Setup(app, services, cfg, cacheStore)

	// Start server
	app.Hooks().OnListen(func(listenData fiber.ListenData) error {
		logger.Info("Server is running",
			zap.String("host", listenData.Host),
			zap.String("port", listenData.Port),
		)
		return nil
	})

	if err := app.Listen(net.JoinHostPort(cfg.Host, cfg.Port)); err != nil {
		logger.Fatal("Failed to start server", zap.Error(err))
	}
}
