package service

import (
	"context"
	"database/sql"
	"encoding/json"
	"errors"
	"fmt"
	"os/exec"
	"strconv"
	"strings"
	"time"

	"github.com/Delivergate-Dev/pos-service-golang/config"
	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
	"github.com/Delivergate-Dev/pos-service-golang/logger"
	"go.uber.org/zap"
)

var (
	ErrShopNotFound             = errors.New("shop not found")
	ErrDeliveryPlatformNotFound = errors.New("delivery platform not found")
	ErrMainMenuNotFound         = errors.New("main menu not found")
)

type menu = *db.WebshopMenu

type ItemCategoryService struct {
	tenantManager TenantManager
	config        *config.Config
}

func NewItemCategoryService(tenantManager TenantManager, cfg *config.Config) *ItemCategoryService {
	return &ItemCategoryService{
		tenantManager: tenantManager,
		config:        cfg,
	}
}

func (s *ItemCategoryService) GetItemCategoriesByID(ctx context.Context, tenantCode string, shopId, brandId, mainMenuId uint64) (json.RawMessage, error) {
	tenant, err := s.tenantManager.GetConnection(ctx, tenantCode)
	if err != nil {
		logger.Error("Failed to get tenant connection", zap.Error(err), zap.String("tenant", tenantCode))
		return nil, err
	}
	queries := db.New(tenant)

	// get shop
	shop, err := queries.GetShopByID(ctx, shopId)
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, ErrShopNotFound
		}

		logger.Error("Failed to get shop", zap.Error(err), zap.String("tenant", tenantCode))
		return nil, err
	}

	// get delivery platform
	platformId, _ := strconv.Atoi(s.config.PosPlatformID)
	deliveryPlatform, err := queries.GetDeliveryPlatform(ctx, &db.GetDeliveryPlatformParams{
		OutletID:       sql.NullInt64{Int64: int64(shop.ID), Valid: true},
		WebshopBrandID: sql.NullInt32{Int32: int32(brandId), Valid: true},
		PlatformID:     sql.NullInt32{Int32: int32(platformId), Valid: true},
	})
	if err != nil {
		if errors.Is(err, sql.ErrNoRows) {
			return nil, ErrDeliveryPlatformNotFound
		}
		logger.Error("Failed to get delivery platform", zap.Error(err), zap.String("tenant", tenantCode))
		return nil, err
	}

	// Get current time in that timezone
	timeZone, err := time.LoadLocation(shop.Timezone)
	if err != nil {
		logger.Error("Failed to load timezone", zap.Error(err), zap.String("tenant", tenantCode))
		return nil, err
	}
	localTime := time.Now().In(timeZone)

	// get menu
	menu, err := getBestAvailableMenu(ctx, queries, localTime, shopId, mainMenuId, deliveryPlatform.ID)
	if err != nil && errors.Is(err, sql.ErrNoRows) {
		return nil, ErrMainMenuNotFound
	} else if err != nil {
		logger.Error("Failed to get menu", zap.Error(err), zap.String("tenant", tenantCode))
		return nil, err
	}

	// decode menu
	menuBytes, err := decodeMenu(menu.Menu.String)
	if err != nil {
		logger.Error("Failed to decode menu", zap.Error(err), zap.String("tenant", tenantCode))
		return nil, err
	}

	// return menu
	return menuBytes, nil

}

func getBestAvailableMenu(ctx context.Context, queries *db.Queries, timeNow time.Time, shopId, mainMenuId, deliveryPlatformId uint64) (menu, error) {

	partialParams := db.GetCategoryItemsByMenuIdAndShopIdParams{
		MainMenuID:         int32(mainMenuId),
		DeliveryPlatformID: int32(deliveryPlatformId),
		OutletID:           int32(shopId),
	}

	m, err := getMenuFromCurrentDay(ctx, queries, partialParams, timeNow)
	if err != nil && errors.Is(err, sql.ErrNoRows) {
		m, err = getMenuFromPreviousDay(ctx, queries, partialParams, timeNow)
		if err != nil && errors.Is(err, sql.ErrNoRows) {
			m, err = getAnyAvailableMenu(ctx, queries, partialParams, timeNow)
		}
	}

	return m, err

}

func getMenuFromCurrentDay(ctx context.Context, q *db.Queries, params db.GetCategoryItemsByMenuIdAndShopIdParams, timeNow time.Time) (menu, error) {

	// get menu from current day at same time slot
	params.Day = sql.NullString{String: timeNow.Weekday().String(), Valid: true}
	params.TimeFrom = sql.NullString{String: timeNow.Format(time.TimeOnly), Valid: true}
	params.TimeTo = sql.NullString{String: timeNow.Format(time.TimeOnly), Valid: true}

	return q.GetCategoryItemsByMenuIdAndShopId(ctx, &params)
}

func getMenuFromPreviousDay(ctx context.Context, q *db.Queries, params db.GetCategoryItemsByMenuIdAndShopIdParams, timeNow time.Time) (menu, error) {
	// get menu from previous day before current time
	params.Day = sql.NullString{String: timeNow.Add(-24 * time.Hour).Weekday().String(), Valid: true}
	params.TimeFrom = sql.NullString{}
	params.TimeTo = sql.NullString{String: timeNow.Format(time.TimeOnly), Valid: true}

	return q.GetCategoryItemsByMenuIdAndShopId(ctx, &params)

}

func getAnyAvailableMenu(ctx context.Context, q *db.Queries, params db.GetCategoryItemsByMenuIdAndShopIdParams, timeNow time.Time) (menu, error) {
	// get menu from today at any time
	params.Day = sql.NullString{}
	params.TimeFrom = sql.NullString{}
	params.TimeTo = sql.NullString{}

	return q.GetCategoryItemsByMenuIdAndShopId(ctx, &params)

}

func decodeMenu(menu string) ([]byte, error) {

	cmd := exec.Command("php", "-r", `
		$input = file_get_contents("php://stdin");
		$obj = unserialize($input);
		echo json_encode($obj);
	`)
	cmd.Stdin = strings.NewReader(menu)

	output, err := cmd.Output()
	if err != nil {
		return nil, fmt.Errorf("failed to execute php command: command output: %s : %w", string(output), err)
	}

	return output, nil
}
