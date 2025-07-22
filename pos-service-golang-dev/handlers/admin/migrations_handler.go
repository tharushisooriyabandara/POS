package handlers

import (
	"github.com/Delivergate-Dev/pos-service-golang/api"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/gofiber/fiber/v2"
)

type MigrationsHandler struct {
	migrationsService service.IMigrationsService
}

func NewMigrationsHandler(migrationsService service.IMigrationsService) *MigrationsHandler {
	return &MigrationsHandler{
		migrationsService: migrationsService,
	}
}

func (h *MigrationsHandler) ApplyMigrations(c *fiber.Ctx) error {
	step := c.QueryInt("step", 0)
	if err := h.migrationsService.ApplyMigrations(c.Context(), step); err != nil {
		return err
	}
	return api.Accepted[any](c, "Request accepted", struct{}{})
}
