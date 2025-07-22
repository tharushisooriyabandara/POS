package routes

import (
	handlers "github.com/Delivergate-Dev/pos-service-golang/handlers/orders"
	"github.com/Delivergate-Dev/pos-service-golang/middleware"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/gofiber/fiber/v2"
)

func setupOrderRoutes(router fiber.Router, reg *service.ServiceRegistry) {
	orderHandler := handlers.NewOrdersHandler(reg.Order)
	orders := router.Group("/orders",
		middleware.TenantMiddleware,
		middleware.AuthMiddleware(reg.Auth),
	)

	orders.Get("/", orderHandler.GetOrders)
	orders.Post("/", orderHandler.CreateOrder)
	orders.Patch("/:id/status", orderHandler.UpdateOrderStatus)
	// complete order endpoint (payment)
}
