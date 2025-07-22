package service

import (
	"context"
	"database/sql"
	"errors"

	"github.com/Delivergate-Dev/pos-service-golang/database"
	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
)

var (
	ErrUserNotFound = errors.New("user not found")
)

// IUserService defines the interface for user-related operations
type IUserService interface {
	GetUsers(ctx context.Context, tenantCode string) ([]*db.GetUsersRow, error)
	GetUser(ctx context.Context, tenantCode string, userID uint64) (*db.GetUserByIDRow, error)
	// Add more methods as needed
}

// userService must implement IUserService interface
var _ IUserService = (*userService)(nil)

// userService implements IUserService interface
type userService struct {
	tenantManager TenantManager
	cache         database.Cache
}

// NewUserService creates a new user service instance
func NewUserService(tenantManager TenantManager, cacheStore database.Cache) *userService {
	return &userService{
		tenantManager: tenantManager,
		cache:         cacheStore,
	}
}

// GetUsers retrieves all users from the database
func (s *userService) GetUsers(ctx context.Context, tenantCode string) ([]*db.GetUsersRow, error) {
	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		return nil, err
	}
	queries := db.New(tenant)

	users, err := queries.GetUsers(ctx)
	if err != nil {
		return nil, err
	}

	posUsers := []*db.GetUsersRow{}
	for _, user := range users {
		if user.RoleID.Int32 == 4 || user.RoleID.Int32 == 5 {
			posUsers = append(posUsers, user)
		}
	}

	return posUsers, nil
}

func (s *userService) GetUser(ctx context.Context, tenantCode string, userID uint64) (*db.GetUserByIDRow, error) {
	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		return nil, err
	}
	queries := db.New(tenant)

	user, err := queries.GetUserByID(ctx, userID)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, ErrUserNotFound
		}
		return nil, err
	}

	return user, nil
}
