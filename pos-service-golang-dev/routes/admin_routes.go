package routes

import (
	handlers "github.com/Delivergate-Dev/pos-service-golang/handlers/admin"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/gofiber/fiber/v2"
)

func setupAdminRoutes(router fiber.Router, services *service.ServiceRegistry) {
	migrationsHandler := handlers.NewMigrationsHandler(services.Migrations)
	admin := router.Group("/admin")

	admin.Post("/migrations", migrationsHandler.ApplyMigrations)
}
