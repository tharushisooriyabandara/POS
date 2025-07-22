package routes

import (
	"github.com/Delivergate-Dev/pos-service-golang/config"
	"github.com/Delivergate-Dev/pos-service-golang/database"
	handlers "github.com/Delivergate-Dev/pos-service-golang/handlers/user"
	"github.com/Delivergate-Dev/pos-service-golang/middleware"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/gofiber/fiber/v2"
)

// SetupUserRoutes configures all user-related routes
func setupUserRoutes(router fiber.Router, services *service.ServiceRegistry, cfg *config.Config, cacheStore database.Cache) {
	userHandler := handlers.NewUserHandler(services.User)
	users := router.Group("/users",
		middleware.TenantMiddleware,
		middleware.UserCache(cfg, cacheStore),
	)

	users.Get("/", userHandler.GetUsers)
	users.Get("/:id", userHandler.GetUser)
}
