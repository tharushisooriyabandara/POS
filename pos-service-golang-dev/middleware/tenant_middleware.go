package middleware

import (
	"github.com/gofiber/fiber/v2"
)

// TenantMiddleware is a middleware function that checks the tenant code in the request header and sets the database connection for the request context.
func TenantMiddleware(c *fiber.Ctx) error {
	tenantCode := c.Get("x-tenant-code")
	if tenantCode == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"error": "Missing x-tenant-code header",
		})
	}

	// Set the tenant code in the request context
	c.Locals("tenant", tenantCode)
	return c.Next()
}
