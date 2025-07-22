package handlers

import (
	"errors"

	"github.com/Delivergate-Dev/pos-service-golang/api"
	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/Delivergate-Dev/pos-service-golang/types"
	"github.com/Delivergate-Dev/pos-service-golang/validator"
	"github.com/gofiber/fiber/v2"
)

type AuthHandler struct {
	authService service.IAuthService
}

func NewAuthHandler(authService service.IAuthService) *AuthHandler {
	return &AuthHandler{
		authService: authService,
	}
}

// Login handles the POST /login endpoint
func (h *AuthHandler) Login(c *fiber.Ctx) error {

	var loginRequest types.LoginRequest
	if err := c.BodyParser(&loginRequest); err != nil {
		return api.BadRequest("Invalid request body")
	}

	if err := validator.Validate(&loginRequest); err != nil {
		return api.BadRequest("Validation failed", validator.TranslateErrors(err))
	}

	tenantCode := c.Locals("tenant").(string)
	accessToken, refreshToken, err := h.authService.Authenticate(c.Context(), tenantCode, loginRequest.Email, loginRequest.Pin)
	if err != nil {
		if errors.Is(err, service.ErrAuthenticationFailed) {
			return api.Unauthorized("Invalid credentials")
		}
		return err
	}

	return api.Ok(c, "Login successful", &types.LoginResponse{
		AccessToken:  accessToken,
		RefreshToken: refreshToken,
	})

}

func (h *AuthHandler) Refresh(c *fiber.Ctx) error {
	var refreshRequest types.RefreshRequest
	if err := c.BodyParser(&refreshRequest); err != nil {
		return api.BadRequest("Invalid request body")
	}

	if err := validator.Validate(&refreshRequest); err != nil {
		return api.BadRequest("Validation failed", validator.TranslateErrors(err))
	}

	tenantCode := c.Locals("tenant").(string)
	accessToken, refreshToken, err := h.authService.Refresh(c.Context(), tenantCode, refreshRequest.RefreshToken)
	if err != nil {
		if errors.Is(err, service.ErrInvalidToken) {
			return api.Unauthorized("Invalid token")
		}
		return err
	}

	return api.Ok(c, "Token refreshed", &types.LoginResponse{
		AccessToken:  accessToken,
		RefreshToken: refreshToken,
	})

}

func (h *AuthHandler) Logout(c *fiber.Ctx) error {
	user := c.Locals("user").(*db.User)
	tenantCode := c.Locals("tenant").(string)

	if err := h.authService.InvalidateRefreshToken(c.Context(), tenantCode, user.ID); err != nil {
		return err
	}

	return api.Ok(c, "Logout successful", struct{}{})

}
