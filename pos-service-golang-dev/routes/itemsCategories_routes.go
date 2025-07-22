package routes

import (
	"github.com/Delivergate-Dev/pos-service-golang/config"
	"github.com/Delivergate-Dev/pos-service-golang/database"
	handlers "github.com/Delivergate-Dev/pos-service-golang/handlers/items"
	"github.com/Delivergate-Dev/pos-service-golang/middleware"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/gofiber/fiber/v2"
)

func setupItemCategoriesRoutes(router fiber.Router, reg *service.ServiceRegistry, cfg *config.Config, cacheStore database.Cache) {
	itemsHandler := handlers.NewItemCategoriesHandler(reg.Menu)
	items := router.Group("/main-menu",
		middleware.TenantMiddleware,
		middleware.AuthMiddleware(reg.Auth),
		middleware.CategoryItemsCache(cfg, cacheStore),
	)

	items.Get("/:mainMenuId/categories/webshop-brand/:brandId/shop/:shopId", itemsHandler.GetItemCategories)
}
