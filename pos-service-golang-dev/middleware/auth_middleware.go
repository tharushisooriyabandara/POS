package middleware

import (
	"errors"
	"strings"

	"github.com/Delivergate-Dev/pos-service-golang/api"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/gofiber/fiber/v2"
)

func AuthMiddleware(authService service.IAuthService) func(c *fiber.Ctx) error {
	return func(c *fiber.Ctx) error {
		auth := c.Get("Authorization")
		if auth == "" {
			return api.Unauthorized("Missing Authorization header")
		}

		if !strings.HasPrefix(auth, "Bearer ") {
			return api.Unauthorized("Invalid Authorization header")
		}

		tokenStr := strings.TrimPrefix(auth, "Bearer ")
		user, err := authService.ValidateAccessToken(c.Context(), c.Locals("tenant").(string), tokenStr)
		if err != nil {
			if errors.Is(err, service.ErrInvalidToken) {
				return api.Unauthorized("Invalid token")
			}
			return err
		}

		c.Locals("user", user)
		return c.Next()
	}
}
