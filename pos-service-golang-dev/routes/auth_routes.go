package routes

import (
	"github.com/Delivergate-Dev/pos-service-golang/config"
	handlers "github.com/Delivergate-Dev/pos-service-golang/handlers/auth"
	"github.com/Delivergate-Dev/pos-service-golang/middleware"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/gofiber/fiber/v2"
)

func setupAuthRoutes(router fiber.Router, services *service.ServiceRegistry, _ *config.Config) {
	authHandler := handlers.NewAuthHandler(services.Auth)
	auth := router.Group("/auth", middleware.TenantMiddleware)

	auth.Post("/login", authHandler.Login)
	auth.Post("/refresh", authHandler.Refresh)
	auth.Delete("/logout", middleware.AuthMiddleware(services.Auth), authHandler.Logout)
}
