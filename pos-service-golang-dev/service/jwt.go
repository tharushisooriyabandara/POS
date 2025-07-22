package service

import (
	"strconv"
	"time"

	"github.com/Delivergate-Dev/pos-service-golang/config"
	"github.com/golang-jwt/jwt/v5"
)

type tokenPair struct {
	accessToken  *jwt.Token
	refreshToken *jwt.Token
}

type CustomClaims struct {
	TokenType string `json:"tokenType"`
	jwt.RegisteredClaims
}

func generateTokenPair(userID uint64, config *config.Config) (*tokenPair, error) {

	now := time.Now()
	key := []byte(config.JWTSecret)
	accessTokenExp, _ := time.ParseDuration(config.AccessTokenExp)
	refreshTokenExp, _ := time.ParseDuration(config.RefreshTokenExp)

	accessToken := jwt.NewWithClaims(jwt.SigningMethodHS256, CustomClaims{
		TokenType: "access",
		RegisteredClaims: jwt.RegisteredClaims{
			Subject:   strconv.FormatUint(userID, 10),
			IssuedAt:  jwt.NewNumericDate(now),
			ExpiresAt: jwt.NewNumericDate(now.Add(accessTokenExp)),
		}})

	signedAccessToken, err := accessToken.SignedString(key)
	if err != nil {
		return nil, err
	}

	accessToken, err = parse(signedAccessToken, key)
	if err != nil {
		return nil, err
	}

	refreshToken := jwt.NewWithClaims(jwt.SigningMethodHS256, CustomClaims{
		TokenType: "refresh",
		RegisteredClaims: jwt.RegisteredClaims{
			Subject:   strconv.FormatUint(userID, 10),
			IssuedAt:  jwt.NewNumericDate(now),
			ExpiresAt: jwt.NewNumericDate(now.Add(refreshTokenExp)),
		}})

	signedRefreshToken, err := refreshToken.SignedString(key)
	if err != nil {
		return nil, err
	}

	refreshToken, err = parse(signedRefreshToken, key)
	if err != nil {
		return nil, err
	}

	return &tokenPair{
		accessToken:  accessToken,
		refreshToken: refreshToken,
	}, nil

}

func parse(tokenString string, key []byte) (*jwt.Token, error) {
	token, err := jwt.ParseWithClaims(tokenString, &CustomClaims{}, func(t *jwt.Token) (interface{}, error) {
		return key, nil
	})
	if err != nil {
		return nil, err
	}

	return token, nil
}
