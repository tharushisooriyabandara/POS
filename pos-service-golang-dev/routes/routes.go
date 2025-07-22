package routes

import (
	"github.com/Delivergate-Dev/pos-service-golang/config"
	"github.com/Delivergate-Dev/pos-service-golang/database"
	"github.com/Delivergate-Dev/pos-service-golang/logger"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/gofiber/fiber/v2"
	"github.com/gofiber/fiber/v2/middleware/cors"
	"github.com/gofiber/fiber/v2/middleware/recover"
)

// Setup configures all application routes
func Setup(app *fiber.App, services *service.ServiceRegistry, cfg *config.Config, cacheStore database.Cache) {

	// Setup CORS
	app.Use(cors.New())

	// panic recover
	app.Use(recover.New())

	// Setup base API
	router := app.Group("/api/v1")

	// Setup all route groups
	setupAdminRoutes(router, services)
	setupUserRoutes(router, services, cfg, cacheStore)
	setupAuthRoutes(router, services, cfg)
	setupItemCategoriesRoutes(router, services, cfg, cacheStore)
	setupOrderRoutes(router, services)

	// Setup health check endpoint
	app.Get("/", func(c *fiber.Ctx) error {
		logger.Info("Server is running")
		return c.SendString("Go-Fiber POS Service API is running")
	})
}
