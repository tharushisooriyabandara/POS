package middleware

import (
	"fmt"
	"strings"
	"time"

	"github.com/Delivergate-Dev/pos-service-golang/config"
	"github.com/gofiber/fiber/v2"
	"github.com/gofiber/fiber/v2/middleware/cache"
)

func UserCache(apiConfig *config.Config, cacheStore fiber.Storage) fiber.Handler {

	exp, _ := time.ParseDuration(apiConfig.CacheExpiry)

	config := cache.Config{
		Storage:      cacheStore,
		Expiration:   exp,
		Next:         cacheMethodPolicy,
		KeyGenerator: keyGenPolicy,
	}

	return cache.New(config)
}

func CategoryItemsCache(apiConfig *config.Config, cacheStore fiber.Storage) fiber.Handler {

	exp, _ := time.ParseDuration(apiConfig.CacheExpiry)

	config := cache.Config{
		Storage:      cacheStore,
		Expiration:   exp,
		Next:         cacheMethodPolicy,
		KeyGenerator: keyGenPolicy,
	}

	return cache.New(config)
}

// only cache GET requests
var cacheMethodPolicy = func(c *fiber.Ctx) bool {
	return c.Method() != fiber.MethodGet
}

// key generator
var keyGenPolicy = func(c *fiber.Ctx) string {
	tenant := c.Locals("tenant").(string)
	path := c.Path()

	// api/v1/users/1 -> users:1
	path = strings.Trim(path, "/")
	path = strings.TrimPrefix(path, "api/v1/")
	path = strings.ReplaceAll(path, "/", ":")

	// format <tenantCode>:<path>:<params>
	return fmt.Sprintf("%s:%s", tenant, path)
}
