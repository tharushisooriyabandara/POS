package handlers

import (
	"errors"
	"strconv"

	"github.com/Delivergate-Dev/pos-service-golang/api"
	"github.com/Delivergate-Dev/pos-service-golang/service"
	"github.com/Delivergate-Dev/pos-service-golang/types"
	"github.com/gofiber/fiber/v2"
)

type UserHandler struct {
	service service.IUserService
}

func NewUserHandler(service service.IUserService) *UserHandler {
	return &UserHandler{
		service: service,
	}
}

// GetUsers handles the GET /users endpoint
func (h *UserHandler) GetUsers(c *fiber.Ctx) error {

	tenantCode := c.Locals("tenant").(string)
	users, err := h.service.GetUsers(c.Context(), tenantCode)
	if err != nil {
		return err
	}

	userResponses := make([]types.GetUsersResponse, len(users))
	for i, user := range users {
		roleIdStr := strconv.Itoa(int(user.RoleID.Int32))
		userResponses[i] = types.GetUsersResponse{
			ID:        user.ID,
			FirstName: user.Name,
			LastName:  user.LastName.String,
			Email:     user.Email,
			Role:      user.RoleName.String,
			RoleId:    roleIdStr,
		}
	}

	return api.Ok(c, "Users fetched successfully", userResponses)
}

// GetUser handles the GET /users/:id endpoint
func (h *UserHandler) GetUser(c *fiber.Ctx) error {
	userID, err := c.ParamsInt("id")
	if err != nil || userID == 0 {
		return api.BadRequest("Invalid user ID", err.Error())
	}

	tenantCode := c.Locals("tenant").(string)
	user, err := h.service.GetUser(c.Context(), tenantCode, uint64(userID))
	if err != nil {
		if errors.Is(err, service.ErrUserNotFound) {
			return api.NotFound("User not found")
		}
		return err
	}

	resp := types.GetUsersResponse{
		ID:        user.ID,
		FirstName: user.Name,
		LastName:  user.LastName.String,
		Email:     user.Email,
		Role:      user.RoleName.String,
		RoleId:    strconv.Itoa(int(user.RoleID.Int32)),
	}
	return api.Ok(c, "User fetched successfully", resp)
}
