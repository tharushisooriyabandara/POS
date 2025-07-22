package service

import (
	"context"
	"database/sql"
	"errors"
	"strconv"
	"time"

	"github.com/Delivergate-Dev/pos-service-golang/config"
	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
	"github.com/Delivergate-Dev/pos-service-golang/logger"
	"go.uber.org/zap"
	"golang.org/x/crypto/bcrypt"
)

var (
	ErrAuthenticationFailed = errors.New("authentication failed")
	ErrInvalidToken         = errors.New("invalid token")
)

type IAuthService interface {
	Authenticate(ctx context.Context, tenantCode string, username, pin string) (string, string, error)
	Refresh(ctx context.Context, tenantCode string, refreshToken string) (string, string, error)
	InvalidateRefreshToken(ctx context.Context, tenantCode string, userID uint64) error
	ValidateAccessToken(ctx context.Context, tenantCode string, tokenString string) (*db.User, error)
}

// AuthService must implements IAuthService interface
var _ IAuthService = (*AuthService)(nil)

type AuthService struct {
	tenantManager TenantManager
	config        *config.Config
}

func NewAuthService(tenantManger TenantManager, cfg *config.Config) *AuthService {
	return &AuthService{
		tenantManager: tenantManger,
		config:        cfg,
	}
}

// Authenticate authenticates a user and returns an access token and a refresh token. it returns ErrAuthenticationFailed if the authentication fails.
func (s *AuthService) Authenticate(ctx context.Context, tenantCode string, userEmail, pin string) (string, string, error) {

	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		logger.Error("Failed to get tenant connection", zap.Error(err), zap.String("tenant", tenantCode))
		return "", "", err
	}
	queries := db.New(tenant)

	user, err := queries.GetUserByEmail(ctx, userEmail)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return "", "", ErrAuthenticationFailed
		}
		logger.Error("Failed to get user", zap.Error(err), zap.String("user_email", userEmail), zap.String("tenant", tenantCode))
		return "", "", err
	}

	if err := bcrypt.CompareHashAndPassword([]byte(user.Pin.String), []byte(pin)); err != nil {
		return "", "", ErrAuthenticationFailed
	}

	if user.Status.String != "Active" {
		return "", "", ErrAuthenticationFailed
	}

	return generateTokens(ctx, queries, user.ID, s.config)

}

// Refresh refreshes an access token and returns a new access token and a new refresh token. it returns ErrInvalidToken if the refresh token is invalid.
func (s *AuthService) Refresh(ctx context.Context, tenantCode string, refreshToken string) (string, string, error) {

	token, err := parse(refreshToken, []byte(s.config.JWTSecret))
	if err != nil {
		return "", "", ErrInvalidToken
	}

	if !token.Valid {
		return "", "", ErrInvalidToken
	}

	claims, ok := token.Claims.(*CustomClaims)
	if !ok {
		return "", "", ErrInvalidToken
	}

	if claims.TokenType != "refresh" {
		return "", "", ErrInvalidToken
	}

	userId, err := claims.GetSubject()
	if err != nil {
		return "", "", ErrInvalidToken
	}

	userID, err := strconv.ParseUint(userId, 10, 64)
	if err != nil {
		return "", "", ErrInvalidToken
	}

	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		logger.Error("Failed to get tenant connection", zap.Error(err), zap.String("tenant", tenantCode))
		return "", "", err
	}
	queries := db.New(tenant)

	currentRefreshTokenRecord, err := queries.GetRefreshToken(ctx, userID)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return "", "", ErrInvalidToken
		}
		return "", "", err
	}

	if currentRefreshTokenRecord.Token != refreshToken {
		return "", "", ErrInvalidToken
	}

	if currentRefreshTokenRecord.ExpiresAt.Before(time.Now()) {
		return "", "", ErrInvalidToken
	}

	// generate new tokens
	return generateTokens(ctx, queries, userID, s.config)

}

// ValidateAccessToken validates an access token and returns the user if the token is valid. it returns ErrInvalidToken if the token is invalid.
func (s *AuthService) ValidateAccessToken(ctx context.Context, tenantCode string, tokenString string) (*db.User, error) {

	token, err := parse(tokenString, []byte(s.config.JWTSecret))
	if err != nil {
		logger.Error("Failed to parse token", zap.Error(err), zap.String("token", tokenString))
		return nil, ErrInvalidToken
	}

	if !token.Valid {
		logger.Error("token.Valid failed", zap.Error(err), zap.String("token", tokenString))
		return nil, ErrInvalidToken
	}

	claims, ok := token.Claims.(*CustomClaims)
	if !ok {
		logger.Error("passing to custom claims failed", zap.Error(err), zap.String("token", tokenString))
		return nil, ErrInvalidToken
	}

	if claims.TokenType != "access" || claims.ExpiresAt.Before(time.Now()) {
		logger.Error("not access token", zap.Error(err), zap.String("token", tokenString))
		return nil, ErrInvalidToken
	}

	userID, err := strconv.ParseUint(claims.Subject, 10, 64)
	if err != nil {
		return nil, ErrInvalidToken
	}

	if userID == 0 {
		return nil, ErrInvalidToken
	}

	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		logger.Error("Failed to get tenant connection", zap.Error(err), zap.String("tenant", tenantCode))
		return nil, err
	}
	queries := db.New(tenant)

	user, err := queries.GetUserByID(ctx, userID)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, ErrInvalidToken
		}
		return nil, err
	}

	return &db.User{
		ID:       user.ID,
		Name:     user.Name,
		LastName: user.LastName,
		Email:    user.Email,
	}, nil
}

func (s *AuthService) InvalidateRefreshToken(ctx context.Context, tenantCode string, userID uint64) error {
	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		logger.Error("Failed to get tenant connection", zap.Error(err), zap.String("tenant", tenantCode))
		return err
	}
	queries := db.New(tenant)

	if err := queries.DeleteRefreshToken(ctx, userID); err != nil {
		return err
	}

	return nil
}

// generateTokens generates a new access token and a new refresh token, removing the old refresh token from the database.
func generateTokens(ctx context.Context, queries *db.Queries, userID uint64, config *config.Config) (string, string, error) {

	// delete old refresh token
	if err := queries.DeleteRefreshToken(ctx, userID); err != nil {
		return "", "", err
	}

	// generate new pair
	tokenPair, err := generateTokenPair(userID, config)
	if err != nil {
		return "", "", err
	}

	// save new refresh token to database
	expiredAt, err := tokenPair.refreshToken.Claims.GetExpirationTime()
	if err != nil {
		return "", "", err
	}

	if err := queries.CreateRefreshToken(ctx, &db.CreateRefreshTokenParams{
		UserID:    userID,
		Token:     tokenPair.refreshToken.Raw,
		ExpiresAt: expiredAt.Time,
	}); err != nil {
		return "", "", err
	}

	return tokenPair.accessToken.Raw, tokenPair.refreshToken.Raw, nil
}
